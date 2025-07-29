using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace JobMan.Storage.PostgreSql;

//TODO: SqlStorage options



internal class PostgreSqlNativeStorage : IWorkItemStorage
{
    private bool disposedValue;
    private object _locker = new object();

    internal NpgsqlConnection Connection { get; set; }
    internal IPostgreDmlCommandCreator DmlCommandCreator { get; set; }
    readonly ConcurrentDictionary<Guid, IWorkItemDefinition> _schedules = new ConcurrentDictionary<Guid, IWorkItemDefinition>();
    internal ConcurrentDictionary<Guid, IWorkItemDefinition> Schedules => _schedules;

    protected HashSet<IWorkPool> _directEnqueueCheckRegisteredWps = new HashSet<IWorkPool>();

    protected ILogger logger;

    public PostgreSqlNativeStorage(string connectionString)
    {
        this.logger = JobManGlobals.LoggerFactory.CreateLogger<PostgreSqlNativeStorage>();

        NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(connectionString);
        if (builder.MaxPoolSize == 0)
        {
            builder.MaxPoolSize = 100;
            builder.MinPoolSize = 10;
        }

        this.Connection = new NpgsqlConnection(builder.ToString());
        this.DmlCommandCreator = new PostgreDmlCommandCreator(this.Connection); //TODO: Transaction?

        this.CheckTables();
    }

    private void CheckTables()
    {
        this.logger.LogInformation($"PostgreSqlNativeStorage; checking schema");

        string sql = Properties.Resources.SchemaPostgreSql;
        string[] parts = sql.Split("GO;");

        this.CheckConnectionState();

        foreach (var sqlpart in parts)
        {
            using (NpgsqlCommand command = this.DmlCommandCreator.CreateCommand())
            {
                command.CommandText = sqlpart;
                command.ExecuteNonQuery();
            }
        }



    }

    internal void CheckConnectionState()
    {
        switch (this.Connection.State)
        {
            case ConnectionState.Closed:
            case ConnectionState.Broken:
                this.Connection.Open();
                break;
        }
    }

    protected IEnumerable<IWorkItemDefinition> CheckSchedules()
    {
        List<IWorkItemDefinition> availableItems = new List<IWorkItemDefinition>();
        DateTime now = JobManGlobals.Time.Now;
        IWorkItemDefinition[] definitions = _schedules.Values.ToArray();
        foreach (IWorkItemDefinition wid in definitions)
        {
            if (wid.Status == WorkItemStatus.WaitingProcess && wid.NextExecuteTime <= now)
            {
                IWorkItemDefinition clone = wid.Clone();
                clone.Type = WorkItemType.SingleRun;
                clone.Status = WorkItemStatus.Enqueued;
                this.Set(clone);

                wid.CalculateNextRun();

                availableItems.Add(clone);
            }
        }

        return availableItems;
    }

    internal DataTable GetTable(NpgsqlCommand command)
    {
        using (NpgsqlDataReader reader = command.ExecuteReader())
        {
            DataTable table = new DataTable();
            table.Load(reader);
            return table;
        }
    }

    internal DataSet GetTables(NpgsqlCommand command)
    {
        DataSet dataSet = new DataSet();
        using (NpgsqlDataAdapter adapter = new NpgsqlDataAdapter())
        {
            adapter.SelectCommand = command;
            adapter.Fill(dataSet);
            return dataSet;
        }
    }

    internal object GetValue(NpgsqlCommand command)
    {

        object obj = command.ExecuteScalar();
        if (Convert.IsDBNull(obj))
            obj = null;

        return obj;
    }

    public T GetValue<T>(NpgsqlCommand command)
    {
        object obj = this.GetValue(command);
        return (T)(obj ?? default(T));
    }

    internal void Execute(NpgsqlCommand command)
    {

        command.ExecuteNonQuery();
    }

    public void Clean()
    {
        lock (_locker)
        {
            this.logger.LogInformation($"PostgreSqlNativeStorage; clean");

            this.CheckConnectionState();
            using (var NpgsqlCommand = this.DmlCommandCreator.CreateClean(WorkItemStatus.Completed, JobManGlobals.Time.Now.AddDays(-1)))
            {
                this.Execute(NpgsqlCommand);
            }
        }
    }

    public StorageMetrics GetMetrics()
    {
        this.CheckConnectionState();

        DataSet dataSet;
        lock (_locker)
        {
            using (var NpgsqlCommand = this.DmlCommandCreator.CreateMetrics())
            {
                dataSet = this.GetTables(NpgsqlCommand);
            }
        }

        DataTable itemCountsTable = dataSet.Tables[0]; //"ItemCounts"

        StorageMetrics metrics = new StorageMetrics();

        foreach (DataRow row in itemCountsTable.Rows)
        {
            metrics[row.To<string>("Caption")] = Convert.ToInt32(row.To<long>("Count"));
        }

        metrics.TotalItemCount = (int)metrics.Get("ItemCount");
        metrics.WaitingItemCountOnStorate = (int)metrics.Get("WaitingItemCount");

        //Performance problem 01 ..
        //DataTable statCountsTable = dataSet.Tables[1]; //"ItemCounts"

        //foreach (DataRow row in statCountsTable.Rows)
        //{
        //    metrics.StatusCounts[row.To<WorkItemStatus>("StatusValue")] = row.To<int>("Count");
        //}

        return metrics;
    }

    //todo: this.CheckSchedules();

    public IWorkItemDefinition[] PeekOrWait(int count, string poolName, int waitTimeMs, CancellationToken cancellationToken)
    {
        if (count <= 0)
            count = 1;

        if (waitTimeMs < 500)
            waitTimeMs = 500;

        List<IWorkItemDefinition> definitions = new List<IWorkItemDefinition>();

        do
        {
            definitions.AddRange(this.CheckSchedules());

            this.CheckConnectionState();

            DataTable table;
            lock (_locker)
            {
                using (var NpgsqlCommand = this.DmlCommandCreator.CreatePeek(count, poolName))
                {
                    table = this.GetTable(NpgsqlCommand);
                }
            }

            definitions.AddRange(table.MapToWorkItemDefinitions(this, logger));

            if (definitions.Count == 0)
                cancellationToken.WaitHandle.WaitOne(waitTimeMs); //TODO: Set signal !!
        }
        while (definitions.Count == 0 && !cancellationToken.IsCancellationRequested);

        return definitions.ToArray();
    }

    protected bool DoItemAddedToStorage(IWorkItemDefinition workItemDefinition)
    {
        foreach (var workPool in _directEnqueueCheckRegisteredWps)
        {
            if (workItemDefinition.Pool == workPool.Name)
            {
                bool taken = workPool.CanEnqueueDirect(workItemDefinition);
#if DEBUG
                this.logger.LogDebug($"PostgreSqlNativeStorage; Direct taken; {workItemDefinition.Id} / {workItemDefinition.Data.MethodName}: {taken}");
#endif

                return taken;
            }
        }


        return false;
    }

    public void Set(IWorkItemDefinition workItemDefinition)
    {
#if DEBUG
        this.logger.LogDebug($"PostgreSqlNativeStorage; Set: {workItemDefinition.Id}, {workItemDefinition.Type}, {workItemDefinition.Status}");
#endif

        switch (workItemDefinition.Type)
        {
            case WorkItemType.SingleRun:

                if (workItemDefinition.Status == WorkItemStatus.WaitingProcess && this.DoItemAddedToStorage(workItemDefinition))
                {
                    workItemDefinition.Status = WorkItemStatus.Enqueued;
                }

                this.CheckConnectionState();

                lock (_locker) //TODO: lock with timeout
                {
                    using (var NpgsqlCommand = this.DmlCommandCreator.CreateInsert(workItemDefinition))
                    {
                        long id = this.GetValue<long>(NpgsqlCommand);
                        workItemDefinition.Id = id;
                    }
                }
                break;
            case WorkItemType.RecurrentRun:
                workItemDefinition.Schedule = Guid.NewGuid();
                _schedules.TryAdd(workItemDefinition.Schedule, workItemDefinition);

                break;
        }
    }

    public void UpdateStatus(IWorkItemDefinition workItemDefinition)
    {
        this.CheckConnectionState();

        lock (_locker)
        {
#if DEBUG
            this.logger.LogDebug($"PostgreSqlNativeStorage; UpdateStatus: {workItemDefinition.Id}, {workItemDefinition.Type}, {workItemDefinition.Status}");
#endif


            using (var NpgsqlCommand = this.DmlCommandCreator.CreateUpdateStatus(workItemDefinition))
            {
                this.Execute(NpgsqlCommand);
            }
        }

        switch (workItemDefinition.Type)
        {
            case WorkItemType.RecurrentRun:
                if (string.IsNullOrWhiteSpace(workItemDefinition.Cron))
                {
                    switch (workItemDefinition.Status)
                    {
                        case WorkItemStatus.Fail:
                        case WorkItemStatus.Completed:
                            IWorkItemDefinition scheduleItem = _schedules.Get(workItemDefinition.Schedule);
                            scheduleItem.LastExecuteTime = workItemDefinition.LastExecuteTime;
                            scheduleItem.CalculateNextRun();
                            scheduleItem.Status = WorkItemStatus.WaitingProcess;
                            break;
                    }
                }
                break;
        }
    }

    public void RegisterDirectEnqueueCheck(IWorkPool workPool)
    {
        _directEnqueueCheckRegisteredWps.Add(workPool);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            this.Connection.Close();
            this.Connection.Dispose();

            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~PostgreSqlStorage()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }


    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

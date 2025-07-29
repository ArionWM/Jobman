using Microsoft.Extensions.Logging;
using JobMan.Storage.MemoryStorage;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace JobMan;

public class WorkServer : IWorkServer
{
    private bool disposed;


    protected CancellationTokenSource managementThreadCancellationTokenSource;
    protected Thread managementThread;
    protected ILogger<WorkServer> logger;
    protected List<IWorkPool> pools { get; } = new List<IWorkPool>();

    public IWorkPool[] Pools => this.pools.ToArray();
    public IWorkItemStorage[] Storages { get; private set; }

    public string Name { get; protected set; }
    public WorkServerStatus Status { get; protected set; } = WorkServerStatus.Terminated;

    public IWorkServerOptions Options { get; protected set; }

    public WorkServerMetrics Metrics { get; protected set; }

    public WorkServer(IWorkServerOptions options, ILogger<WorkServer> logger)
    {
        this.Name = Environment.MachineName;
        //this._logger = logger;
        this.Options = options;
        this.Metrics = new WorkServerMetrics(this.Name);
        this.logger = logger;

        this.Setup();
    }

    protected void AddWorkPool(IWorkPoolOptions poolOptions)
    {
        IWorkPool workPool = this.Options.WorkPoolFactory.Create(this, poolOptions);
        this.pools.Add(workPool);
        workPool.MetricsUpdated += WorkPool_MetricsUpdated;
    }

    internal IWorkPool GetPool(string name)
    {
        return this.pools.FirstOrDefault(pl => pl.Name == name);
    }

    protected void Setup()
    {
        for (int i = 0; i < this.Options.PoolOptions.Count; i++)
        {
            IWorkPoolOptions poolOptions = this.Options.PoolOptions[i];
            if (string.IsNullOrEmpty(poolOptions.Name))
                poolOptions.Name = $"Pool #{i + 1}";

            this.AddWorkPool(poolOptions);
        }

        this.Options.UseInMemoryStorage();

        this.Storages = this.pools.Select(pool => pool.Options.Storage).Distinct().ToArray();

        if (!string.IsNullOrWhiteSpace(this.Options.CleanJobsScheduleCron))
            this.Schedule(() => HelperJobContainer.Clean(), this.Options.CleanJobsScheduleCron);

        this.managementThread = new Thread(this.ManagementThread);

    }

    protected void StartInternal()
    {
        List<Task> tasks = new List<Task>();
        foreach (IWorkPool workPool in this.pools)
            tasks.Add(workPool.StartAsync());

        var _this = this;

        this.managementThreadCancellationTokenSource = new CancellationTokenSource();

        _ = Task.Run(() =>
        {
            _this.managementThread.Start(this.managementThreadCancellationTokenSource.Token);
        });

        this.Status = WorkServerStatus.Active;
        this.UpdateMetrics();

        Task.WhenAll(tasks.ToArray());
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        //TODO: Implement CancellationToken cancellationToken
        if (this.Status == WorkServerStatus.Active)
            throw new InvalidOperationException("WorkServer is already running.");

        if (this.Options.DelayedStart)
        {
            await Task.Delay(this.Options.DelayedStartMilliseconds).ContinueWith(t => this.StartInternal());
        }
        else
        {
            this.StartInternal();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        //TODO: Implement CancellationToken cancellationToken

        if (this.Status == WorkServerStatus.Stopped || this.Status == WorkServerStatus.Terminated)
            throw new InvalidOperationException("WorkServer is already stopped or terminated.");

        this.Status = WorkServerStatus.WaitingStop;

        List<Task> tasks = new List<Task>();
        foreach (IWorkPool workPool in this.pools)
            tasks.Add(workPool.StopAsync());

        tasks.Add(
            Task.Run(() =>
            {
                this.managementThreadCancellationTokenSource.Cancel();
                this.managementThread.Join();
            }));

        await Task.WhenAll(tasks.ToArray());
        this.Status = WorkServerStatus.Stopped;
        this.UpdateMetrics();
    }

    public void Schedule(string workPoolName, Expression<Action> action, string cronExpression, string tag = null)
    {
        IWorkItemDefinition workItemDefinition = this.Options.WorkItemDefinitionFactory.Create(action, cronExpression, workPoolName, tag);
        workItemDefinition.Type = WorkItemType.RecurrentRun;
        workItemDefinition.CalculateNextRun();

        IWorkPool workPool = this.GetPool(workItemDefinition.Pool);
        workPool.Options.Storage.Set(workItemDefinition);
    }

    public void Enqueue(string workPoolName, Expression<Action> action, string tag = null)
    {
        IWorkItemDefinition workItemDefinition = this.Options.WorkItemDefinitionFactory.Create(action, workPoolName, tag);

        IWorkPool workPool = this.GetPool(workItemDefinition.Pool);
        workPool.Options.Storage.Set(workItemDefinition);
    }

    public void Enqueue(string workPoolName, Expression<Action> action, TimeSpan runAfter, string tag = null)
    {
        IWorkItemDefinition workItemDefinition = this.Options.WorkItemDefinitionFactory.Create(action, runAfter, workPoolName, tag);

        IWorkPool workPool = this.GetPool(workItemDefinition.Pool);
        workPool.Options.Storage.Set(workItemDefinition);
    }


    protected void WorkPool_MetricsUpdated(IWorkPool pool)
    {
        ProcessDataSample sample = pool.Metrics.GetLast();
        _ = this.Metrics.Add(pool, sample);
    }

    protected void UpdateMetrics()
    {
        try
        {
            int waitingInStorage = 0;
            int waitingItemCountOnBuffer = 0;
            foreach (var storage in this.Storages)
            {
                StorageMetrics sm = storage.GetMetrics();
                waitingInStorage += sm.WaitingItemCountOnStorate;
                waitingItemCountOnBuffer += sm.WaitingItemCountOnBuffer;
            }

            this.Metrics.UpdateGlobalLive(waitingInStorage, waitingItemCountOnBuffer);
            this.Metrics.PoolCount = this.pools.Count;
            this.Metrics.WorkerCount = this.pools.Select(pool => pool.Metrics.WorkerCount).Sum();
        }
        catch (Exception ex)
        {
            //Do nothing
            this.logger.LogError(ex, "UpdateMetrics");
        }
    }

    protected virtual void ManagementThread(object token)
    {
        CancellationToken cancellationToken = (CancellationToken)token;
        try
        {
            while (true)
            {
                try
                {
                    cancellationToken.WaitHandle.WaitOne(4000);
                    cancellationToken.ThrowIfCancellationRequested();
                    this.UpdateMetrics();
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "ManagementThread");
                    Thread.Sleep(20000);
                }
            }
        }
        catch (OperationCanceledException) { }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                
            }

            //TODO: dispose sub objects


            disposed = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~WorkServer()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}

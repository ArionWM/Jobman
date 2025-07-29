using Microsoft.Data.SqlClient;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan.Storage.SqlServer;

internal class DmlCommandCreator : IDmlCommandCreator
//TODO: Configuration? + schema name = 'dbo'
//TODO: Locks + isolation 
{
    protected readonly SqlConnection connection;
    protected readonly SqlTransaction transaction;

    public DmlCommandCreator(SqlConnection connection)
    {
        this.connection = connection;
    }

    public DmlCommandCreator(SqlConnection connection, SqlTransaction transaction) : this(connection)
    {
        this.transaction = transaction;
    }

    protected object SqlValue(DateTime dateTime)
    {
        if (dateTime < JobmanHelperExtensions.MinDateTime)
            return Convert.DBNull;

        return dateTime;
    }

    protected object SqlValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return Convert.DBNull;
        return value;
    }

    protected object SqlValue(object value)
    {
        if (value == null)
            return Convert.DBNull;
        return value;
    }

    public SqlCommand CreateCommand()
    {
        SqlCommand command = connection.CreateCommand();
        command.Transaction = this.transaction;
        //command.CommandTimeout = this.CommandTimeout;
        return command;
    }

    public virtual SqlCommand CreateInsert(IWorkItemDefinition workItemDefinition)
    {
        string sql =
            @"INSERT INTO [jm_jobs] 
                    ([Type]
                    ,[Schedule]
                    ,[Pool]                    
                    ,[Cron]
                    ,[LastExecuteTime]
                    ,[NextExecuteTime]
                    ,[Status]
                    ,[Data]
                    ,[Tag])
                VALUES
                    (
                    @type
                    ,@schedule
                    ,@pool
                    ,@cron
                    ,@lastExecuteTime
                    ,@nextExecuteTime
                    ,@status
                    ,@data
                    ,@tag);

                SELECT cast( SCOPE_IDENTITY() as bigint);
                ";


        SqlCommand command = this.CreateCommand();
        command.CommandText = sql;

        command.Parameters.Add("@type", System.Data.SqlDbType.Int).Value = SqlValue(workItemDefinition.Type);
        command.Parameters.Add("@schedule", System.Data.SqlDbType.UniqueIdentifier).Value = SqlValue(workItemDefinition.Schedule);
        command.Parameters.Add("@pool", System.Data.SqlDbType.NVarChar).Value = SqlValue(workItemDefinition.Pool);
        command.Parameters.Add("@cron", System.Data.SqlDbType.NVarChar).Value = SqlValue(workItemDefinition.Cron);
        command.Parameters.Add("@lastExecuteTime", System.Data.SqlDbType.DateTime).Value = SqlValue(workItemDefinition.LastExecuteTime);
        command.Parameters.Add("@nextExecuteTime", System.Data.SqlDbType.DateTime).Value = SqlValue(workItemDefinition.NextExecuteTime);
        command.Parameters.Add("@status", System.Data.SqlDbType.Int).Value = SqlValue(workItemDefinition.Status);

        string dataJson = JobManGlobals.WorkServerOptions.WorkItemDefinitionSerializer.ToJson(workItemDefinition.Data);
        command.Parameters.Add("@data", System.Data.SqlDbType.NVarChar).Value = SqlValue(dataJson);
        command.Parameters.Add("@tag", System.Data.SqlDbType.NVarChar).Value = SqlValue(workItemDefinition.Tag);

        return command;
    }

    public virtual SqlCommand CreateUpdateStatus(IWorkItemDefinition workItemDefinition)
    {
        string sql =
            @"
                  UPDATE [jm_jobs] 
                  SET
                    lastExecuteTime = @lastExecuteTime,
                    nextExecuteTime = @nextExecuteTime,
					processTimeMs = @processTimeMs,
                    retryCount = @retryCount,
                    [Description] = @description,
                    status = @status
                  WHERE
                    Id = @id;

                   select @@ROWCOUNT;
                ";


        SqlCommand command = this.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("@lastExecuteTime", System.Data.SqlDbType.DateTime).Value = SqlValue(workItemDefinition.LastExecuteTime);
        command.Parameters.Add("@nextExecuteTime", System.Data.SqlDbType.DateTime).Value = SqlValue(workItemDefinition.NextExecuteTime);
        command.Parameters.Add("@processTimeMs", System.Data.SqlDbType.BigInt).Value = SqlValue(workItemDefinition.ProcessTimeMs);
        command.Parameters.Add("@retryCount", System.Data.SqlDbType.Int).Value = SqlValue(workItemDefinition.RetryCount);
        command.Parameters.Add("@description", System.Data.SqlDbType.NVarChar).Value = SqlValue(workItemDefinition.Description)?.ToString().Crop(200, true);
        command.Parameters.Add("@status", System.Data.SqlDbType.Int).Value = SqlValue(workItemDefinition.Status);
        command.Parameters.Add("@id", System.Data.SqlDbType.BigInt).Value = SqlValue(workItemDefinition.Id);

        return command;
    }

    public virtual SqlCommand CreateClean(WorkItemStatus status, DateTime maxTimeUtc)
    {
        string sql =
            @"delete from [jm_jobs] 
                where [Status] = @status and LastExecuteTime <= @lastExecuteTime;

                select @@ROWCOUNT;
                ";
        SqlCommand command = this.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("@status", System.Data.SqlDbType.Int).Value = SqlValue(status);
        command.Parameters.Add("@lastExecuteTime", System.Data.SqlDbType.DateTime).Value = SqlValue(maxTimeUtc);

        return command;
    }

    public virtual SqlCommand CreatePeek(int recordCount, string poolName)
    {
        string sql =
            @"

                begin tran
	                select top(@tcount) Id
	                into #tmp_ids
	                from jm_jobs with(holdlock)
	                where [Status] = 10 and [Pool] = @pool and [NextExecuteTime] <= @nextExecuteTime

	                update jm_jobs set [status] = 14 /*Enqueuing*/ where Id in (select Id from #tmp_ids)

	                select * from jm_jobs
                    inner join #tmp_ids as tid on tid.Id = jm_jobs.Id

                commit
                ";

        //where Id in (select Id from #tmp_ids)

        SqlCommand command = this.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("@tcount", System.Data.SqlDbType.Int).Value = SqlValue(recordCount);
        command.Parameters.Add("@pool", System.Data.SqlDbType.NVarChar).Value = SqlValue(poolName);
        command.Parameters.Add("@nextExecuteTime", System.Data.SqlDbType.DateTime).Value = SqlValue(JobManGlobals.Time.Now);
        //command.Parameters.Add("@status", System.Data.SqlDbType.Int).Value = SqlValue(WorkItemStatus.WaitingProcess);
        return command;
    }

    public SqlCommand CreateMetrics()
    {
        string sql =
          @"
                select 'ItemCount' as [Caption], SUM(row_count) as [Count]  FROM sys.dm_db_partition_stats WHERE object_id=OBJECT_ID('jm_jobs') AND (index_id=0 or index_id=1)
                union
                select 'WaitingItemCount' as [Caption], count(*) as [Count] from [jm_jobs] as ItemCounts with(nolock) where [Status] = 10 /*WaitingProcess*/;

                ";

        /*
         * //Performance problem 01..
        select jobs.Status as [StatusValue], count(*) as [Count] 
            from [jm_jobs] as jobs with(nolock)
            group by jobs.Status


         select statVals.Name as [Status], statVals.Value as [StatusValue], count(*) as [Count] 
            from [jm_jobs] as jobs with(nolock)
            inner join jm_enums_WorkItemStatuses as statVals with(nolock) on statVals.Value = jobs.Status
            group by statVals.Name, statVals.Value
         */

        SqlCommand command = this.CreateCommand();
        command.CommandText = sql;
        return command;
    }
}

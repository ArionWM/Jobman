using System.Text.Json;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan.Storage.PostgreSql;

internal class PostgreDmlCommandCreator : IPostgreDmlCommandCreator
//TODO: Configuration? + schema name 
//TODO: Locks + isolation 
{
    protected readonly NpgsqlConnection connection;
    protected readonly NpgsqlTransaction transaction;

    public PostgreDmlCommandCreator(NpgsqlConnection connection)
    {
        this.connection = connection;
    }

    public PostgreDmlCommandCreator(NpgsqlConnection connection, NpgsqlTransaction transaction) : this(connection)
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

    public NpgsqlCommand CreateCommand()
    {
        NpgsqlCommand command = connection.CreateCommand();
        command.Transaction = this.transaction;
        //command.CommandTimeout = this.CommandTimeout;
        return command;
    }

    public virtual NpgsqlCommand CreateInsert(IWorkItemDefinition workItemDefinition)
    {
        string sql =
            @"INSERT INTO public.jm_jobs 
                    (Type
                    ,Schedule
                    ,Pool                    
                    ,Cron
                    ,LastExecuteTime
                    ,NextExecuteTime
                    ,Status
                    ,Data
                    ,Tag)
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

                SELECT lastval();
                ";

        

        NpgsqlCommand command = this.CreateCommand();
        command.CommandText = sql;



        command.Parameters.Add("@type", NpgsqlDbType.Integer).Value = (int)SqlValue(workItemDefinition.Type);
        command.Parameters.Add("@schedule", NpgsqlDbType.Uuid).Value = SqlValue(workItemDefinition.Schedule);
        command.Parameters.Add("@pool", NpgsqlDbType.Varchar).Value = SqlValue(workItemDefinition.Pool);
        command.Parameters.Add("@cron", NpgsqlDbType.Varchar).Value = SqlValue(workItemDefinition.Cron);
        command.Parameters.Add("@lastExecuteTime", NpgsqlDbType.Timestamp).Value = SqlValue(workItemDefinition.LastExecuteTime);
        command.Parameters.Add("@nextExecuteTime", NpgsqlDbType.Timestamp).Value = SqlValue(workItemDefinition.NextExecuteTime);
        command.Parameters.Add("@status", NpgsqlDbType.Integer).Value = (int)SqlValue(workItemDefinition.Status);

        string dataJson = JobManGlobals.WorkServerOptions.WorkItemDefinitionSerializer.ToJson(workItemDefinition.Data);
        command.Parameters.Add("@data", NpgsqlDbType.Text).Value = SqlValue(dataJson);
        command.Parameters.Add("@tag", NpgsqlDbType.Varchar).Value = SqlValue(workItemDefinition.Tag);

        return command;
    }

    public virtual NpgsqlCommand CreateUpdateStatus(IWorkItemDefinition workItemDefinition)
    {
        string sql =
            @"
                  UPDATE public.jm_jobs
                  SET
                    lastExecuteTime = @lastExecuteTime,
                    nextExecuteTime = @nextExecuteTime,
					processTimeMs = @processTimeMs,
                    retryCount = @retryCount,
                    Description = @description,
                    status = @status
                  WHERE
                    Id = @id;
                ";

        

        NpgsqlCommand command = this.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("@lastExecuteTime", NpgsqlDbType.Timestamp).Value = SqlValue(workItemDefinition.LastExecuteTime);
        command.Parameters.Add("@nextExecuteTime", NpgsqlDbType.Timestamp).Value = SqlValue(workItemDefinition.NextExecuteTime);
        command.Parameters.Add("@processTimeMs", NpgsqlDbType.Bigint).Value = SqlValue(workItemDefinition.ProcessTimeMs);
        command.Parameters.Add("@retryCount", NpgsqlDbType.Integer).Value = SqlValue(workItemDefinition.RetryCount);
        command.Parameters.Add("@description", NpgsqlDbType.Text).Value = SqlValue(workItemDefinition.Description)?.ToString().Crop(200, true);
        command.Parameters.Add("@status", NpgsqlDbType.Integer).Value = (int)SqlValue(workItemDefinition.Status);
        command.Parameters.Add("@id", NpgsqlDbType.Bigint).Value = SqlValue(workItemDefinition.Id);

        return command;
    }

    public virtual NpgsqlCommand CreateClean(WorkItemStatus status, DateTime maxTimeUtc)
    {
        string sql =
            @"delete from public.jm_jobs
                where Status = @status and LastExecuteTime <= @lastExecuteTime;
                ";
        NpgsqlCommand command = this.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("@status", NpgsqlDbType.Integer).Value = (int)SqlValue(status);
        command.Parameters.Add("@lastExecuteTime", NpgsqlDbType.Timestamp).Value = SqlValue(maxTimeUtc);

        return command;
    }

    public virtual NpgsqlCommand CreatePeek(int recordCount, string poolName)
    {
        string _poolName = poolName.ToFriendly();

        string sql =
            @$"

begin transaction;

DROP TABLE IF EXISTS tmp_ids_{_poolName};
CREATE TEMP TABLE tmp_ids_{_poolName} 
as
select  Id
from jm_jobs 
where Status = 10 and Pool = @pool and NextExecuteTime <= @nextExecuteTime
limit 10
FOR UPDATE;

update jm_jobs set status = 14 /*Enqueuing*/ where Id in (select Id from tmp_ids_{_poolName});

select * from jm_jobs
	inner join tmp_ids_{_poolName} as tid on tid.Id = jm_jobs.Id;

DROP TABLE IF EXISTS tmp_ids_{_poolName};     
commit transaction;

                ";

        //where Id in (select Id from #tmp_ids)

        NpgsqlCommand command = this.CreateCommand();
        command.CommandText = sql;
        command.Parameters.Add("@tcount", NpgsqlDbType.Integer).Value = SqlValue(recordCount);
        command.Parameters.Add("@pool", NpgsqlDbType.Varchar).Value = SqlValue(poolName);
        command.Parameters.Add("@nextExecuteTime", NpgsqlDbType.Timestamp).Value = SqlValue(JobManGlobals.Time.Now);
        //command.Parameters.Add("@status", NpgsqlDbType.Int).Value = SqlValue(WorkItemStatus.WaitingProcess);
        return command;
    }

    public NpgsqlCommand CreateMetrics()
    {
        string sql =
          @"
                select 'ItemCount' as Caption, count(*) as Count from public.jm_jobs as ItemCounts 
                union
                select 'WaitingItemCount' as Caption, count(*) as Count from public.jm_jobs as ItemCounts where Status = 10 /*WaitingProcess*/;

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

        NpgsqlCommand command = this.CreateCommand();
        command.CommandText = sql;
        return command;
    }
}

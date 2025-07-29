using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace JobMan;
public static class StorageExtensions
{
    public static IWorkItemDefinition MapToWorkItemDefinition(this DataRow row)
    {
        IWorkItemDefinition workItemDefinition = JobManGlobals.WorkServerOptions.WorkItemDefinitionFactory.Create(); 
        workItemDefinition.Id = row.To<long>(nameof(IWorkItemDefinition.Id));
        workItemDefinition.Type = row.To<WorkItemType>(nameof(IWorkItemDefinition.Type));
        workItemDefinition.Cron = row.To<string>(nameof(IWorkItemDefinition.Cron));
        workItemDefinition.Tag = row.To<string>(nameof(IWorkItemDefinition.Tag));
        workItemDefinition.LastExecuteTime = row.To<DateTime>(nameof(IWorkItemDefinition.LastExecuteTime));
        workItemDefinition.NextExecuteTime = row.To<DateTime>(nameof(IWorkItemDefinition.NextExecuteTime));
        workItemDefinition.Status = row.To<WorkItemStatus>(nameof(IWorkItemDefinition.Status));
        workItemDefinition.ProcessTimeMs = row.To<long>(nameof(IWorkItemDefinition.ProcessTimeMs));
        workItemDefinition.RetryCount = row.To<int>(nameof(IWorkItemDefinition.RetryCount));

        string dataStr = row.To<string>(nameof(IWorkItemDefinition.Data));

        if (!string.IsNullOrWhiteSpace(dataStr))
            workItemDefinition.Data = JobManGlobals.WorkServerOptions.WorkItemDefinitionSerializer.FromJson(dataStr);

        return workItemDefinition;
    }

    public static void MarkFail(IWorkItemStorage storage, DataRow row, string description)
    {
        if (row == null)
            throw new ArgumentNullException(nameof(row));
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentNullException(nameof(description));

        

        IWorkItemDefinition mock = JobManGlobals.WorkServerOptions.WorkItemDefinitionFactory.Create();
        mock.Id = row.To<long>(nameof(IWorkItemDefinition.Id), true);
        mock.Status = WorkItemStatus.Fail;
        mock.Description = description;
        storage.UpdateStatus(mock);
    }

    public static IWorkItemDefinition[] MapToWorkItemDefinitions(this DataTable table, IWorkItemStorage storage, ILogger logger)
    {
        List<IWorkItemDefinition> items = new List<IWorkItemDefinition>();
        foreach (DataRow row in table.Rows)
            try
            {
                try
                {
                    items.Add(row.MapToWorkItemDefinition());
                }
                catch (Exception ex)
                {

                    logger.LogError(ex, ex.Message);
                    MarkFail(storage, row, ex.Message);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }

        return items.ToArray();
    }
}

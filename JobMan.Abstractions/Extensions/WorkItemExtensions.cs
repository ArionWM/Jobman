using NCrontab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace JobMan;

public static class WorkItemExtensions
{
    public static IWorkItemDefinition CalculateNextRun(this IWorkItemDefinition definition)
    {
        if (definition.Type != WorkItemType.RecurrentRun)
            throw new InvalidOperationException("Invalid work item type");

        if (string.IsNullOrWhiteSpace(definition.Cron))
            throw new InvalidOperationException($"Cron can't be empty ({definition.Id})");

        DateTime startTime = JobmanHelperExtensions.Bigger(JobManGlobals.Time.Now, definition.LastExecuteTime);
        DateTime next = JobManGlobals.Time.GetNextOccurrence(definition.Cron, startTime);
        definition.NextExecuteTime = next;
        return definition;
    }

    public static void MarkFail(this IWorkItemDefinition wiDef, IWorkItemStorage storage, string description)
    {
        wiDef.Status = WorkItemStatus.Fail;
        wiDef.Description = description;
        storage.UpdateStatus(wiDef);
    }

}

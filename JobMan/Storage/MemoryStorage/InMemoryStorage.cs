
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace JobMan.Storage.MemoryStorage;

public class InMemoryStorage : IWorkItemStorage
{
    long _workItemIdSequence = 0;
    object _scheduleLock = new object();
    readonly ConcurrentQueue<IWorkItemDefinition> _workItems = new ConcurrentQueue<IWorkItemDefinition>();
    readonly ConcurrentDictionary<long, IWorkItemDefinition> _schedules = new ConcurrentDictionary<long, IWorkItemDefinition>();

    internal ConcurrentDictionary<long, IWorkItemDefinition> Schedules => _schedules;
    internal ConcurrentQueue<IWorkItemDefinition> WorkItems => _workItems;
    protected HashSet<IWorkPool> _directEnqueueCheckRegisteredWps = new HashSet<IWorkPool>();

    protected ILogger logger;


    public InMemoryStorage(string connectionString = null)
    {
        this.logger = JobManGlobals.LoggerFactory.CreateLogger<InMemoryStorage>();
    }

    public StorageMetrics GetMetrics()
    {
        StorageMetrics metrics = new StorageMetrics();
        metrics.WaitingItemCountOnBuffer = _workItems.Count(itm => itm.Status == WorkItemStatus.WaitingProcess);
        return metrics;
    }

    //TODO: To thread
    protected void CheckSchedules()
    {
        lock (_scheduleLock)
        {
            DateTime now = JobManGlobals.Time.Now;
            IWorkItemDefinition[] definitions = _schedules.Values.ToArray();
            foreach (IWorkItemDefinition wid in definitions)
            {
                if (wid.Status == WorkItemStatus.WaitingProcess && wid.NextExecuteTime <= now)
                {
                    _workItems.Enqueue(wid);
                }
            }
        }
    }

    protected long GetWorkItemId()
    {
        Interlocked.Add(ref _workItemIdSequence, _workItemIdSequence++);
        return _workItemIdSequence;
    }

    public IWorkItemDefinition[] PeekOrWait(int count, string poolName, int waitTimeMs, CancellationToken cancellationToken)
    {
        if (count <= 0)
            count = 1;

        this.CheckSchedules();

        List<IWorkItemDefinition> workItemDefinitions = new List<IWorkItemDefinition>();

        int index = 0;
        IWorkItemDefinition def;
        while (index <= count && _workItems.TryDequeue(out def))
        {
            def.Status = WorkItemStatus.Enqueued;
            workItemDefinitions.Add(def);
            index++;
        }

        if (workItemDefinitions.Count == 0)
            Thread.Sleep(waitTimeMs);

        return workItemDefinitions.ToArray();
    }

    protected bool DoItemAddedToStorage(IWorkItemDefinition workItemDefinition)
    {
        foreach (var workPool in _directEnqueueCheckRegisteredWps)
        {
            if (workItemDefinition.Pool == workPool.Name)
            {
                bool taken = workPool.CanEnqueueDirect(workItemDefinition);
#if DEBUG
                Debug.WriteLine($"Direct taken; {workItemDefinition.Id} / {workItemDefinition.Data.MethodName}: {taken}");
#endif

                return taken;
            }
        }


        return false;
    }

    public void Set(IWorkItemDefinition workItemDefinition)
    {
        workItemDefinition.Id = this.GetWorkItemId();

#if DEBUG
        Debug.WriteLine($"Storage try set; {workItemDefinition.Id} / {workItemDefinition.Data.MethodName} / {workItemDefinition.Data.MethodName} ");
#endif


        switch (workItemDefinition.Type)
        {
            case WorkItemType.SingleRun:
                if (workItemDefinition.Status == WorkItemStatus.WaitingProcess && this.DoItemAddedToStorage(workItemDefinition))
                {
#if DEBUG
                    Debug.WriteLine($"Storage try set; Already taken, pass: {workItemDefinition.Id} / {workItemDefinition.Data.MethodName}");
#endif
                    workItemDefinition.Status = WorkItemStatus.Enqueued;
                }
                else
                {
#if DEBUG
                    Debug.WriteLine($"Storage try set; Enqueue in storage: {workItemDefinition.Id} / {workItemDefinition.Data.MethodName}");
#endif
                    _workItems.Enqueue(workItemDefinition);
                }
                break;
            case WorkItemType.RecurrentRun:
                _schedules.TryAdd(workItemDefinition.Id, workItemDefinition);
                break;
        }
    }

    public void UpdateStatus(IWorkItemDefinition workItemDefinition)
    {
        switch (workItemDefinition.Type)
        {
            case WorkItemType.RecurrentRun:
                IWorkItemDefinition storedItem = _schedules.Get(workItemDefinition.Id, true);
                storedItem.Status = workItemDefinition.Status;

                storedItem.LastExecuteTime = workItemDefinition.LastExecuteTime;
                storedItem.NextExecuteTime = workItemDefinition.NextExecuteTime;
                storedItem.CalculateNextRun();

                switch (storedItem.Status)
                {
                    case WorkItemStatus.Completed:
                    case WorkItemStatus.Fail:
                        storedItem.Status = WorkItemStatus.WaitingProcess;
                        break;
                }
                break;
        }
    }

    public void RegisterDirectEnqueueCheck(IWorkPool workPool)
    {
        _directEnqueueCheckRegisteredWps.Add(workPool);
    }

    public void Dispose()
    {

    }

    public void Clean()
    {
        _workItems.Clear();
        _directEnqueueCheckRegisteredWps.Clear();
        _schedules.Clear();
    }
}

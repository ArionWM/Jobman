
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JobMan;



public interface IWorkItemDefinition
{
    long Id { get; set; }
    Guid Schedule { get; set; }
    WorkItemType Type { get; set; }
    string Pool { get; set; }
    string Cron { get; set; }

    DateTime LastExecuteTime { get; set; }
    DateTime NextExecuteTime { get; set; }

    InvokeData Data { get; set; }

    long ProcessTimeMs { get; set; }

    int RetryCount { get; set; }
    WorkItemStatus Status { get; set; }

    string Description { get; set; }

    /// <summary>
    /// Debug purposes
    /// </summary>
    string Tag { get; set; }
    IWorkItemDefinition Clone();
}

//public delegate void ItemAddedToStorageDelegate(IWorkItemDefinition item, out bool taken);

public class StorageOptions
{
    public Type StorageType { get; set; }
    public string ConnectionString { get; set; }


    public StorageOptions(Type storageType, string connectionString = null)
    {
        this.StorageType = storageType;
        this.ConnectionString = connectionString;
    }
}

public class StorageOptions<T> : StorageOptions
    where T : IWorkItemStorage
{
    public StorageOptions(string connectionString) : base(typeof(T), connectionString)
    {
    }
}


public interface IWorkItemStorage : IDisposable
{
    StorageMetrics GetMetrics();
    IWorkItemDefinition[] PeekOrWait(int count, string poolName, int waitTimeMs, CancellationToken cancellationToken);
    void Set(IWorkItemDefinition workItemDefinition);
    void UpdateStatus(IWorkItemDefinition workItemDefinition);
    void Clean();

    void RegisterDirectEnqueueCheck(IWorkPool workPool);

}

public interface IJob
{
    Guid Id { get; }


    void Execute();
}

public interface IWorkItem
{
    IJob Job { get; }
    IWorkItemDefinition Definition { get; }
    IWorkItemStorage Storage { get; } //Storage? 
}


public interface IWorker : IDisposable
{
    Guid Id { get; }
    bool IsDisposing { get; }
    Thread Thread { get; }
    IWorkPool WorkPool { get; }
    WorkerStatus Status { get; }

    void Start();
    Task StopAsync(); //TODO: Timeout && CancToken
    Task DisposeAsync();
}


public interface IWorkPoolOptions
{
    string Name { get; set; }
    int ThreadCount { get; set; }
    int PreProcessBufferLenght { get; set; }

    StorageOptions StorageOptions { get; set; }
    IWorkItemStorage? Storage { get; set; }

    TimeSpan IdlePeriod { get; set; } //2sn ? 
    ThreadPriority Priority { get; set; }


    bool ShowInUi { get; set; }

    IExecutionPolicy ExecutionPolicy { get; set; }

    IJobFilterManager JobExecutionFilter { get; set; }


    event Action<IWorker, IWorkItem> Executing;
    event Action<IWorker, IWorkItem> Executed;
    event Action<IWorker, IWorkItem> Failure;
}




public interface IWorkPool : IDisposable
{
    string Name { get; }
    int EnabledWorkerCount { get; }
    IWorkPoolOptions Options { get; }
    IEnumerable<IWorker> Workers { get; }

    WorkPoolStatus Status { get; }
    WorkPoolMetrics Metrics { get; }

    Task StartAsync();
    Task StopAsync();


    /// <summary>
    /// Get a work item from preprocess buffer 
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    IWorkItem GetWorkItemOrWait(CancellationToken cancellationToken);
    void UpdateStatus(IWorkItem workItem);

    public void EnqueueDirect(IWorkItem item);
    public bool CanEnqueueDirect(IWorkItemDefinition item);

    event Action<IWorkPool> MetricsUpdated;
}

public interface IWorkServerOptions
{
    ITypeResolver TypeResolver { get; set; }
    IWorkItemFactory WorkItemFactory { get; set; }
    IWorkItemDefinitionFactory WorkItemDefinitionFactory { get; set; }
    IJobFactory JobFactory { get; set; }
    IWorkPoolFactory WorkPoolFactory { get; set; }
    IWorkItemDefinitionSerializer WorkItemDefinitionSerializer { get; set; }

    IPolicyExecutor PolicyExecutor { get; set; }
    IExecutionPolicy DefaultPolicy { get; set; }

    IJobFilterManager JobExecutionFilter { get; set; }

    IWorkPoolOptions this[string name] { get; }

    List<IWorkPoolOptions> PoolOptions { get; }
    IWorkPoolOptions DefaultPoolOptions { get; }

    string CleanJobsScheduleCron { get; set; }
    bool DelayedStart { get; set; }
    int DelayedStartMilliseconds { get; set; }
}

public interface IWorkServer : IHostedService, IDisposable
{
    string Name { get; }

    IWorkServerOptions Options { get; }
    WorkServerStatus Status { get; }
    WorkServerMetrics Metrics { get; }

    IWorkPool[] Pools { get; }
    IWorkItemStorage[] Storages { get; }

    /// <summary>
    /// Add job to the default pool for run after a certain time
    /// </summary>
    /// <param name="workPoolName"></param>
    /// <param name="action"></param>
    /// <param name="tag"></param>
    void Enqueue(string workPoolName, Expression<Action> action, string tag = null);

    /// <summary>
    /// Add job to the default pool for run after a certain time
    /// </summary>
    /// <param name="workPoolName"></param>
    /// <param name="action"></param>
    /// <param name="runAfter"></param>
    /// <param name="tag"></param>
    void Enqueue(string workPoolName, Expression<Action> action, TimeSpan runAfter, string tag = null);

    /// <summary>
    /// Add recurring job to the default pool with cron expression
    /// </summary>
    /// <param name="workPoolName"></param>
    /// <param name="action"></param>
    /// <param name="cronExpression"></param>
    /// <param name="tag"></param>
    void Schedule(string workPoolName, Expression<Action> action, string cronExpression, string tag = null);

}


public interface IJobFactory
{
    IJob Create(InvokeData InvokeData);
}

public interface IWorkItemFactory
{
    IWorkItem Create(IWorkItemDefinition workItemDefinition);
}


public interface IWorkItemDefinitionFactory
{
    IWorkItemDefinition Create();
    IWorkItemDefinition Create(Expression<Action> expression, string poolName = null, string tag = null);
    IWorkItemDefinition Create(Expression<Action> expression, TimeSpan runAfter, string poolName = null, string tag = null);
    IWorkItemDefinition Create(Expression<Action> expression, string cronExpression, string poolName = null, string tag = null);
    void Validate(IWorkItemDefinition wiDef);
}

public interface IWorkPoolFactory
{
    IWorkPool Create(IWorkServer server, IWorkPoolOptions options);
}

public interface ITypeResolver
{
    Type Get(string name);
}

/// <summary>
/// Usable to change time / now reference, for test purposes
/// </summary>
public interface ITimeResolver
{
    DateTime Now { get; }

    DateTime GetNextOccurrence(string cronExpression, DateTime startTime);
}

public interface IWorkItemDefinitionSerializer
{
    string ToJson(InvokeData data);

    InvokeData FromJson(string json);
}

public interface IExecutionPolicy
{
    int FailureRetryCount { get; set; }

    TimeSpan ExecutionTimeout { get; set; } //= TimeSpan.MaxValue;

    int[] FailureRetryWaitTimes { get; set; }

}

public interface IFilterManager<T> where T : IFilter
{
    void Add(T filter);
    void Remove(T filter);
    T[] GetFilters();
}

public interface IJobFilterManager : IFilterManager<IJobExecutionFilter>
{
    void PreExecute(IWorker worker, IWorkItem item);

    void PostExecute(IWorker worker, IWorkItem item);

    JobExecutionFilterFailureResult Failure(IWorker worker, IWorkItem item, Exception ex, int retryCount);
}

public interface IFilter
{
    int Index { get; set; }
}

public interface IJobExecutionFilter : IFilter //DI / service yapmalı mı her bir şeyi?
{
    void PreExecute(IWorker worker, IWorkItem item);

    void PostExecute(IWorker worker, IWorkItem item);

    void Failure(IWorker worker, IWorkItem item, Exception ex, int retryCount, ref JobExecutionFilterFailureResult filterFailureResult);
}

public interface IPolicyExecutor
{
    void ExecuteFailurePolicy(IWorker worker, IWorkItem item, Exception ex, int retryCount, JobExecutionFilterFailureResult ffres);
}

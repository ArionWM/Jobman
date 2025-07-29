using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JobMan.Tests.Mock
{
    internal class MockWorkPool : IWorkPool, IJobExecutionFilter
    {
        public int Index { get; set; } = 1000;
        public string Name { get; protected set; }

        public int EnabledWorkerCount { get; protected set; }

        public IWorkPoolOptions Options { get; protected set; }

        public IEnumerable<IWorker> Workers { get; protected set; }

        public ConcurrentQueue<IWorkItem> PreProcessBuffer { get; protected set; }

        public WorkPoolStatus Status { get; protected set; }

        public WorkPoolMetrics Metrics { get; protected set; }


        public Func<IWorkItem> TestGetWorkItemOrWaitCallBack;
        public Action<IWorkItem> FailureCallBack;

        public event Action<IWorkPool> MetricsUpdated;

        public MockWorkPool()
        {
            Workers = new ConcurrentBag<IWorker>();
        }

        public void Dispose()
        {

        }

        public IWorkItem GetWorkItemOrWait(CancellationToken cancellationToken)
        {
            return TestGetWorkItemOrWaitCallBack.Invoke();
        }


        public async Task StartAsync()
        {
            this.Status = WorkPoolStatus.Active;
            await Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            this.Status = WorkPoolStatus.Stopped;
            await Task.CompletedTask;
        }

        public void UpdateStatus(IWorkItem workItem)
        {
            
        }

        public bool CanEnqueueDirect(IWorkItemDefinition item)
        {
            return false;
        }

        public void PreExecute(IWorker worker, IWorkItem item)
        {

        }

        public void PostExecute(IWorker worker, IWorkItem item)
        {
            
        }

        public void Failure(IWorker worker, IWorkItem item, Exception ex, int retryCount, ref JobExecutionFilterFailureResult filterFailureResult)
        {
            FailureCallBack.Invoke(item);

        }

        public void EnqueueDirect(IWorkItem item)
        {
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan
{
    public class StorageMetrics : Dictionary<string, object>
    {
        public int TotalItemCount { get; set; }
        public int WaitingItemCountOnStorate { get; set; }
        public int WaitingItemCountOnBuffer { get; set; }
        public Dictionary<WorkItemStatus, int> StatusCounts { get; set; } = new Dictionary<WorkItemStatus, int>();
        public int ScheduledItemCount { get; set; }
    }
}

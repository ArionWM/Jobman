using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan;

public enum WorkItemType
{
    SingleRun = 10,
    RecurrentRun = 20
}

public enum WorkerStatus
{
    Idle,
    Running,
    WaitingStop,
    Stopped,
    Terminated
}

public enum WorkServerStatus
{
    Active,
    WaitingStop,
    Stopped,
    Terminated
}

public enum WorkPoolStatus
{
    Active,
    WaitingStop,
    Stopped,
    Terminated
}

public enum WorkItemStatus
{
    WaitingProcess = 10,
    Enqueuing = 14,
    Enqueued = 16,
    Processing = 20,
    Completed = 50,
    Canceled = 99,
    Fail = 100
}

using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan;

public class JobExecutionFilterFailureResult
{
    public bool Handled { get; set; } = false;
    public bool ReTry { get; set; }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan;

public class ProcessDataSample : ICloneable //TODO: Must be struct
{
    public DateTime Time { get; set; }
    public int Waiting { get; set; }
    public int Processed { get; set; }
    public int Fail { get; set; }
    public int InQueue { get; set; }


    public ProcessDataSample(DateTime time)
    {
        Time = time.WithSecond();
    }

    public object Clone()
    {
        var clone = new ProcessDataSample(Time);
        clone.Processed = Processed;
        clone.Fail = Fail;
        clone.Waiting = Waiting;
        clone.InQueue = InQueue;
        return clone;

    }
}

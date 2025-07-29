using NCrontab;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan.Factories
{
    public class DefaultTimeResolver : ITimeResolver
    {
        public virtual DateTime Now => DateTime.Now;


        public virtual DateTime GetNextOccurrence(string cronExpression, DateTime startTime)
        {
            CrontabSchedule crontabSchedule = CrontabSchedule.Parse(cronExpression);
            DateTime next = crontabSchedule.GetNextOccurrence(startTime);
            return next;
        }


        
    }
}

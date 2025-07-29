using System;
using System.Collections.Generic;
using System.Text;

namespace JobMan
{
    public class CronConst
    {
        public const string PER_1MIN = "*/1 * * * *";
        public const string PER_5MIN = "*/5 * * * *";
        public const string PER_30MIN = "*/30 * * * *";
        public const string PER_2HOUR = "0 */2 * * *";
        public const string PER_MIDDAY = "0 12 * * *";
        public const string PER_MIDNIGHT = "0 0 * * *";

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWMS.Influx.Module.Services
{
    public class DateTimeService
    {
        public static DateTime RoundDateTimeToSeconds(DateTime input)
        {
            return new DateTime(input.Year, input.Month, input.Day, input.Hour, input.Minute, 0);
        }
    }
}

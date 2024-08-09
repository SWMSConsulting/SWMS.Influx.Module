using DevExpress.CodeParser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWMS.Influx.Module.Models
{
    // Documenation: https://docs.influxdata.com/flux/v0/stdlib/universe/range/
    public class FluxRange
    {
        public static string Now = "now()";
        public string Start {  get; set; }
        public string Stop {  get; set; }

        public FluxRange(string start, string stop)
        {
            // TODO: add validation
            Start = start;
            Stop = stop;
        }

        public FluxRange(DateTime start, DateTime stop) 
        {
            Start = DateTimeToUnixTimeStampString(start);
            Stop = DateTimeToUnixTimeStampString(stop);
        }

        public static string DateTimeToUnixTimeStampString(DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
        }
    }
}

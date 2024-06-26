using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWMS.Influx.Module.Models
{
    // Documentation: https://docs.influxdata.com/flux/v0/stdlib/universe/aggregatewindow/
    public class FluxAggregateWindow
    {
        public string Every {  get; set; }
        public FluxAggregateFunction Fn { get; set; }

        public FluxAggregateWindow(string every, FluxAggregateFunction fn)
        {
            Every = every;
            Fn = fn;
        }
    }
}

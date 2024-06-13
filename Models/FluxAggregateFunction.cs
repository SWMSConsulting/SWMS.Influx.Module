using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWMS.Influx.Module.Models
{
    public enum FluxAggregateFunction
    {
        Mean,
        Median,
        Sum,
        First,
        Last,
        Min,
        Max,
    }
}

using SWMS.Influx.Module.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWMS.Influx.Module.Attributes
{
    public class ValidFluxDurationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return InfluxDBService.IsValidFluxDuration((string)value);
        }
    }
}

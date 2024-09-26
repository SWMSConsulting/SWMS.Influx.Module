using SWMS.Influx.Module.Services;
using System.ComponentModel.DataAnnotations;

namespace SWMS.Influx.Module.Attributes
{
    public class ValidFluxDurationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return FluxService.IsValidFluxDuration((string)value);
        }
    }
}

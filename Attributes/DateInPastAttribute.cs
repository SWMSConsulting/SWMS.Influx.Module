using System.ComponentModel.DataAnnotations;

namespace SWMS.Influx.Module.Attributes
{
    public class DateInPastAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return (DateTime)value <= DateTime.Today;
        }
    }
}

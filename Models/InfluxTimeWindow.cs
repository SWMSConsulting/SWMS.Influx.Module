using SWMS.Influx.Module.Attributes;
using SWMS.Influx.Module.Services;
using System.ComponentModel.DataAnnotations;

namespace SWMS.Influx.Module.Models
{
    public class InfluxTimeWindow
    {
        [Required]
        [ValidFluxDuration(ErrorMessage = "Invalid flux duration.")]
        public string FluxDuration { get; set; }

        [Required]
        public FluxAggregateFunction FluxAggregateFunction { get; set; }

        [DateAfter("StartDate", ErrorMessage = "The end date must be after the start date.")]
        public DateTime EndDate { get; set; }

        [DateBefore("EndDate", ErrorMessage = "The start date must be before the end date.")]
        public DateTime StartDate { get; set; }

        public InfluxTimeWindow()
        {
            EndDate = DateTimeService.RoundDateTimeToSeconds(DateTime.Now);
            StartDate = EndDate.AddHours(-3);
            FluxDuration = "1m";
            FluxAggregateFunction = FluxAggregateFunction.Mean;
        }
    }
}

using SWMS.Influx.Module.Models;
using System.Text.RegularExpressions;

namespace SWMS.Influx.Module.Services;

public static class FluxService
{
    public static string GetFluxQueryForMeasurements(string bucket)
    {
        return $"import \"influxdata/influxdb/schema\"\n" +
                $"schema.measurements(" +
                $"bucket: \"{bucket}\"" +
                $")";
    }

    public static string GetFluxQueryForFields(string bucket, string measurement)
    {
        // List fields for measurement in bucket: https://docs.influxdata.com/influxdb/cloud/query-data/flux/explore-schema/
        // By default, this function returns results from the last 30 days.

        return $"import \"influxdata/influxdb/schema\"\n" +
                $"schema.measurementFieldKeys(" +
                $"bucket: \"{bucket}\"," +
                $"measurement: \"{measurement}\"," +
                $")";
    }

    public static string GetFluxQueryForTagKeys(string bucket, string measurement)
    {
        // List tags for measurement in bucket: https://docs.influxdata.com/influxdb/cloud/query-data/flux/explore-schema/
        // By default, this function returns results from the last 30 days.

        return $"import \"influxdata/influxdb/schema\"\n" +
                $"schema.measurementTagKeys(" +
                $"bucket: \"{bucket}\"," +
                $"measurement: \"{measurement}\"," +
                $")";
    }

    public static string GetFluxQueryForDatapoints(
        string bucket,
        FluxRange fluxRange,
        FluxAggregateWindow? aggregateWindow = null,
        Dictionary<string, List<string>>? filters = null,
        string? pipe = ""
    )
    {
        filters ??= new Dictionary<string, List<string>>();
        List<string> tagFluxFilters = new List<string>();
        foreach (var kvp in filters)
        {
            var arrowFunctionParts = new List<string>();
            foreach (var value in kvp.Value)
            {
                var arrowFunctionPart = $"r[\"{kvp.Key}\"] == \"{value}\"";
                arrowFunctionParts.Add(arrowFunctionPart);
            }
            var arrowFunction = String.Join(" or ", arrowFunctionParts);
            var tagFluxFilter = $"|> filter(fn: (r) => {arrowFunction})";
            tagFluxFilters.Add(tagFluxFilter);
        }
        var fluxFilterString = String.Join("\n", tagFluxFilters);

        var aggregateWindowString = "";
        if (aggregateWindow != null)
        {
            aggregateWindowString = $"|> aggregateWindow(every: {aggregateWindow.Every}, fn: {aggregateWindow.Fn.ToString().ToLower()}, createEmpty: false)";
        }

        string query = $"from(bucket:\"{bucket}\") " +
            $"|> range(start: {fluxRange.Start}, stop: {fluxRange.Stop}) " +
            fluxFilterString +
            aggregateWindowString +
            pipe;
        return query;
    }

    #region Duration Helper Functions

    private static string FluxDurationRegexPattern = @"^(\d+w)?(\d+d)?(\d+h)?(\d+m)?(\d+s)?(\d+ms)?$";

    public static bool IsValidFluxDuration(string duration)
    {
        if (string.IsNullOrWhiteSpace(duration))
        {
            return false;
        }

        Regex regex = new Regex(FluxDurationRegexPattern);
        return regex.IsMatch(duration);
    }

    public static string CalculateFluxDuration(InfluxTimeWindow influxTimeWindow, int pointNumber = 360)
    {
        return CalculateFluxDuration(influxTimeWindow.StartDate, influxTimeWindow.EndDate, pointNumber);
    }

    private static string CalculateFluxDuration(DateTime StartDate, DateTime EndDate, int pointNumber = 360)
    {
        if (StartDate >= EndDate)
        {
            throw new ArgumentException("The StartDate must be before the EndDate.");
        }
        var timeSpan = EndDate - StartDate;
        var milliseconds = timeSpan.TotalMilliseconds;
        var aggregateTime = milliseconds / pointNumber;
        return MillisecondsToFluxDuration(aggregateTime);
    }

    private static string MillisecondsToFluxDuration(double milliseconds)
    {
        if (milliseconds <= 0)
        {
            throw new ArgumentException("The number of seconds must be positive.");
        }

        TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);

        return TimeSpanToFluxDuration(timeSpan);
    }

    private static string TimeSpanToFluxDuration(TimeSpan timeSpan)
    {
        int weeks = (int)(timeSpan.TotalDays / 7);
        int days = timeSpan.Days % 7;
        int hours = timeSpan.Hours;
        int minutes = timeSpan.Minutes;
        int seconds = timeSpan.Seconds;
        int milliseconds = timeSpan.Milliseconds;

        string result = "";

        if (weeks > 0)
        {
            result += $"{weeks}w";
        }
        if (days > 0)
        {
            result += $"{days}d";
        }
        if (hours > 0)
        {
            result += $"{hours}h";
        }
        if (minutes > 0)
        {
            result += $"{minutes}m";
        }
        if (seconds > 0)
        {
            result += $"{seconds}s";
        }
        if (milliseconds > 0)
        {
            result += $"{milliseconds}ms";
        }

        return result;
    }
    #endregion
}

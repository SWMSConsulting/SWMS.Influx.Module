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

    public static DateTime? FluxRangeToDateTime(string? rangeQuantifier)
    {
        if (rangeQuantifier == null)
        {
            return null;
        }

        if (rangeQuantifier == "now()")
        {
            return DateTime.Now;
        }

        if (DateTime.TryParse(rangeQuantifier, out var date))
        {
            return date;
        }

        if (rangeQuantifier.Length > 1)
        {
            var durationUnit = rangeQuantifier.Last();
            var validDuration = int.TryParse(rangeQuantifier.Substring(0, rangeQuantifier.Length - 1), out var duration);


            DateTime dateTime = DateTime.Now;
            switch (durationUnit)
            {
                case 'y':
                    return dateTime.AddYears(duration);
                case 'M':
                    return dateTime.AddMonths(duration);
                case 'd':
                    return dateTime.AddDays(duration);
                case 'h':
                    return dateTime.AddHours(duration);
                case 'm':
                    return dateTime.AddMinutes(duration);
            }
        }

        return null;
    }
}

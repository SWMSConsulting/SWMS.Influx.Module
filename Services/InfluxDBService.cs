using System;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevExpress.ExpressApp.Security;
using InfluxDB.Client;
using Microsoft.Extensions.Configuration;
using SWMS.Influx.Module.BusinessObjects;
using SWMS.Influx.Module.Models;

namespace SWMS.Influx.Module.Services
{
    public class InfluxDBService
    {

        public InfluxDBService(IConfiguration configuration)
        {
        }

        public static void Write(Action<WriteApi> action)
        {
            var url = Environment.GetEnvironmentVariable("INFLUX_URL");
            var _token = Environment.GetEnvironmentVariable("INFLUX_TOKEN");

            using var client = new InfluxDBClient(url, _token);
            using var write = client.GetWriteApi();
            action(write);
        }

        public static async Task<T> QueryAsync<T>(Func<QueryApi, Task<T>> action)
        {
            var url = Environment.GetEnvironmentVariable("INFLUX_URL");
            var _token = Environment.GetEnvironmentVariable("INFLUX_TOKEN");

            using var client = new InfluxDBClient(url, _token);

            var query = client.GetQueryApi();
            return await action(query);
        }

        public static string FluxDurationRegexPattern = @"^(\d+w)?(\d+d)?(\d+h)?(\d+m)?(\d+s)?(\d+ms)?$";

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

        public static string CalculateFluxDuration(DateTime StartDate, DateTime EndDate, int pointNumber = 360)
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

        public static string MillisecondsToFluxDuration(double milliseconds)
        {
            if (milliseconds <= 0)
            {
                throw new ArgumentException("The number of seconds must be positive.");
            }

            TimeSpan timeSpan = TimeSpan.FromMilliseconds(milliseconds);

            return TimeSpanToFluxDuration(timeSpan);
        }

        public static string TimeSpanToFluxDuration(TimeSpan timeSpan)
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

        public static string GetFluxQuery(
            string bucket,
            DateTime start,
            DateTime end,
            string aggregateTime,
            FluxAggregateFunction aggregateFunction,
            Dictionary<string, string>? filters = null
        )
        {
            // TODO: enable strings for start and end inputs
            string rangeStart = start.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
            string rangeEnd = end.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");

            filters ??= new Dictionary<string, string>();

            List<string> tagFluxFilters = new List<string>();
            foreach ( var kvp in filters )
            {
                var tagFluxFilter = $"|> filter(fn: (r) => r[\"{kvp.Key}\"] == \"{kvp.Value}\")";
                tagFluxFilters.Add( tagFluxFilter );
            }

            var fluxFilterString = String.Join(" ", tagFluxFilters);

            string query = $"from(bucket:\"{bucket}\") " +
                $"|> range(start: {rangeStart}, stop: {rangeEnd}) " +
                fluxFilterString +
                $"|> aggregateWindow(every: {aggregateTime}, fn: {aggregateFunction.ToString().ToLower()})" +
                "|> group(columns: [\"_field\", \"_time\"])";
            return query;
        }

    }
}
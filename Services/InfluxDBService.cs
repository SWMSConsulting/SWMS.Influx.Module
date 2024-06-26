﻿using System.Text.RegularExpressions;
using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Domain;
using SWMS.Influx.Module.Models;

namespace SWMS.Influx.Module.Services
{
    public class InfluxDBService
    {
        private readonly static string _url = EnvironmentVariableService.GetRequiredStringFromENV("INFLUX_URL");
        private readonly static string _token = EnvironmentVariableService.GetRequiredStringFromENV("INFLUX_TOKEN");
        private readonly static string _organization = EnvironmentVariableService.GetRequiredStringFromENV("INFLUX_ORG");
        private readonly static string _bucket = EnvironmentVariableService.GetRequiredStringFromENV("INFLUX_BUCKET");
        private readonly static InfluxDBClient _client = new InfluxDBClient(_url, _token);
        private readonly static WriteApi _writeApi = _client.GetWriteApi();
        private readonly static QueryApi _queryApi = _client.GetQueryApi();

        public static void Write(Action<WriteApi> action)
        {
            action(_writeApi);
        }

        public static async Task<T> QueryAsync<T>(Func<QueryApi, Task<T>> action)
        {
            return await action(_queryApi);
        }

        public static async Task<List<FluxTable>> QueryAsync(string flux)
        {
            return await _queryApi.QueryAsync(flux, _organization);
        }

        public static async Task<List<FluxTable>> QueryAsync(
            FluxRange fluxRange,
            FluxAggregateWindow? aggregateWindow = null,
            Dictionary<string, string>? filters = null
            )
        {
            var flux = GetFluxQuery(fluxRange, aggregateWindow, filters);
            //Console.WriteLine(flux);
            return await _queryApi.QueryAsync(flux, _organization);
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
            FluxRange fluxRange,
            FluxAggregateWindow? aggregateWindow = null,
            Dictionary<string, string>? filters = null
        )
        {
            filters ??= new Dictionary<string, string>();
            List<string> tagFluxFilters = new List<string>();
            foreach ( var kvp in filters )
            {
                var tagFluxFilter = $"|> filter(fn: (r) => r[\"{kvp.Key}\"] == \"{kvp.Value}\")";
                tagFluxFilters.Add( tagFluxFilter );
            }
            var fluxFilterString = String.Join(" ", tagFluxFilters);

            var aggregateWindowString = "";
            if( aggregateWindow != null)
            {
                aggregateWindowString = $"|> aggregateWindow(every: {aggregateWindow.Every}, fn: {aggregateWindow.Fn.ToString().ToLower()})";
            }

            string query = $"from(bucket:\"{_bucket}\") " +
                $"|> range(start: {fluxRange.Start}, stop: {fluxRange.Stop}) " +
                fluxFilterString +
                aggregateWindowString;
            return query;
        }

    }
}
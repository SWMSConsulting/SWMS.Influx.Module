using System;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DevExpress.ExpressApp.Security;
using InfluxDB.Client;
using Microsoft.Extensions.Configuration;

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

        public static string FluxDurationRegexPattern = @"^(\d+w)?(\d+d)?(\d+h)?(\d+m)?(\d+s)?$";

        public static bool IsValidFluxDuration(string duration)
        {
            if (string.IsNullOrWhiteSpace(duration))
            {
                return false;
            }

            Regex regex = new Regex(FluxDurationRegexPattern);
            return regex.IsMatch(duration);
        }

    }
}
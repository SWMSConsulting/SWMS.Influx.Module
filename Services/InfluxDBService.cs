using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Core;
using DevExpress.XtraRichEdit.Fields;
using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Domain;
using Microsoft.Extensions.DependencyInjection;
using SWMS.Influx.Module.BusinessObjects;
using SWMS.Influx.Module.Models;
using System.Security.Policy;
using System.Text.RegularExpressions;

namespace SWMS.Influx.Module.Services;

public class InfluxDBService
{
    private readonly string _url = EnvironmentVariableService.GetRequiredStringFromENV("INFLUX_URL");
    private readonly string _token = EnvironmentVariableService.GetRequiredStringFromENV("INFLUX_TOKEN");
    
    public readonly string Organization = EnvironmentVariableService.GetRequiredStringFromENV("INFLUX_ORG");
    public readonly string Bucket = EnvironmentVariableService.GetRequiredStringFromENV("INFLUX_BUCKET");

    private WriteApi _writeApi;
    private QueryApi _queryApi;

    private IServiceScopeFactory _serviceScopeFactory;
    public Dictionary<string, InfluxDatapoint> LastDatapoints { get; private set; } = new Dictionary<string, InfluxDatapoint>();

    public InfluxDBService(
        IServiceScopeFactory serviceScopeFactory
    )
    {
        _serviceScopeFactory = serviceScopeFactory;

        InfluxDBClient client = new InfluxDBClient(_url, _token);
        _writeApi = client.GetWriteApi();
        _queryApi = client.GetQueryApi();
    }

    public void Write(Action<WriteApi> action)
    {
        action(_writeApi);
    }

    public async Task<T> QueryAsync<T>(Func<QueryApi, Task<T>> action)
    {
        return await action(_queryApi);
    }

    public async Task<List<FluxTable>> QueryAsync(string flux)
    {
        return await _queryApi.QueryAsync(flux, Organization);
    }

    public async Task SetLastDatapoints()
    {
        var datapoints = await QueryLastDatapoints("-24h");
        // InfluxField.ID would also be possible as key, but is less readable
        LastDatapoints = datapoints.ToDictionary(x => x.InfluxField.GlobalIdentifer, x => x);
    }

    public InfluxDatapoint GetLastDatapointForField(InfluxField field)
    {
        return LastDatapoints.GetValueOrDefault(field.GlobalIdentifer);
    }

    public async Task<List<InfluxDatapoint>> QueryLastDatapoints(string fluxDuration)
    {
        var fluxRange = new FluxRange(fluxDuration, "now()");
        var datapoints = await QueryInfluxDatapoints(
            fluxRange: fluxRange,
            pipe: "|> last()"
            );
        return datapoints;
    }

    public async Task<List<InfluxDatapoint>> QueryInfluxDatapoints(
        FluxRange fluxRange,
        FluxAggregateWindow? aggregateWindow = null,
        Dictionary<string, List<string>>? filters = null,
        string? pipe = ""
        )
    {
        var flux = GetFluxQuery(fluxRange, aggregateWindow, filters, pipe);
        //Console.WriteLine(flux);
        var tables = await _queryApi.QueryAsync(flux, Organization);
        return FluxTablesToInfluxDatapoints(tables);
    }

    public bool FluxRecordIsInfluxField(InfluxField field, FluxRecord record)
    {
        if(field == null || record == null)
        {
            return false;
        }
        var measurementName = record.GetMeasurement();
        var fieldName = record.GetField();
        var influxIdentifier = field.InfluxMeasurement.AssetAdministrationShell.AssetCategory.InfluxIdentifier;
        var assetId = field.InfluxMeasurement.AssetAdministrationShell.AssetId;
        var recordIsCurrentField = field.Name == fieldName &&
            field.InfluxMeasurement.Name == measurementName &&
            record.GetValueByKey(influxIdentifier).ToString() == assetId;
        return recordIsCurrentField;
    }

    public List<InfluxDatapoint> FluxTablesToInfluxDatapoints(List<FluxTable> tables)
    {
        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var objectSpaceFactory = scope.ServiceProvider.GetService<INonSecuredObjectSpaceFactory>();
            var _objectSpace = objectSpaceFactory.CreateNonSecuredObjectSpace<AssetCategory>();
            // TODO: optimize by keeping local List / HashSet of InfluxFields instead of loading from ObjectSpace
            var influxFields = _objectSpace.GetObjects<InfluxField>().ToList();

            List<InfluxDatapoint> datapoints = new();
            tables.ForEach(table =>
            {
                InfluxField currentField = influxFields.First();
                table.Records.ForEach(record =>
                {
                    if (record.GetValue() == null)
                    {
                        return;
                    }
                    if (!FluxRecordIsInfluxField(currentField, record))
                    {
                        currentField = influxFields.FirstOrDefault(x => FluxRecordIsInfluxField(x, record));
                    }
                    if (currentField == null)
                    {
                        return;
                    }

                    InfluxDatapoint datapoint = new InfluxDatapoint()
                    {
                        Value = (double)record.GetValue(),
                        Time = (DateTime)record.GetTimeInDateTime(),
                        InfluxField = currentField,
                    };
                    datapoints.Add(datapoint);
                });
            });
            return datapoints;
        }
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

    public string GetFluxQuery(
        FluxRange fluxRange,
        FluxAggregateWindow? aggregateWindow = null,
        Dictionary<string, List<string>>? filters = null,
        string? pipe = ""
    )
    {
        filters ??= new Dictionary<string, List<string>>();
        List<string> tagFluxFilters = new List<string>();
        foreach ( var kvp in filters )
        {
            var arrowFunctionParts = new List<string>();
            foreach ( var value in kvp.Value)
            {
                var arrowFunctionPart = $"r[\"{kvp.Key}\"] == \"{value}\"";
                arrowFunctionParts.Add( arrowFunctionPart );
            }
            var arrowFunction = String.Join(" or ", arrowFunctionParts);
            var tagFluxFilter = $"|> filter(fn: (r) => {arrowFunction})";
            tagFluxFilters.Add( tagFluxFilter );
        }
        var fluxFilterString = String.Join("\n", tagFluxFilters);

        var aggregateWindowString = "";
        if( aggregateWindow != null)
        {
            aggregateWindowString = $"|> aggregateWindow(every: {aggregateWindow.Every}, fn: {aggregateWindow.Fn.ToString().ToLower()})";
        }

        string query = $"from(bucket:\"{Bucket}\") " +
            $"|> range(start: {fluxRange.Start}, stop: {fluxRange.Stop}) " +
            fluxFilterString +
            aggregateWindowString +
            pipe;
        return query;
    }

}
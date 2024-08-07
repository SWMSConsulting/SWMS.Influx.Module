using DevExpress.ExpressApp.Core;
using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Domain;
using Microsoft.Extensions.DependencyInjection;
using SWMS.Influx.Module.BusinessObjects;
using SWMS.Influx.Module.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace SWMS.Influx.Module.Services;

public class InfluxDBService
{
    private readonly static string _url = EnvironmentVariableService.GetRequiredStringFromENV("INFLUX_URL");
    private readonly static string _token = EnvironmentVariableService.GetRequiredStringFromENV("INFLUX_TOKEN");
    
    public readonly static string Organization = EnvironmentVariableService.GetRequiredStringFromENV("INFLUX_ORG");
    public readonly static string _bucket = EnvironmentVariableService.GetRequiredStringFromENV("INFLUX_BUCKET");
    
    private readonly static InfluxDBClient _client = new InfluxDBClient(_url, _token);
    private readonly static WriteApi _writeApi = _client.GetWriteApi();
    private readonly static QueryApi _queryApi = _client.GetQueryApi();
    private static Dictionary<string, InfluxDatapoint> LastDatapoints { get; set; } = new Dictionary<string, InfluxDatapoint>();
    
    private static IServiceScopeFactory _serviceScopeFactory;

    public InfluxDBService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;

        InitializeInfluxSchema();
    }

    private static async void InitializeInfluxSchema()
    {
        await QueryInfluxMeasurements();
        await RefreshLastDatapoints();
    }


    #region Query Measurements
    public static async Task<IEnumerable<InfluxMeasurement>> QueryInfluxMeasurements()
    {
        var results = await QueryAsync(async query =>
        {
            var flux = GetFluxQueryForMeasurements(_bucket);
            try
            {
                var tables = await query.QueryAsync(flux, Organization);
                return tables.SelectMany(table =>
                    table.Records.Select(record =>
                        new InfluxMeasurement
                        {
                            Name = record.GetValueByKey("_value").ToString(),
                        }));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new ObservableCollection<InfluxMeasurement>();
            }
        });

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var objectSpaceFactory = scope.ServiceProvider.GetService<INonSecuredObjectSpaceFactory>();
            var objectSpace = objectSpaceFactory.CreateNonSecuredObjectSpace<InfluxMeasurement>();

            var allMeasurements = objectSpace.GetObjects<InfluxMeasurement>();

            foreach (var measurement in results)
            {
                var existingMeasurement = allMeasurements.FirstOrDefault(x => x.Name == measurement.Name);
                if (existingMeasurement == null)
                {
                    existingMeasurement = objectSpace.CreateObject<InfluxMeasurement>();
                    existingMeasurement.Name = measurement.Name;
                }

                await existingMeasurement.GetFields();
                await existingMeasurement.GetTagKeys();
            }

            objectSpace.CommitChanges();

            var updatedMeasurements = objectSpace.GetObjects<InfluxMeasurement>();
            return updatedMeasurements;
        }
    }
    #endregion

    #region Query Fields
    public static async Task<IEnumerable<InfluxField>> GetInfluxFieldsForMeasurement(string measurement)
    {
        return await QueryAsync(async query =>
        {
            var flux = GetFluxQueryForFields(measurement);

            var tables = await query.QueryAsync(flux, Organization);

            return tables.SelectMany(table =>
                table.Records.Select(record =>
                    new InfluxField
                    {
                        Name = record.GetValueByKey("_value").ToString()
                    }
                )
            );
        });
    }
    #endregion

    #region Query TagKeys
    public static async Task<IEnumerable<InfluxTagKey>> GetInfluxTagKeysForMeasurement(string measurement)
    {
        return await QueryAsync(async query =>
        {
            var flux = GetFluxQueryForTagKeys(measurement);

            var tables = await query.QueryAsync(flux, Organization);

            return tables.SelectMany(table =>
                table.Records.Select(record =>
                    new InfluxTagKey
                    {
                        Name = record.GetValueByKey("_value").ToString()
                    }
                )
            );
        });
    }
    #endregion

    #region Query Last Datapoints
    public static InfluxDatapoint GetLastDatapointForField(InfluxField field, InfluxIdentificationInstance identification)
    {
        return LastDatapoints.GetValueOrDefault(GetFieldIdentifier(field, identification));
    }

    public static async Task RefreshLastDatapoints()
    {
        var datapoints = await QueryLastDatapoints("-24h");
        // InfluxField.ID would also be possible as key, but is less readable
        LastDatapoints = datapoints.ToDictionary(x => GetFieldIdentifier(x.InfluxField, x.InfluxTagValues), x => x);
    }

    private static async Task<List<InfluxDatapoint>> QueryLastDatapoints(string fluxDuration)
    {
        var fluxRange = new FluxRange(fluxDuration, "now()");
        var datapoints = await QueryInfluxDatapoints(
            fluxRange: fluxRange,
            pipe: "|> last()"
            );
        return datapoints;
    }
    #endregion

    #region Query Datapoints
    public static async Task<List<InfluxDatapoint>> QueryInfluxDatapoints(
        FluxRange fluxRange,
        FluxAggregateWindow? aggregateWindow = null,
        Dictionary<string, List<string>>? filters = null,
        string? pipe = ""
        )
    {
        var flux = GetFluxQueryForDatapoints(fluxRange, aggregateWindow, filters, pipe);
        //Console.WriteLine(flux);
        var tables = await _queryApi.QueryAsync(flux, Organization);
        return FluxTablesToInfluxDatapoints(tables);
    }

    public static List<InfluxDatapoint> FluxTablesToInfluxDatapoints(List<FluxTable> tables)
    {
        // TODO: optimize by keeping local List / HashSet of InfluxFields instead of loading from ObjectSpace

        using (var scope = _serviceScopeFactory.CreateScope())
        {
            var objectSpaceFactory = scope.ServiceProvider.GetService<INonSecuredObjectSpaceFactory>();
            var objectSpace = objectSpaceFactory.CreateNonSecuredObjectSpace<InfluxMeasurement>();
            var influxFields = objectSpace.GetObjects<InfluxField>();
            
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

                    var tagList = new BindingList<InfluxTagValue>();

                    var recordTags = record.Values.Where(x => !x.Key.StartsWith("_") && x.Key != "result" && x.Key != "table").OrderBy(x => x.Key).ToList();

                    foreach (var tag in recordTags)
                    {
                        var influxTagKeys = objectSpace.GetObjects<InfluxTagKey>();
                        var influxTagKey = influxTagKeys.FirstOrDefault(x => x.Name == tag.Key && x.InfluxMeasurement.Name == record.GetMeasurement());
                        if (influxTagKey == null)
                        {
                            return;
                        }
                        var tagInfluxValue = new InfluxTagValue();
                        tagInfluxValue.InfluxTagKey = influxTagKey;
                        tagInfluxValue.Value = tag.Value.ToString();
                        tagList.Add(tagInfluxValue);
                    }

                    InfluxDatapoint datapoint = new InfluxDatapoint()
                    {
                        Value = (double)record.GetValue(),
                        Time = (DateTime)record.GetTimeInDateTime(),
                        InfluxField = currentField,
                        InfluxTagValues = tagList,
                    };
                    datapoints.Add(datapoint);
                });
            });
            return datapoints;
        }
    }
    public static bool FluxRecordIsInfluxField(InfluxField field, FluxRecord record)
    {
        if (field == null || record == null)
        {
            return false;
        }
        var measurementName = record.GetMeasurement();
        var fieldName = record.GetField();
        //var influxIdentifier = field.InfluxMeasurement.AssetAdministrationShell.AssetCategory.InfluxIdentifier;
        //var assetId = field.InfluxMeasurement.AssetAdministrationShell.AssetId;
        var recordIsCurrentField = field.Name == fieldName &&
            field.InfluxMeasurement.Name == measurementName;
        //record.GetValueByKey(influxIdentifier).ToString() == assetId;
        return recordIsCurrentField;
    }
    #endregion

    #region Static Helper Functions

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

    private static string GetFluxQueryForMeasurements(string? bucket = null)
    {
        if (bucket == null)
        {
            bucket = _bucket;
        }

        return $"import \"influxdata/influxdb/schema\"\n" +
                $"schema.measurements(" +
                $"bucket: \"{bucket}\"" +
                $")";
    }

    private static string GetFluxQueryForFields(string measurement, string? bucket = null)
    {
        // List fields for measurement in bucket: https://docs.influxdata.com/influxdb/cloud/query-data/flux/explore-schema/
        // By default, this function returns results from the last 30 days.

        if (bucket == null)
        {
            bucket = _bucket;
        }

        return $"import \"influxdata/influxdb/schema\"\n" +
                $"schema.measurementFieldKeys(" +
                $"bucket: \"{bucket}\"," +
                $"measurement: \"{measurement}\"," +
                $")";
    }

    private static string GetFluxQueryForTagKeys(string measurement, string? bucket = null)
    {
        // List tags for measurement in bucket: https://docs.influxdata.com/influxdb/cloud/query-data/flux/explore-schema/
        // By default, this function returns results from the last 30 days.

        if (bucket == null)
        {
            bucket = _bucket;
        }

        return $"import \"influxdata/influxdb/schema\"\n" +
                $"schema.measurementTagKeys(" +
                $"bucket: \"{bucket}\"," +
                $"measurement: \"{measurement}\"," +
                $")";
    }

    private static string GetFluxQueryForDatapoints(
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
            aggregateWindowString = $"|> aggregateWindow(every: {aggregateWindow.Every}, fn: {aggregateWindow.Fn.ToString().ToLower()}, createEmpty: false)";
        }

        string query = $"from(bucket:\"{_bucket}\") " +
            $"|> range(start: {fluxRange.Start}, stop: {fluxRange.Stop}) " +
            fluxFilterString +
            aggregateWindowString +
            pipe;
        return query;
    }

    private static string GetFieldIdentifier(InfluxField field, InfluxIdentificationInstance identification)
    {
        return GetFieldIdentifier(field, identification.InfluxTagValues);
    }

    private static string GetFieldIdentifier(InfluxField field, IList<InfluxTagValue> influxTagValues)
    {
        return $"{field.InfluxMeasurement.Name}_{field.Name}_{GetTagSetString(influxTagValues)}";
    }

    public static string GetTagSetString(IList<InfluxTagValue> influxTagValues)
    {
        var orderedInfluxTagValues = influxTagValues.OrderBy(x => x.InfluxTagKey.Name);
        return String.Join(",", orderedInfluxTagValues.Select(x => x.ToString()));
    }

    public static string GetTagSetString(FluxRecord record)
    {
        var recordTags = record.Values.Where(x => !x.Key.StartsWith("_") && x.Key != "result" && x.Key != "table").OrderBy(x => x.Key);
        var keyValuePairStrings = recordTags.Select(x => KeyValuePairToString(x.Key, x.Value.ToString()));
        return String.Join(",", keyValuePairStrings);
    }

    public static string KeyValuePairToString(string key, string value)
    {
        return $"{key}={value}";
    }
    #endregion

    #region Influx API
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
        return await _queryApi.QueryAsync(flux, Organization);
    }
    #endregion
}
using DevExpress.ExpressApp.Blazor.Components;
using DevExpress.ExpressApp.Core;
using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Domain;
using Microsoft.Extensions.DependencyInjection;
using SWMS.Influx.Module.BusinessObjects;
using SWMS.Influx.Module.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Timers;

namespace SWMS.Influx.Module.Services;

public class InfluxDBService
{
    private readonly static string _url = EnvironmentVariableService.GetRequiredStringFromENV("INFLUX_URL");
    private readonly static string _token = EnvironmentVariableService.GetRequiredStringFromENV("INFLUX_TOKEN");
    private readonly static string _bucket = EnvironmentVariableService.GetRequiredStringFromENV("INFLUX_BUCKET");
    
    public readonly static string Organization = EnvironmentVariableService.GetRequiredStringFromENV("INFLUX_ORG");
    
    private readonly static InfluxDBClientOptions _clientOptions = new InfluxDBClientOptions.Builder()
        .Url(_url)
        .AuthenticateToken(_token)
        .TimeOut(TimeSpan.FromMinutes(5))
        .Build();

    private readonly static InfluxDBClient _client = new InfluxDBClient(_clientOptions);
    private readonly static WriteApi _writeApi = _client.GetWriteApi();
    private readonly static QueryApi _queryApi = _client.GetQueryApi();
    private static Dictionary<string, InfluxDatapoint> LastDatapoints { get; set; } = new Dictionary<string, InfluxDatapoint>();
    
    private static Dictionary<string, Dictionary<string, List<InfluxDatapoint>>> CachedQueries { get; set; } = new Dictionary<string, Dictionary<string, List<InfluxDatapoint>>>();

    private static IServiceScopeFactory _serviceScopeFactory;

    public InfluxDBService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;

        InitializeInfluxSchema();
        SetupBackgroundWorker();
    }

    private static async void InitializeInfluxSchema()
    {
        await QueryInfluxMeasurements();
        await RefreshLastDatapoints();
        await RefreshCachedQueries();
    }


    #region Query Measurements
    public static async Task<IEnumerable<InfluxMeasurement>> QueryInfluxMeasurements()
    {
        var results = await QueryAsync(async query =>
        {
            var flux = FluxService.GetFluxQueryForMeasurements(_bucket);
            try
            {
                var tables = await query.QueryAsync(flux, Organization);
                return tables.SelectMany(table =>
                    table.Records.Select(record =>
                        new InfluxMeasurement
                        {
                            Identifier = record.GetValueByKey("_value").ToString(),
                            DisplayName = record.GetValueByKey("_value").ToString(),
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
                var existingMeasurement = allMeasurements.FirstOrDefault(x => x.Identifier == measurement.Identifier);
                if (existingMeasurement == null)
                {
                    existingMeasurement = objectSpace.CreateObject<InfluxMeasurement>();
                    existingMeasurement.Identifier = measurement.Identifier;
                    existingMeasurement.DisplayName = measurement.DisplayName;
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
            var flux = FluxService.GetFluxQueryForFields(_bucket, measurement);

            var tables = await query.QueryAsync(flux, Organization);

            return tables.SelectMany(table =>
                table.Records.Select(record =>
                    new InfluxField
                    {
                        Identifier = record.GetValueByKey("_value").ToString(),
                        DisplayName = record.GetValueByKey("_value").ToString()
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
            var flux = FluxService.GetFluxQueryForTagKeys(_bucket, measurement);

            var tables = await query.QueryAsync(flux, Organization);

            return tables.SelectMany(table =>
                table.Records.Select(record =>
                    new InfluxTagKey
                    {
                        Identifier = record.GetValueByKey("_value").ToString()
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

    public static List<InfluxDatapoint> GetCachedQueryValues(string cachedQueryIdentifier, InfluxField field, InfluxIdentificationInstance identification)
    {
        return CachedQueries.GetValueOrDefault(cachedQueryIdentifier)?.GetValueOrDefault(GetFieldIdentifier(field, identification)) ?? new List<InfluxDatapoint>();
    }

    public static async Task RefreshCachedQueries()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var objectSpaceFactory = scope.ServiceProvider.GetService<INonSecuredObjectSpaceFactory>();
        var objectSpace = objectSpaceFactory.CreateNonSecuredObjectSpace<CachedQuery>();
        var queries = objectSpace.GetObjects<CachedQuery>().ToList();

        queries.ForEach(query => RefreshCachedQuery(query));
    }

    public static async Task RefreshCachedQuery(CachedQuery query)
    {
        var datapoints = await QueryInfluxDatapointsWithCancellation(
                fluxRange: new FluxRange(query.QuerySettings.RangeQuantifierStart, query.QuerySettings.RangeQuantifierEnd),
                aggregateWindow: new FluxAggregateWindow(query.QuerySettings.AggregateWindow, query.QuerySettings.AggregateFunction),
                //aggregateFunction: query.QuerySettings.AggregateFunction,
                influxFields: query.InfluxFields,
                influxMeasurements: query.InfluxFields.Select(x => x.InfluxMeasurement).Distinct()
            );

        var dictionaryResult = new Dictionary<string, List<InfluxDatapoint>>();
        datapoints.ForEach(dp =>
        {
            var key = GetFieldIdentifier(dp.InfluxField, dp.InfluxTagValues);
            if (!dictionaryResult.ContainsKey(key))
            {
                dictionaryResult[key] = new List<InfluxDatapoint>();
            }
            dictionaryResult[key].Add(dp);
        });

        CachedQueries[query.Identifier] = dictionaryResult;
    }

    public static async Task RefreshLastDatapoints()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var objectSpaceFactory = scope.ServiceProvider.GetService<INonSecuredObjectSpaceFactory>();
        var objectSpace = objectSpaceFactory.CreateNonSecuredObjectSpace<InfluxMeasurement>();
        var measurements = objectSpace.GetObjects<InfluxMeasurement>().Where(m => m.IsInUse).ToList();

        if (measurements.Count == 0)
        {
            Console.WriteLine("No relevant measurements found");
            return;
        }

        var datapoints = await QueryLastDatapoints("-30d", measurements);
        // InfluxField.ID would also be possible as key, but is less readable
        LastDatapoints = datapoints.ToDictionary(x => GetFieldIdentifier(x.InfluxField, x.InfluxTagValues), x => x);
        Console.WriteLine($"Last Datapoints refreshed: {LastDatapoints.Count}");
    }

    private static CancellationTokenSource cancellationTokenSourceLastDp = new CancellationTokenSource();

    private static async Task<List<InfluxDatapoint>> QueryLastDatapoints(string fluxDuration, IList<InfluxMeasurement> measurements)
    {
        cancellationTokenSourceLastDp.Cancel();
        cancellationTokenSourceLastDp = new CancellationTokenSource();

        var fluxRange = new FluxRange(fluxDuration, FluxRange.Now);
        return await QueryInfluxDatapointsWithCancellation(
            fluxRange: fluxRange,
            influxMeasurements: measurements,
            pipe: FluxQueryPipe.Last,
            cancellationToken: cancellationTokenSourceLastDp.Token
        );
    }
    #endregion

    #region Query Datapoints
    private static CancellationTokenSource cancellationTokenSourceQueryDp = new CancellationTokenSource();
    public static async Task<List<InfluxDatapoint>> QueryInfluxDatapoints(
        FluxRange fluxRange,
        FluxAggregateWindow? aggregateWindow = null,
        IEnumerable<InfluxField>? influxFields = null,
        IEnumerable<InfluxMeasurement>? influxMeasurements = null,
        IEnumerable<InfluxIdentificationInstance>? influxIdentificationInstances = null,
        FluxQueryPipe? pipe = null
        )
    {
        cancellationTokenSourceQueryDp.Cancel();
        cancellationTokenSourceQueryDp = new CancellationTokenSource();
        return await QueryInfluxDatapointsWithCancellation(
            fluxRange: fluxRange,
            aggregateWindow: aggregateWindow,
            influxFields: influxFields,
            influxMeasurements: influxMeasurements,
            influxIdentificationInstances: influxIdentificationInstances,
            pipe: pipe,
            cancellationToken: cancellationTokenSourceQueryDp.Token
        );
    }

    private static async Task<List<InfluxDatapoint>> QueryInfluxDatapointsWithCancellation(
        FluxRange fluxRange,
        FluxAggregateWindow? aggregateWindow = null,
        IEnumerable<InfluxField>? influxFields = null,
        IEnumerable<InfluxMeasurement>? influxMeasurements = null,
        IEnumerable<InfluxIdentificationInstance>? influxIdentificationInstances = null,
        FluxQueryPipe? pipe = null,
        CancellationToken cancellationToken = default
        )
    {
        string query = new FluxQueryBuilder()
            .AddBucket(_bucket)
            .AddRange(fluxRange)
            .AddAggregation(aggregateWindow)
            .AddMeasurementFilter(influxMeasurements?.Distinct())
            .AddFieldFilter(influxFields?.Distinct())
            .AddTagFilter(influxIdentificationInstances)
            .AddPipe(pipe)
            .Build();
        Console.WriteLine(query);

        try
        {
            var tables = await _queryApi.QueryAsync(query, Organization, cancellationToken);
            return FluxTablesToInfluxDatapoints(tables);
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine("Operation Canceled");
            Console.WriteLine(ex.Message);
            return new List<InfluxDatapoint>();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return new List<InfluxDatapoint>();
        }
    }

    public static List<InfluxDatapoint> FluxTablesToInfluxDatapoints(List<FluxTable> tables)
    {
        // TODO: optimize by keeping local List / HashSet of InfluxFields instead of loading from ObjectSpace

        using var scope = _serviceScopeFactory.CreateScope();
            var objectSpaceFactory = scope.ServiceProvider.GetService<INonSecuredObjectSpaceFactory>();
            var objectSpace = objectSpaceFactory.CreateNonSecuredObjectSpace<InfluxMeasurement>();
            var influxFields = objectSpace.GetObjects<InfluxField>();
            var influxTagKeys = objectSpace.GetObjects<InfluxTagKey>();
            
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
                        var influxTagKey = influxTagKeys.FirstOrDefault(x => x.Identifier == tag.Key && x.InfluxMeasurement.Identifier == record.GetMeasurement());
                        if (influxTagKey == null)
                        {
                            return;
                        }
                        var tagInfluxValue = new InfluxTagValue()
                        {
                            InfluxTagKey = influxTagKey,
                            Value = tag.Value.ToString()
                        };
                        tagInfluxValue.InfluxTagKey = influxTagKey;
                        tagInfluxValue.Value = tag.Value.ToString();
                        tagList.Add(tagInfluxValue);
                    }

                    InfluxDatapoint datapoint = new InfluxDatapoint((DateTime)record.GetTimeInDateTime(), record.GetValue());
                    datapoint.InfluxField = currentField;
                    datapoint.InfluxTagValues = tagList;
                    datapoints.Add(datapoint);
                });
            });
            return datapoints;
        }
    public static bool FluxRecordIsInfluxField(InfluxField field, FluxRecord record)
    {
        if (field == null || record == null)
        {
            return false;
        }
        var measurementName = record.GetMeasurement();
        var fieldIdentifier = record.GetField();
        //var influxIdentifier = field.InfluxMeasurement.AssetAdministrationShell.AssetCategory.InfluxIdentifier;
        //var assetId = field.InfluxMeasurement.AssetAdministrationShell.AssetId;
        var recordIsCurrentField = field.Identifier == fieldIdentifier &&
            field.InfluxMeasurement.Identifier == measurementName;
        //record.GetValueByKey(influxIdentifier).ToString() == assetId;
        return recordIsCurrentField;
    }
    #endregion

    #region Helper Functions
    private static string GetFieldIdentifier(InfluxField field, InfluxIdentificationInstance identification)
    {
        return GetFieldIdentifier(field, identification.InfluxTagValues);
    }

    private static string GetFieldIdentifier(InfluxField field, IList<InfluxTagValue> influxTagValues)
    {
        return $"{field.InfluxMeasurement.Identifier}_{field.Identifier}_{GetTagSetString(influxTagValues)}";
    }

    public static string GetTagSetString(IList<InfluxTagValue> influxTagValues)
    {
        var orderedInfluxTagValues = influxTagValues.OrderBy(x => x.InfluxTagKey.Identifier);
        return String.Join(",", orderedInfluxTagValues.Select(x => x.ToString()));
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

    #region Background Worker
    private BackgroundWorker worker;
    private void SetupBackgroundWorker()
    {
        var refreshRate = Environment.GetEnvironmentVariable("LAST_DATAPOINTS_REFRESH_RATE");

        if (!double.TryParse(refreshRate, out double rate))
            return;

        Console.WriteLine($"Setting up Background Worker with refresh rate of {rate}s");

        worker = new BackgroundWorker()
        {
            WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
        };
        worker.DoWork += worker_DoWork;
        worker.ProgressChanged += worker_ProgressChanged;
        worker.RunWorkerCompleted += worker_RunWorkerCompleted;

        var timer = new System.Timers.Timer(TimeSpan.FromSeconds(rate));
        timer.Elapsed += timer_Elapsed;
        timer.Start();
    }

    void timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        if (!worker.IsBusy)
            worker.RunWorkerAsync();
    }

    async void worker_DoWork(object sender, DoWorkEventArgs e)
    {
        BackgroundWorker w = (BackgroundWorker)sender;

        await RefreshLastDatapoints();
        await RefreshCachedQueries();
    }

    private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
        //display the progress using e.ProgressPercentage and/or e.UserState
    }

    private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        if (e.Cancelled)
        {
            //do something
        }
        else
        {
            //do something else
        }
    }
    #endregion
}
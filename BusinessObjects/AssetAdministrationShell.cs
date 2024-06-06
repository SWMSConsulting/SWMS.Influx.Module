using DevExpress.Data.Helpers;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using SWMS.Influx.Module.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWMS.Influx.Module.BusinessObjects
{
    // Register this entity in your DbContext (usually in the BusinessObjects folder of your project) with the "public DbSet<AssetAdministrationShell> AssetAdministrationShells { get; set; }" syntax.
    [DefaultClassOptions]
    //[ImageName("BO_Contact")]
    [DefaultProperty(nameof(AssetId))]
    [NavigationItem("Influx")]
    public class AssetAdministrationShell : BaseObject
    {
        public AssetAdministrationShell()
        {

        }

        public virtual string AssetId { get; set; }

        public virtual AssetCategory AssetCategory { get; set; }

        public virtual IList<InfluxMeasurement> InfluxMeasurements { get; set; } = new ObservableCollection<InfluxMeasurement>();

        [NotMapped]
        public BindingList<InfluxDatapoint> Datapoints { get; set; } = new BindingList<InfluxDatapoint>();

        public override void OnLoaded()
        {
            base.OnLoaded();
        }

        public virtual void OnInfluxFieldUpdated(InfluxField field) { }

        public virtual void OnDataRefreshed() { }

        public async Task RefreshData(DateTime? start = null, DateTime? end = null)
        {
            await GetMeasurements();

            foreach (var measurement in InfluxMeasurements)
            {
                await measurement.GetFields();
                foreach (var field in measurement.InfluxFields)
                {
                    await field.GetDatapoints(start, end);
                }
            }

            Datapoints = new BindingList<InfluxDatapoint>(
                InfluxMeasurements.SelectMany(measurement => measurement.InfluxFields.SelectMany(field => field.Datapoints)).ToList()
            );

            if (start == null || end == null)
            {
                OnDataRefreshed();
            }
            
            Console.WriteLine("Refreshed" + Datapoints.Count());
        }

        public async Task GetMeasurements()
        {
            if(AssetCategory == null || InfluxMeasurements.Count > 0)
            {
                return;
            }

            string bucket = Environment.GetEnvironmentVariable("INFLUX_BUCKET");
            var organization = Environment.GetEnvironmentVariable("INFLUX_ORG");

            var results = await InfluxDBService.QueryAsync(async query =>
            {
                // List measurements in bucket: https://docs.influxdata.com/influxdb/cloud/query-data/flux/explore-schema/
                // By default, this function returns results from the last 30 days.
                var flux = $"import \"influxdata/influxdb/schema\"\n" +
                            $"schema.measurements(" +
                            $"bucket: \"{bucket}\"," +
                            $")";
                try
                {
                    var tables = await query.QueryAsync(flux, organization);
                    return tables.SelectMany(table =>
                        table.Records.Select(record =>
                            new InfluxMeasurement
                            {
                                Name = record.GetValueByKey("_value").ToString(),
                                AssetAdministrationShell = this,
                            }));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return new ObservableCollection<InfluxMeasurement>();
                }

            });

            ObservableCollection<InfluxMeasurement> iotMeasurementsWithAssetId = new();

            foreach(var measurement in InfluxMeasurements)
            {
                ObjectSpace.Delete(measurement);
            }

            foreach (var measurement in results)
            {
                //Console.WriteLine(measurement.Name);
                if (AssetCategory == null)
                {
                    return;
                }
                var isAssetIdInTags = await InfluxDBService.QueryAsync(async query =>
                {
                    // List measurements in bucket: https://docs.influxdata.com/influxdb/cloud/query-data/flux/explore-schema/
                    // By default, this function returns results from the last 30 days.
                    var flux = $"import \"influxdata/influxdb/schema\"\n" +
                                $"schema.measurementTagValues(" +
                                $"bucket: \"{bucket}\"," +
                                $"tag: \"{AssetCategory.InfluxIdentifier}\"," +
                                $"measurement: \"{measurement.Name}\"," +
                                $")";
                    List<string> tagsInMeasurement = new();
                    var tables = await query.QueryAsync(flux, organization);
                    /*
                    tables.ForEach(table =>
                    {
                        table.Records.ForEach(record =>
                        {
                            Console.WriteLine($"{record.GetValueByKey("_value")}");
                            tagsInMeasurement.Add(record.GetValueByKey("_value").ToString());
                        });
                    });
                    */
                    return tagsInMeasurement.Contains(AssetId);
                });

                if (isAssetIdInTags)
                {
                    var createdMeasurement = ObjectSpace.CreateObject<InfluxMeasurement>();
                    createdMeasurement.Name = measurement.Name;
                    createdMeasurement.AssetAdministrationShell = measurement.AssetAdministrationShell;
                }
            }

            ObjectSpace.CommitChanges();

        }

    }
}
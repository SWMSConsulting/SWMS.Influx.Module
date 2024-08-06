using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using SWMS.Influx.Module.Models;
using SWMS.Influx.Module.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SWMS.Influx.Module.BusinessObjects
{
    [DefaultClassOptions]
    [NavigationItem("Influx")]
    //[ImageName("BO_Contact")]
    [DefaultProperty("Name")]
    public class AssetCategory : BaseObject
    {     
        public virtual string Name { get; set; }
        public virtual string RangeStart { get; set; }
        public virtual string RangeEnd { get; set; }
        public virtual string AggregateWindow { get; set; }
        public virtual FluxAggregateFunction AggregateFunction { get; set; }
        public virtual IList<InfluxIdentificationTemplate> InfluxIdentificationTemplates { get; set; } = new ObservableCollection<InfluxIdentificationTemplate>();
        public virtual IList<AssetAdministrationShell> AssetAdministrationShells { get; set; } = new ObservableCollection<AssetAdministrationShell>();

        [Browsable(false)]
        [RuleFromBoolProperty("ValidFluxDuration", DefaultContexts.Save, "Invalid flux duration!", UsedProperties = "AggregateWindow")]
        public bool AggregateWindowIsValid
        {
            get
            {
                return InfluxDBService.IsValidFluxDuration(AggregateWindow);
            }
        }


        // TODO: move influx schema loading
        [Action(
            Caption = "Refresh Measurements",
            AutoCommit = true,
            ImageName = "Action_Refresh"
        )]
        public async Task GetMeasurements()
        {
            string bucket = Environment.GetEnvironmentVariable("INFLUX_BUCKET");
            var organization = Environment.GetEnvironmentVariable("INFLUX_ORG");

            var results = await InfluxDBService.QueryAsync(async query =>
            {
                // List measurements in bucket: https://docs.influxdata.com/influxdb/cloud/query-data/flux/explore-schema/
                // By default, this function returns results from the last 30 days.
                var flux = $"import \"influxdata/influxdb/schema\"\n" +
                            $"schema.measurements(" +
                            $"bucket: \"{bucket}\"" +
                            $")";
                try
                {
                    var tables = await query.QueryAsync(flux, organization);
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

            var allMeasurements = ObjectSpace.GetObjects<InfluxMeasurement>();

            foreach (var measurement in allMeasurements)
            {
                ObjectSpace.Delete(measurement);
            }

            foreach (var measurement in results)
            {
                var createdMeasurement = ObjectSpace.CreateObject<InfluxMeasurement>();
                createdMeasurement.Name = measurement.Name;
                await createdMeasurement.GetFields();
                await createdMeasurement.GetTagKeys();
            }

            ObjectSpace.CommitChanges();

        }
    }
}
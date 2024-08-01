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
    public abstract class AssetAdministrationShell : BaseObject
    {
        public AssetAdministrationShell()
        {

        }

        public abstract string AssetId { get; }

        //public virtual AssetCategory AssetCategory { get; set; }
        private AssetCategory _AssetCategory;
        public virtual AssetCategory AssetCategory
        { 
            get
            {
                return _AssetCategory;
            } 
            set
            {
                _AssetCategory = value;
                CreateInfluxIdentificationInstances();
                ObjectSpace.CommitChanges();
            } 
        }

        [ExpandObjectMembers(ExpandObjectMembers.InListView)]
        public virtual IList<InfluxIdentificationInstance> InfluxIdentificationInstances { get; set; } = new ObservableCollection<InfluxIdentificationInstance>();

        // TODO: move influx schema loading
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

            foreach(var measurement in allMeasurements)
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

        public void CreateInfluxIdentificationInstances()
        {
            if (AssetCategory == null)
            {
                return;
            }
            foreach (var instance in InfluxIdentificationInstances)
            {
                ObjectSpace.Delete(instance);
            }
            var influxIdentificationTemplates = AssetCategory.InfluxIdentificationTemplates;
            foreach (var influxIdentificationTemplate in influxIdentificationTemplates)
            {
                var instance = ObjectSpace.CreateObject<InfluxIdentificationInstance>();
                instance.AssetAdministrationShell = this;
                instance.InfluxIdentificationTemplate = influxIdentificationTemplate;
                var bindings = influxIdentificationTemplate.InfluxTagKeyPropertyBindings;
                foreach (var binding in bindings)
                {
                    var influxTagValue = new InfluxTagValue(binding, this);
                    var objectSpaceInfluxTagValue = ObjectSpace.CreateObject<InfluxTagValue>();
                    objectSpaceInfluxTagValue.InfluxTagKey = binding.InfluxTagKey;
                    objectSpaceInfluxTagValue.Value = influxTagValue.Value;
                    binding.InfluxTagKey.InfluxTagValues.Add(objectSpaceInfluxTagValue);
                    instance.InfluxTagValues.Add(objectSpaceInfluxTagValue);
                }
                InfluxIdentificationInstances.Add(instance);
            }
        }

    }
}
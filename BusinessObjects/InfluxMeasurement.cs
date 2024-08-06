using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using SWMS.Influx.Module.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SWMS.Influx.Module.BusinessObjects
{
    [DefaultClassOptions]
    //[ImageName("BO_Contact")]
    [DefaultProperty(nameof(Name))]
    [NavigationItem("Influx")]
    public class InfluxMeasurement : BaseObject
    {
        public virtual string Name { get; set; }
        public virtual IList<InfluxField> InfluxFields { get; set; } = new ObservableCollection<InfluxField>();
        public virtual IList<InfluxTagKey> InfluxTagKeys { get; set; } = new ObservableCollection<InfluxTagKey>();
        public virtual IList<InfluxIdentificationTemplate> InfluxIdentificationTemplates { get; set; } = new ObservableCollection<InfluxIdentificationTemplate>();


        public async Task GetFields()
        {
            string bucket = Environment.GetEnvironmentVariable("INFLUX_BUCKET");
            var organization = Environment.GetEnvironmentVariable("INFLUX_ORG");
            var measurement = Name;

            var results = await InfluxDBService.QueryAsync(async query =>
            {
                // List fields for measurement in bucket: https://docs.influxdata.com/influxdb/cloud/query-data/flux/explore-schema/
                // By default, this function returns results from the last 30 days.
                var flux = $"import \"influxdata/influxdb/schema\"\n" +
                            $"schema.measurementFieldKeys(" +
                            $"bucket: \"{bucket}\"," +
                            $"measurement: \"{measurement}\"," +
                            $")";

                var tables = await query.QueryAsync(flux, organization);
                
                return tables.SelectMany(table =>
                    table.Records.Select(record =>
                        new InfluxField
                        {
                            Name = record.GetValueByKey("_value").ToString()
                        }));
            });

            foreach(var result in results)
            {
                if(InfluxFields.Any(f => f.Name == result.Name))
                {
                    continue;
                }

                var field = ObjectSpace.CreateObject<InfluxField>();
                field.Name = result.Name;
                InfluxFields.Add(field);
            }

            ObjectSpace.CommitChanges();

        }

        public async Task GetTagKeys()
        {
            string bucket = Environment.GetEnvironmentVariable("INFLUX_BUCKET");
            var organization = Environment.GetEnvironmentVariable("INFLUX_ORG");
            var measurement = Name;

            var results = await InfluxDBService.QueryAsync(async query =>
            {
                // List tags for measurement in bucket: https://docs.influxdata.com/influxdb/cloud/query-data/flux/explore-schema/
                // By default, this function returns results from the last 30 days.
                var flux = $"import \"influxdata/influxdb/schema\"\n" +
                            $"schema.measurementTagKeys(" +
                            $"bucket: \"{bucket}\"," +
                            $"measurement: \"{measurement}\"," +
                            $")";

                var tables = await query.QueryAsync(flux, organization);
                
                return tables.SelectMany(table =>
                    table.Records.Select(record =>
                        new InfluxTagKey
                        {
                            Name = record.GetValueByKey("_value").ToString()
                        }));
            });

            foreach(var result in results)
            {
                if (InfluxTagKeys.Any(f => f.Name == result.Name) || result.Name.StartsWith("_"))
                {
                    continue;
                }
                var tag = ObjectSpace.CreateObject<InfluxTagKey>();
                tag.Name = result.Name;
                InfluxTagKeys.Add(tag);
            }

            ObjectSpace.CommitChanges();

        }


    }
}
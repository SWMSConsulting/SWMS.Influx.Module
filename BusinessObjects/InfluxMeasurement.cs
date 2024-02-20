using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using SWMS.Influx.Module.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SWMS.Influx.Module.BusinessObjects
{
    // Register this entity in your DbContext (usually in the BusinessObjects folder of your project) with the "public DbSet<InfluxMeasurement> InfluxMeasurements { get; set; }" syntax.
    [DefaultClassOptions]
    //[ImageName("BO_Contact")]
    [DefaultProperty(nameof(Name))]
    public class InfluxMeasurement : BaseObject
    {
        public InfluxMeasurement()
        {

        }        

        public virtual string Name { get; set; }
        public virtual AssetAdministrationShell AssetAdministrationShell { get; set; }
        public virtual IList<InfluxField> InfluxFields { get; set; } = new ObservableCollection<InfluxField>();


        public async Task GetFields()
        {
            Console.WriteLine("GetFields");
            string bucket = Environment.GetEnvironmentVariable("INFLUX_BUCKET");
            var organization = Environment.GetEnvironmentVariable("INFLUX_ORG");
            var measurement = Name;

            foreach(var field in InfluxFields)
            {
                ObjectSpace.Delete(field);                
            }

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
                tables.ForEach(table =>
                {
                    table.Records.ForEach(record =>
                    {
                        Console.WriteLine($"{record.GetValueByKey("_value")}");
                    });
                });
                return tables.SelectMany(table =>
                    table.Records.Select(record =>
                        new InfluxField
                        {
                            Name = record.GetValueByKey("_value").ToString(),
                            InfluxMeasurement = this,
                        }));
            });

            foreach(var result in results)
            {
                var field = ObjectSpace.CreateObject<InfluxField>();
                field.Name = result.Name;
                field.InfluxMeasurement = result.InfluxMeasurement;
                InfluxFields.Add(field);
            }

            ObjectSpace.CommitChanges();

        }


    }
}
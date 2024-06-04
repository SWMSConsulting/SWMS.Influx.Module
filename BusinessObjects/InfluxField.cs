using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using DevExpress.Xpo.Logger.Transport;
using SWMS.Influx.Module.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

namespace SWMS.Influx.Module.BusinessObjects
{
    // Register this entity in your DbContext (usually in the BusinessObjects folder of your project) with the "public DbSet<InfluxField> InfluxFields { get; set; }" syntax.
    [DefaultClassOptions]
    //[ImageName("BO_Contact")]
    [DefaultProperty(nameof(Name))]
    [NavigationItem("Influx")]
    public class InfluxField : BaseObject, INotifyPropertyChanged
    {
        public InfluxField()
        {

        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }        

        public virtual string Name { get; set; }
        [ExpandObjectMembers(ExpandObjectMembers.InListView)]
        public virtual InfluxMeasurement InfluxMeasurement { get; set; }


        private BindingList<InfluxDatapoint> _Datapoints;
        [NotMapped]
        public BindingList<InfluxDatapoint> Datapoints
        {
            get { return _Datapoints; }
            set
            {
                if (_Datapoints != value)
                {
                    _Datapoints = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public async Task<BindingList<InfluxDatapoint>> GetDatapoints()
        {
            Console.WriteLine("GetDatapoints");
            string bucket = Environment.GetEnvironmentVariable("INFLUX_BUCKET");
            var organization = Environment.GetEnvironmentVariable("INFLUX_ORG");
            var measurement = InfluxMeasurement.Name;
            var field = this.Name;

            var results = await InfluxDBService.QueryAsync(async query =>
            {
                // TODO: data range based on user input and dynamic aggregateWindow
                var flux = $"from(bucket:\"{bucket}\") " +
                            $"|> range(start: -{InfluxMeasurement.AssetAdministrationShell.AssetCategory.Duration}h) " +
                            $"|> filter(fn: (r) => " +
                            $"r._measurement == \"{measurement}\" and " +
                            $"r._field == \"{field}\" and " +
                            $"r.{InfluxMeasurement.AssetAdministrationShell.AssetCategory.InfluxIdentifier} == \"{InfluxMeasurement.AssetAdministrationShell.AssetId}\"" +
                            $")" +
                            $"|> aggregateWindow(every: 1m, fn: mean)";

                List<InfluxDatapoint> datapoints = new ();

                var tables = await query.QueryAsync(flux, organization);
                tables.ForEach(table =>
                {
                    table.Records.ForEach(record =>
                    {
                        if (record.GetValueByKey("_value") != null)
                        {
                            InfluxDatapoint datapoint = new InfluxDatapoint()
                            {
                                Value = double.TryParse(record.GetValueByKey("_value").ToString(), out double v) ? v : 0.0,
                                Time = XmlConvert.ToDateTime(record.GetValueByKey("_time").ToString(), XmlDateTimeSerializationMode.Local),
                                InfluxField = this,
                            };
                            datapoints.Add(datapoint);
                        }
                    });
                });
                return datapoints;

            });

            Datapoints = new BindingList<InfluxDatapoint>(results);
            return Datapoints;

        }

        public string GetFullName()
        {
            return $"{InfluxMeasurement.AssetAdministrationShell.AssetId} - {InfluxMeasurement.Name} - {Name}";
        }

        #region INotifyPropertyChanged members (see http://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged(v=vs.110).aspx)
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

    }
}
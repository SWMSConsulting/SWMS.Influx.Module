using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using SWMS.Influx.Module.Services;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
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

#nullable enable
        [NotMapped]
        [VisibleInListView(false)]
        [VisibleInDetailView(false)]
        [VisibleInLookupListView(false)]
        public InfluxDatapoint? LatestDatapoint
        {
            get
            {
                if(Datapoints == null || Datapoints.Count < 1)
                {
                    return null;
                }
                return Datapoints.OrderByDescending(d => d.Time).FirstOrDefault();
            }
        }
        #nullable disable

        public async Task<BindingList<InfluxDatapoint>> GetDatapoints(DateTime? start = null, DateTime? end = null)
        {
            string bucket = Environment.GetEnvironmentVariable("INFLUX_BUCKET");
            var organization = Environment.GetEnvironmentVariable("INFLUX_ORG");
            var measurement = InfluxMeasurement.Name;
            var field = this.Name;

            var results = await InfluxDBService.QueryAsync(async query =>
            {
                // TODO: data range based on user input and dynamic aggregateWindow
                var flux = GetFluxQuery(
                    bucket, 
                    measurement, 
                    field, 
                    InfluxMeasurement.AssetAdministrationShell.AssetId, 
                    InfluxMeasurement.AssetAdministrationShell.AssetCategory,
                    start,
                    end
                );
                // Console.WriteLine(flux);
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

            if(start == null || end == null)
            {
                InfluxMeasurement?.AssetAdministrationShell?.OnInfluxFieldUpdated(this);
            }

            return Datapoints;

        }


        public string GetFluxQuery(
            string bucket, 
            string measurement, 
            string field, 
            string assetId, 
            AssetCategory assetCategory,
            DateTime? start = null,
            DateTime? end = null
        )
        {
            string rangeStart = start == null ? assetCategory.RangeStart : start.Value.ToString("yyyy-MM-dd");

            string query = $"from(bucket:\"{bucket}\") " +
                $"|> range(start: {rangeStart}) " +
                $"|> filter(fn: (r) => " +
                $"r._measurement == \"{measurement}\" and " +
                $"r._field == \"{field}\" and " +
                $"r.{assetCategory.InfluxIdentifier} == \"{assetId}\"" +
                $")" +
                $"|> aggregateWindow(every: {assetCategory.AggregateWindow}, fn: {assetCategory.AggregateFunction})" +
                "|> group(columns: [\"_field\", \"_time\"])" +
                "|> sum()";
            //Console.WriteLine(query);
            return query;
        }

        #region INotifyPropertyChanged members (see http://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged(v=vs.110).aspx)
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

    }
}
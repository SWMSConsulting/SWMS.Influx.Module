using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Xpo;
using SWMS.Influx.Module.Models;
using SWMS.Influx.Module.Services;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace SWMS.Influx.Module.BusinessObjects
{
    // Register this entity in your DbContext (usually in the BusinessObjects folder of your project) with the "public DbSet<InfluxField> InfluxFields { get; set; }" syntax.
    [DefaultClassOptions]
    //[ImageName("BO_Contact")]
    [DefaultProperty(nameof(Name))]
    [NavigationItem("Influx")]
    public class InfluxField : BaseObject, INotifyPropertyChanged
    {
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }        

        public virtual string Name { get; set; }
        [ExpandObjectMembers(ExpandObjectMembers.InListView)]
        public virtual InfluxMeasurement InfluxMeasurement { get; set; }


        private BindingList<InfluxDatapoint> _Datapoints = new();
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
        public string GlobalIdentifer
        {
            get
            {
                return string.Join(" - ", [
                    //InfluxMeasurement.AssetCategories.AssetId,
                    InfluxMeasurement.Name,
                    Name
                ]);
            }
        }

        [NotMapped]
        [VisibleInListView(false)]
        [VisibleInDetailView(false)]
        [VisibleInLookupListView(false)]
        public InfluxDatapoint? LastDatapoint
        {
            get
            {
                InfluxDBService? influxService = ObjectSpace.ServiceProvider.GetService(typeof(InfluxDBService)) as InfluxDBService;
                return influxService?.GetLastDatapointForField(this);
            }
        }

        public async Task<BindingList<InfluxDatapoint>> GetDatapoints(
            FluxRange fluxRange,
            FluxAggregateWindow? aggregateWindow = null
            )
        {
            /*
            var measurement = InfluxMeasurement.Name;
            var field = this.Name;
            var influxIdentifier = InfluxMeasurement.AssetAdministrationShell.AssetCategory.InfluxIdentifier;
            var assetId = InfluxMeasurement.AssetAdministrationShell.AssetId;
            var filters = new Dictionary<string, List<string>>
            {
                { "_measurement", new List<string>(){ measurement } },
                { "_field", new List<string>(){ field } },
                { influxIdentifier, new List<string>(){ assetId } },
            };

            InfluxDBService? influxService = ObjectSpace.ServiceProvider.GetService(typeof(InfluxDBService)) as InfluxDBService;
            if (influxService == null)
            {
                return new BindingList<InfluxDatapoint>();
            }
            var datapoints = await influxService.QueryInfluxDatapoints(
                fluxRange: fluxRange,
                filters: filters,
                aggregateWindow: aggregateWindow
            );

            Datapoints = new BindingList<InfluxDatapoint>(datapoints);

            return Datapoints;
            */
            return new BindingList<InfluxDatapoint>();
        }   
#nullable disable     

        #region INotifyPropertyChanged members (see http://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged(v=vs.110).aspx)
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

    }
}
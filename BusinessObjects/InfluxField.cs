using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
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
        //[NotMapped]
        //[VisibleInListView(false)]
        //[VisibleInDetailView(false)]
        //[VisibleInLookupListView(false)]
        //public InfluxDatapoint? LastDatapoint
        //{
        //    get
        //    {
        //        return InfluxDBService.GetLastDatapointForField(this);
        //    }
        //}
#nullable disable

        public async Task<BindingList<InfluxDatapoint>> GetDatapoints(
            FluxRange fluxRange,
            FluxAggregateWindow? aggregateWindow = null
            )
        {
            var measurement = InfluxMeasurement.Name;
            var field = this.Name;
            //var influxIdentifier = InfluxMeasurement.AssetAdministrationShell.AssetCategory.InfluxIdentifier;
            //var assetId = InfluxMeasurement.AssetAdministrationShell.AssetId;
            var filters = new Dictionary<string, List<string>>
            {
                { "_measurement", new List<string>(){ measurement } },
                { "_field", new List<string>(){ field } },
                //{ influxIdentifier, new List<string>(){ assetId } },
            };

            var datapoints = await InfluxDBService.QueryInfluxDatapoints(
                fluxRange: fluxRange,
                filters: filters,
                aggregateWindow: aggregateWindow
                );

            Datapoints = new BindingList<InfluxDatapoint>(datapoints);

            return Datapoints;

        }

        #region INotifyPropertyChanged members (see http://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged(v=vs.110).aspx)
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

    }
}
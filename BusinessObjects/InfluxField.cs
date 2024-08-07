using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using SWMS.Influx.Module.Models;
using SWMS.Influx.Module.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWMS.Influx.Module.BusinessObjects
{
    [DefaultClassOptions]
    //[ImageName("BO_Contact")]
    [DefaultProperty(nameof(Name))]
    [NavigationItem("Influx")]
    public class InfluxField : BaseObject
    { 

        public virtual string Name { get; set; }
        [ExpandObjectMembers(ExpandObjectMembers.InListView)]
        public virtual InfluxMeasurement InfluxMeasurement { get; set; }


        [NotMapped]
        public ObservableCollection<InfluxDatapoint> Datapoints { get; set; } = new ObservableCollection<InfluxDatapoint>();

        public async Task<ObservableCollection<InfluxDatapoint>> GetDatapoints(
            FluxRange fluxRange,
            FluxAggregateWindow? aggregateWindow = null,
            AssetAdministrationShell? assetAdministrationShell = null
        )
        {
            var filter = InfluxDBService.GetFilterForField(this, assetAdministrationShell);
            var datapoints = await InfluxDBService.QueryInfluxDatapoints(
                fluxRange: fluxRange,
                filters: filter,
                aggregateWindow: aggregateWindow
            );

            while (Datapoints.Count > 0)
            {
                Datapoints.RemoveAt(0);
            }
            datapoints.ForEach(Datapoints.Add);

            return Datapoints;
        }
    }
}
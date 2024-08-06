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

        public async Task<ObservableCollection<InfluxDatapoint>> GetDatapoints(
            FluxRange fluxRange,
            FluxAggregateWindow? aggregateWindow = null,
            AssetAdministrationShell? assetAdministrationShell = null
            )
        {
            var measurement = InfluxMeasurement.Name;
            var filters = new Dictionary<string, List<string>>
            {
                { "_measurement", new List<string>(){ measurement } },
                { "_field", new List<string>(){ Name } },
            };
            if(assetAdministrationShell != null)
            {
                assetAdministrationShell.InfluxIdentificationInstances.ToList().ForEach(instance =>
                {
                    instance.InfluxTagValues.ToList().ForEach(tagValue =>
                    {
                        if (tagValue.InfluxTagKey != null)
                        {
                            if (!filters.ContainsKey(tagValue.InfluxTagKey.Name))
                            {
                                filters[tagValue.InfluxTagKey.Name] = new List<string>();
                            }
                            filters[tagValue.InfluxTagKey.Name].Add(tagValue.Value);
                        }
                    });
                });
            }

            var datapoints = await InfluxDBService.QueryInfluxDatapoints(
                fluxRange: fluxRange,
                filters: filters,
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
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
    }
}
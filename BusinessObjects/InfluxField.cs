using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWMS.Influx.Module.BusinessObjects
{
    [DefaultClassOptions]
    [DefaultProperty(nameof(DisplayName))]
    [NavigationItem("Influx")]
    public class InfluxField : BaseObject
    {
        public static string ColumnName = "Fields";
        public virtual string Identifier { get; set; }

        public virtual string DisplayName { get; set; }
        
        public virtual InfluxMeasurement InfluxMeasurement { get; set; }

        [NotMapped]
        public ObservableCollection<InfluxDatapoint> Datapoints { get; set; } = new ObservableCollection<InfluxDatapoint>();
    }
}
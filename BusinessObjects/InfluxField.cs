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
    [ImageName("ChartType_Line")]
    public class InfluxField : BaseObject
    {
        public override void OnCreated()
        {
            IsVisibleInChart = true;
            IsVisibleInTable = true;

            base.OnCreated();
        }

        public static string ColumnName = "Field";
        public virtual string Identifier { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual bool IsVisibleInTable { get; set; }
        public virtual bool IsVisibleInChart { get; set; }
        public virtual InfluxMeasurement InfluxMeasurement { get; set; }
        public virtual IList<CachedQuery> CachedQueries { get; set; } = new ObservableCollection<CachedQuery>();

        [NotMapped]
        public ObservableCollection<InfluxDatapoint> Datapoints { get; set; } = new ObservableCollection<InfluxDatapoint>();
    }
}
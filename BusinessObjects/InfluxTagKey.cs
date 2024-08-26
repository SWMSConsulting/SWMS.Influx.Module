using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWMS.Influx.Module.BusinessObjects
{
    [DefaultClassOptions]
    [NavigationItem("Influx")]
    [DefaultProperty(nameof(Id))]
    public class InfluxTagKey : BaseObject
    {
        public virtual string Identifier { get; set; }
        public virtual InfluxMeasurement InfluxMeasurement { get; set; }
        public virtual IList<InfluxTagKeyPropertyBinding> InfluxTagKeyPropertyBindings { get; set; } = new ObservableCollection<InfluxTagKeyPropertyBinding>();
        public virtual IList<InfluxTagValue> InfluxTagValues { get; set; } = new ObservableCollection<InfluxTagValue>();


        [NotMapped]
        [VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public string Id => ToString();

        public override string ToString()
        {
            if (InfluxMeasurement == null)
            {
                return Identifier;
            }
            return $"{InfluxMeasurement.Identifier} - {Identifier}";
        }
    }
}
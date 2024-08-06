using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SWMS.Influx.Module.BusinessObjects
{
    [DefaultClassOptions]
    [NavigationItem("Influx")]
    [DefaultProperty(nameof(Id))]
    public class InfluxTagKey : BaseObject
    {
        public string Id => ToString();

        public virtual string Name { get; set; }
        [ExpandObjectMembers(ExpandObjectMembers.InListView)]
        public virtual InfluxMeasurement InfluxMeasurement { get; set; }
        public virtual IList<InfluxTagKeyPropertyBinding> InfluxTagKeyPropertyBindings { get; set; } = new ObservableCollection<InfluxTagKeyPropertyBinding>();
        public virtual IList<InfluxTagValue> InfluxTagValues { get; set; } = new ObservableCollection<InfluxTagValue>();
        public override string ToString()
        {
            if (InfluxMeasurement == null)
            {
                return "";
            }
            return $"{InfluxMeasurement.Name} - {Name}";
        }
    }
}
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWMS.Influx.Module.BusinessObjects
{
    [DefaultClassOptions]
    [NavigationItem("Influx")]
    [DefaultProperty(nameof(Name))]
    public class InfluxIdentificationTemplate : BaseObject
    {
        [RuleRequiredField(DefaultContexts.Save)]
        public virtual AssetCategory AssetCategory { get; set; }

        [RuleRequiredField(DefaultContexts.Save)]
        public virtual InfluxMeasurement InfluxMeasurement { get; set; }

        [DataSourceProperty("InfluxMeasurement.InfluxTagKeys", DataSourcePropertyIsNullMode.SelectAll)]
        [Aggregated]
        public virtual IList<InfluxTagKeyPropertyBinding> InfluxTagKeyPropertyBindings { get; set; } = new ObservableCollection<InfluxTagKeyPropertyBinding>();

        [Aggregated]
        public virtual IList<InfluxIdentificationInstance> InfluxIdentificationInstances { get; set; } = new ObservableCollection<InfluxIdentificationInstance>();

        [NotMapped]
        public string Name => ToString();
        public override string ToString()
        {
            if(InfluxMeasurement == null || AssetCategory == null)
            {
                return "";
            }
            return $"{InfluxMeasurement.Name} - {AssetCategory.Name}";
        }
    }
}
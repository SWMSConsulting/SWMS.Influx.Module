using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWMS.Influx.Module.BusinessObjects
{
    [DefaultClassOptions]
    [NavigationItem("Influx")]
    public class InfluxTagKeyPropertyBinding : BaseObject
    {    
        public virtual InfluxIdentificationTemplate InfluxIdentificationTemplate { get; set; }
        
        [RuleRequiredField(DefaultContexts.Save)]
        [DataSourceProperty(nameof(AvailableInfluxTags))]
        public virtual InfluxTagKey InfluxTagKey { get; set; }

        [RuleRequiredField(DefaultContexts.Save)]
        public virtual string ImplementingClassPropertyName { get; set; }

        [NotMapped]
        [VisibleInDetailView(false), VisibleInListView(false), VisibleInLookupListView(false)]
        public IList<InfluxTagKey> AvailableInfluxTags => InfluxIdentificationTemplate?.InfluxMeasurement?.InfluxTagKeys ?? new List<InfluxTagKey>();
    }
}
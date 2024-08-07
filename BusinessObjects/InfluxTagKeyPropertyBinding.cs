using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;

namespace SWMS.Influx.Module.BusinessObjects
{
    [DefaultClassOptions]
    [NavigationItem("Influx")]
    public class InfluxTagKeyPropertyBinding : BaseObject
    {    
        public virtual InfluxIdentificationTemplate InfluxIdentificationTemplate { get; set; }
        
        [RuleRequiredField(DefaultContexts.Save)]
        public virtual InfluxTagKey InfluxTagKey { get; set; }

        [RuleRequiredField(DefaultContexts.Save)]
        public virtual string ImplementingClassPropertyName { get; set; }

    }
}
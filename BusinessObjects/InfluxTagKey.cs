using DevExpress.Persistent.BaseImpl.EF;

namespace SWMS.Influx.Module.BusinessObjects
{
    public class InfluxTagKey: BaseObject
    {
        public virtual string Name { get; set; }
        public virtual InfluxMeasurement InfluxMeasurement { get; set; }

        public virtual IList<InfluxIdentificationTemplate> InfluxIdentificationTemplates { get; set; }
    }
}

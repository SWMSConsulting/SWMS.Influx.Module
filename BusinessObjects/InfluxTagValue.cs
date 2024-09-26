using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using FastMember;
using SWMS.Influx.Module.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWMS.Influx.Module.BusinessObjects
{
    [DefaultClassOptions]
    [NavigationItem("Influx")]
    [NotMapped, DomainComponent]
    public class InfluxTagValue: NonPersistentBaseObject
    {
        public InfluxTagValue(InfluxTagKey influxTagKey, string value)
        {
            InfluxTagKey = influxTagKey;
            Value = value;
        }

        public InfluxTagValue(InfluxTagKeyPropertyBinding binding, object assetAdministrationShell)
        {
            string propName = binding.ImplementingClassPropertyName;
            if (assetAdministrationShell.GetType().GetProperty(propName) == null)
            {
                return;
            } 
            var wrapped = ObjectAccessor.Create(assetAdministrationShell);
            var propValue = wrapped[propName];
            InfluxTagKey = binding.InfluxTagKey;
            Value = propValue?.ToString();
        }

        public InfluxTagKey InfluxTagKey { get; set; }

        public string Value { get; set; }
       
        public override string ToString()
        {
            if(InfluxTagKey == null)
            {
                return "";
            }
            return InfluxDBService.KeyValuePairToString(InfluxTagKey.Identifier, Value);
        }
    }
}
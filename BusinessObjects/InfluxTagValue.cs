using DevExpress.Persistent.Base;
using FastMember;
using SWMS.Influx.Module.Services;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWMS.Influx.Module.BusinessObjects
{
    [NotMapped]
    [DefaultClassOptions]
    [NavigationItem("Influx")]
    public class InfluxTagValue 
    {
        public InfluxTagValue() { }
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

        public KeyValuePair<string, object> KeyValuePair => new(InfluxTagKey.Identifier , Value);
       
        public override string ToString()
        {
            return InfluxDBService.KeyValuePairToString(InfluxTagKey.Identifier, Value);
        }
    }
}
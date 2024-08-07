using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using SWMS.Influx.Module.Services;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWMS.Influx.Module.BusinessObjects
{
    [DefaultClassOptions]
    [NavigationItem("Influx")]
    public class InfluxIdentificationInstance : BaseObject
    {
        public virtual AssetAdministrationShell AssetAdministrationShell { get; set; }
        public virtual InfluxIdentificationTemplate InfluxIdentificationTemplate { get; set; }

        public virtual IList<InfluxTagValue> InfluxTagValues { get; set; } = new ObservableCollection<InfluxTagValue>();

        [NotMapped]
        public InfluxMeasurement InfluxMeasurement => InfluxIdentificationTemplate?.InfluxMeasurement;

        [NotMapped]
        public string TagSetString => InfluxDBService.GetTagSetString(InfluxTagValues);
        
        [NotMapped]
        public List<KeyValuePair<string, object>> TagKeyValuePairs => InfluxTagValues.Select(x => x.KeyValuePair).ToList();

        [NotMapped]
        public Dictionary<string, List<string>> Filter => InfluxDBService.GetFilterForTagValues(InfluxTagValues);
    }
}
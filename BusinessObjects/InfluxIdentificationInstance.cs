using DevExpress.Persistent.Base;
using SWMS.Influx.Module.Services;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWMS.Influx.Module.BusinessObjects
{
    [NotMapped]
    [NavigationItem("Influx")]
    [ImageName("ChartType_Line")]
    public class InfluxIdentificationInstance 
    {
        public InfluxIdentificationTemplate InfluxIdentificationTemplate { get; set; }

        public IList<InfluxTagValue> InfluxTagValues { get; set; } = new ObservableCollection<InfluxTagValue>();

        public InfluxMeasurement InfluxMeasurement => InfluxIdentificationTemplate?.InfluxMeasurement;

        public string TagSetString => InfluxDBService.GetTagSetString(InfluxTagValues);
        
        public List<KeyValuePair<string, object>> TagKeyValuePairs => InfluxTagValues.Select(x => x.KeyValuePair).ToList();
    }
}
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
        public AssetAdministrationShell AssetAdministrationShell { get; set; }
        public InfluxIdentificationTemplate InfluxIdentificationTemplate { get; set; }

        public IList<InfluxTagValue> InfluxTagValues { get; set; } = new ObservableCollection<InfluxTagValue>();

        public InfluxMeasurement InfluxMeasurement => InfluxIdentificationTemplate?.InfluxMeasurement;

        public string TagSetString => InfluxDBService.GetTagSetString(InfluxTagValues);
    }
}
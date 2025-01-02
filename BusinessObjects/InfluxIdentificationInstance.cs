using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using SWMS.Influx.Module.Services;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWMS.Influx.Module.BusinessObjects
{
    [NavigationItem("Influx")]
    [ImageName("ChartType_Line")]
    [NotMapped, DomainComponent]
    public class InfluxIdentificationInstance: NonPersistentBaseObject
    {
        public AssetAdministrationShell AssetAdministrationShell { get; set; }
        public InfluxIdentificationTemplate InfluxIdentificationTemplate { get; set; }

        public IList<InfluxTagValue> InfluxTagValues { get; set; } = new ObservableCollection<InfluxTagValue>();

        public InfluxMeasurement InfluxMeasurement => InfluxIdentificationTemplate?.InfluxMeasurement;

        public string TagSetString => InfluxDBService.GetTagSetString(InfluxTagValues);
    }
}
using DevExpress.Persistent.BaseImpl.EF;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWMS.Influx.Module.BusinessObjects;

public class InfluxIdentificationTemplate: BaseObject
{
    public virtual AssetCategory AssetCategory { get; set; }

    public virtual InfluxTagKey InfluxTagKey { get; set; }

    public virtual string AssetAdministrationShellBinding { get; set; }

    [NotMapped]
    public InfluxMeasurement InfluxMeasurement => InfluxTagKey.InfluxMeasurement; 
}

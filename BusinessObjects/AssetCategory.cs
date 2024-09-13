using Aqua.EnumerableExtensions;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using SWMS.Influx.Module.Models;
using SWMS.Influx.Module.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWMS.Influx.Module.BusinessObjects
{
    [DefaultClassOptions]
    [NavigationItem("Influx")]
    //[ImageName("BO_Contact")]
    [DefaultProperty("Name")]
    public class AssetCategory : BaseObject
    {     
        public virtual string Name { get; set; }

        [Aggregated]
        public virtual IList<InfluxIdentificationTemplate> InfluxIdentificationTemplates { get; set; } = new ObservableCollection<InfluxIdentificationTemplate>();
        public virtual IList<AssetAdministrationShell> AssetAdministrationShells { get; set; } = new ObservableCollection<AssetAdministrationShell>();

        [DataSourceProperty(nameof(InfluxFields))]
        public virtual ObservableCollection<InfluxField> RelevantInfluxFields { get; set; } = new ObservableCollection<InfluxField>();

        [NotMapped]
        public IList<InfluxMeasurement> InfluxMeasurements => InfluxIdentificationTemplates.Select(t => t.InfluxMeasurement).Distinct().ToList();


        [NotMapped]
        public IList<InfluxField> InfluxFields => InfluxMeasurements.SelectMany(m => m.InfluxFields).Distinct().ToList();

        [Action(
            Caption = "Update all Identification Instances"
        )]
        public void UpdateAllIdentificationInstances()
        {
            AssetAdministrationShells.ForEach(shell => shell.UpdateIdentificationInstancesAndSave());
        }
    }
}
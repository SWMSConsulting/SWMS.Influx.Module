using Aqua.EnumerableExtensions;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using SWMS.Influx.Module.Services;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWMS.Influx.Module.BusinessObjects
{
    //[DefaultClassOptions]
    //[ImageName("BO_Contact")]
    [NavigationItem("Influx")]
    public abstract class AssetAdministrationShell : BaseObject
    {
        public abstract string Caption { get; }

        private AssetCategory _assetCategory;
        [RuleRequiredField(DefaultContexts.Save)]
        public virtual AssetCategory AssetCategory { 
            get => _assetCategory;
            set
            {
                _assetCategory = value;
                CreateInfluxIdentificationInstances(false);
            }
        }
    

        [ExpandObjectMembers(ExpandObjectMembers.InListView)]
        [Aggregated]
        public virtual IList<InfluxIdentificationInstance> InfluxIdentificationInstances { get; set; } = new ObservableCollection<InfluxIdentificationInstance>();

        [NotMapped]
        public IList<InfluxMeasurement> InfluxMeasurements => AssetCategory?.InfluxMeasurements;

        public InfluxIdentificationInstance? GetInfluxIdentificationInstanceForMeasurement(string measurement)
        {
            return InfluxIdentificationInstances.Where(i => i.InfluxMeasurement?.Identifier == measurement).FirstOrDefault();
        }

        public double? GetLastDatapoint(string measurement, string field)
        {
            var identification = GetInfluxIdentificationInstanceForMeasurement(measurement);
            if (identification == null)
                return null;

            var influxField = identification?.InfluxMeasurement.InfluxFields.Where(f => f.Identifier == field).FirstOrDefault();
            if (influxField == null)
                return null;

            return InfluxDBService.GetLastDatapointForField(influxField, identification)?.Value;
        }


        [Action(
            Caption = "Update Identification",
            AutoCommit = true,
            ImageName = "Action_Refresh"
        )]
        public void CreateInfluxIdentificationInstances() => CreateInfluxIdentificationInstances(true);
        public void CreateInfluxIdentificationInstances(bool commitChanges = true)
        {
            while (InfluxIdentificationInstances.Count > 0)
            {
                var inst = InfluxIdentificationInstances[0];
                InfluxIdentificationInstances.Remove(inst);
                ObjectSpace.Delete(inst);
            }

            if (AssetCategory == null)
            {
                return;
            }

            var influxIdentificationTemplates = AssetCategory.InfluxIdentificationTemplates;
            foreach (var influxIdentificationTemplate in influxIdentificationTemplates)
            {
                var instance = ObjectSpace.CreateObject<InfluxIdentificationInstance>();
                instance.AssetAdministrationShell = this;
                instance.InfluxIdentificationTemplate = influxIdentificationTemplate;
                var bindings = influxIdentificationTemplate.InfluxTagKeyPropertyBindings;
                foreach (var binding in bindings)
                {
                    var influxTagValue = new InfluxTagValue(binding, this);
                    var objectSpaceInfluxTagValue = ObjectSpace.CreateObject<InfluxTagValue>();
                    objectSpaceInfluxTagValue.InfluxTagKey = binding.InfluxTagKey;
                    objectSpaceInfluxTagValue.Value = influxTagValue.Value;
                    binding.InfluxTagKey.InfluxTagValues.Add(objectSpaceInfluxTagValue);
                    instance.InfluxTagValues.Add(objectSpaceInfluxTagValue);
                }
                InfluxIdentificationInstances.Add(instance);

            }
            if(commitChanges)
            {
                ObjectSpace.CommitChanges();
                ObjectSpace.Refresh();
            }
        }
    }
}
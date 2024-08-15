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
        [RuleRequiredField]
        public virtual AssetCategory AssetCategory { get; set; }
    

        [ExpandObjectMembers(ExpandObjectMembers.InListView)]
        [Aggregated]
        public virtual IList<InfluxIdentificationInstance> InfluxIdentificationInstances { get; set; } = new ObservableCollection<InfluxIdentificationInstance>();

        [NotMapped]
        public IList<InfluxMeasurement> InfluxMeasurements => AssetCategory?.InfluxMeasurements;

        public InfluxIdentificationInstance? GetInfluxIdentificationInstanceForMeasurement(string measurement)
        {
            return InfluxIdentificationInstances.Where(i => i.InfluxMeasurement?.Name == measurement).FirstOrDefault();
        }

        public InfluxDatapoint? GetLastDatapoint(string measurement, string field)
        {
            var identification = GetInfluxIdentificationInstanceForMeasurement(measurement);
            if (identification == null)
                return null;

            var influxField = identification?.InfluxMeasurement.InfluxFields.Where(f => f.Name == field).FirstOrDefault();
            if (influxField == null)
                return null;

            return InfluxDBService.GetLastDatapointForField(influxField, identification);
        }

        public void UpdateIdentificationInstances()
        {
            AssetCategory?.InfluxIdentificationTemplates.ForEach(template =>
            {
                var instance = InfluxIdentificationInstances.FirstOrDefault(i => i.InfluxIdentificationTemplate == template);
                if (instance == null)
                {
                    instance = ObjectSpace.CreateObject<InfluxIdentificationInstance>();
                    instance.AssetAdministrationShell = this;
                    instance.InfluxIdentificationTemplate = template;
                    InfluxIdentificationInstances.Add(instance);
                }
                template.InfluxTagKeyPropertyBindings.ForEach(binding =>
                {
                    var tagValue = instance.InfluxTagValues.FirstOrDefault(v => v.InfluxTagKey == binding.InfluxTagKey);
                    if (tagValue == null)
                    {
                        tagValue = ObjectSpace.CreateObject<InfluxTagValue>();
                        tagValue.InfluxTagKey = binding.InfluxTagKey;
                        instance.InfluxTagValues.Add(tagValue);
                        binding.InfluxTagKey.InfluxTagValues.Add(tagValue);
                    }
                    var influxTagValue = new InfluxTagValue(binding, this);
                    tagValue.Value = influxTagValue.Value;
                });
            });

            var unusedInstances = InfluxIdentificationInstances.Where(i => !AssetCategory.InfluxIdentificationTemplates.Contains(i.InfluxIdentificationTemplate)).ToList();
            unusedInstances.ForEach(InfluxIdentificationInstances.Remove);
        }

        public override void OnSaving()
        {
            base.OnSaving();
            UpdateIdentificationInstances();
        }
    }
}
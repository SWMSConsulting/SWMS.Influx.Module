using Aqua.EnumerableExtensions;
using DevExpress.ExpressApp.Blazor.Components;
using DevExpress.ExpressApp.DC;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using SWMS.Influx.Module.Attributes;
using SWMS.Influx.Module.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWMS.Influx.Module.BusinessObjects
{
    [NavigationItem("Influx")]
    [XafDefaultProperty(nameof(Caption))]
    public abstract class AssetAdministrationShell : BaseObject
    {
        public override void OnLoaded()
        {
            UpdateProperties();
            base.OnLoaded();
        }

        [Browsable(false)]
        public abstract string Caption { get; }

        [RuleRequiredField]
        public virtual AssetCategory AssetCategory { get; set; }
    
        [Aggregated]
        public virtual IList<InfluxIdentificationInstance> InfluxIdentificationInstances { get; set; } = new ObservableCollection<InfluxIdentificationInstance>();

        [NotMapped]
        public IList<InfluxMeasurement> InfluxMeasurements => AssetCategory?.InfluxMeasurements;

        [NotMapped]
        public IList<InfluxField> InfluxFields => InfluxMeasurements?.SelectMany(m => m.InfluxFields).ToList();

        public InfluxIdentificationInstance? GetInfluxIdentificationInstanceForMeasurement(string measurement)
        {
            return InfluxIdentificationInstances.Where(i => i.InfluxMeasurement?.Identifier == measurement).FirstOrDefault();
        }

        public InfluxDatapoint? GetLastDatapoint(string measurement, string field)
        {
            var identification = GetInfluxIdentificationInstanceForMeasurement(measurement);
            if (identification == null)
                return null;

            var influxField = identification?.InfluxMeasurement.InfluxFields.Where(f => f.Identifier == field).FirstOrDefault();
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

        public void UpdateProperties()
        {
            var properties = this.GetType().GetProperties();

            foreach (var property in properties)
            {
                var attribute = Attribute.GetCustomAttribute(property, typeof(LastDatapointAttribute)) as LastDatapointAttribute;
                if (attribute != null)
                {
                    var field = InfluxFields?.FirstOrDefault(x => x.Identifier == attribute.FieldIndentifier);
                    var identification = InfluxIdentificationInstances.FirstOrDefault(x => x.InfluxMeasurement == field.InfluxMeasurement);
                    if (field == null || identification == null)
                        property.SetValue(this, null);

                    property.SetValue(this, InfluxDBService.GetLastDatapointForField(field, identification)?.Value);
                }
            }
        }
    }
}
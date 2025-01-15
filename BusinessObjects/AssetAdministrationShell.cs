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

        public virtual AssetCategory AssetCategory { get; set; }
    
        [Aggregated]
        public IList<InfluxIdentificationInstance> InfluxIdentificationInstances
        {
            get
            {
                return AssetCategory?.InfluxIdentificationTemplates.Select(t =>
                {
                    return new InfluxIdentificationInstance
                    {
                        AssetAdministrationShell = this,
                        InfluxIdentificationTemplate = t,
                        InfluxTagValues = t.InfluxTagKeyPropertyBindings
                            .Select(binding => new InfluxTagValue(binding, this))
                            .ToList()
                    };
                }).ToList() ?? new List<InfluxIdentificationInstance>();
            }
        }

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


        public void UpdateProperties()
        {
            var properties = this.GetType().GetProperties();

            foreach (var property in properties)
            {
                var ldpAttribute = Attribute.GetCustomAttribute(property, typeof(LastDatapointAttribute)) as LastDatapointAttribute;
                if (ldpAttribute != null)
                {
                    var fields = InfluxFields;
                    if(ldpAttribute.MeasurementIdentifier != null)
                    {
                        var measurement = InfluxMeasurements?.FirstOrDefault(x => x.Identifier == ldpAttribute.MeasurementIdentifier);
                        fields = measurement?.InfluxFields;
                    }
                    var field = fields?.FirstOrDefault(x => x.Identifier == ldpAttribute.FieldIndentifier);
                    var identification = InfluxIdentificationInstances?.FirstOrDefault(x => x.InfluxMeasurement == field?.InfluxMeasurement);
                    if (field == null || identification == null)
                    {
                        property.SetValue(this, null);
                        Console.WriteLine($"Last Datapoint: Could not find field {ldpAttribute.FieldIndentifier} for measurement {ldpAttribute.MeasurementIdentifier}");
                        continue;
                    }

                    var lastDatapoint = InfluxDBService.GetLastDatapointForField(field, identification);

                    if (property.PropertyType.IsAssignableFrom(typeof(DateTime?)))
                    {
                        property.SetValue(this, lastDatapoint?.Time);
                    }
                    else
                    {
                        property.SetValue(this, lastDatapoint?.Value);
                    }
                }

                var cqaAttribute = Attribute.GetCustomAttribute(property, typeof(CachedQueryAttribute)) as CachedQueryAttribute;
                if (cqaAttribute != null)
                {
                    var fields = InfluxFields;
                    if (cqaAttribute.MeasurementIdentifier != null)
                    {
                        var measurement = InfluxMeasurements?.FirstOrDefault(x => x.Identifier == cqaAttribute.MeasurementIdentifier);
                        fields = measurement?.InfluxFields;
                    }
                    var field = fields?.FirstOrDefault(x => x.Identifier == cqaAttribute.FieldIndentifier);
                    var identification = InfluxIdentificationInstances?.FirstOrDefault(x => x.InfluxMeasurement == field?.InfluxMeasurement);
                    if (field == null || identification == null)
                    {
                        property.SetValue(this, null);
                        Console.WriteLine($"Last Datapoint: Could not find field {cqaAttribute.FieldIndentifier} for measurement {cqaAttribute.MeasurementIdentifier}");
                        continue;
                    }
                    var datapoints = InfluxDBService.GetCachedQueryValues(cqaAttribute.CachedQueryIdentifier, field, identification);
                    Type type = property.PropertyType;
                    if (type.IsAssignableTo(typeof(IList<InfluxDatapoint>)))
                    {
                        property.SetValue(this, datapoints);
                    }
                }
            }
        }
    }
}
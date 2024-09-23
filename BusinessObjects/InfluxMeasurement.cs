using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using SWMS.Influx.Module.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWMS.Influx.Module.BusinessObjects
{
    [DefaultClassOptions]
    [DefaultProperty(nameof(DisplayName))]
    [NavigationItem("Influx")]
    [ImageName("ChartType_Line")]
    public class InfluxMeasurement : BaseObject
    {
        public static string ColumnName = "Measurement";
        public virtual string Identifier { get; set; }
        public virtual string DisplayName { get; set; }
        public virtual IList<InfluxField> InfluxFields { get; set; } = new ObservableCollection<InfluxField>();
        public virtual IList<InfluxTagKey> InfluxTagKeys { get; set; } = new ObservableCollection<InfluxTagKey>();
        public virtual IList<InfluxIdentificationTemplate> InfluxIdentificationTemplates { get; set; } = new ObservableCollection<InfluxIdentificationTemplate>();
        public virtual IList<PredefinedQuerySettings> PredefinedSettings { get; set; } = new ObservableCollection<PredefinedQuerySettings>();

        [NotMapped]
        public bool IsInUse => InfluxIdentificationTemplates.Count > 0;

        public async Task GetFields()
        {
            var results = await InfluxDBService.GetInfluxFieldsForMeasurement(Identifier);

            foreach(var result in results)
            {
                if(InfluxFields.Any(f => f.Identifier == result.Identifier))
                {
                    continue;
                }

                var field = ObjectSpace.CreateObject<InfluxField>();
                field.Identifier = result.Identifier;
                field.DisplayName = result.DisplayName;
                InfluxFields.Add(field);
            }

            ObjectSpace.CommitChanges();
        }

        public async Task GetTagKeys()
        {
            var results = await InfluxDBService.GetInfluxTagKeysForMeasurement(Identifier);

            foreach (var result in results)
            {
                if (InfluxTagKeys.Any(f => f.Identifier == result.Identifier) || result.Identifier.StartsWith("_"))
                {
                    continue;
                }
                var tag = ObjectSpace.CreateObject<InfluxTagKey>();
                tag.Identifier = result.Identifier;
                InfluxTagKeys.Add(tag);
            }

            ObjectSpace.CommitChanges();
        }
    }
}
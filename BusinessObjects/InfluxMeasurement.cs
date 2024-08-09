using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using SWMS.Influx.Module.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWMS.Influx.Module.BusinessObjects
{
    [DefaultClassOptions]
    //[ImageName("BO_Contact")]
    [DefaultProperty(nameof(Name))]
    [NavigationItem("Influx")]
    public class InfluxMeasurement : BaseObject
    {
        public virtual string Name { get; set; }
        public virtual IList<InfluxField> InfluxFields { get; set; } = new ObservableCollection<InfluxField>();
        public virtual IList<InfluxTagKey> InfluxTagKeys { get; set; } = new ObservableCollection<InfluxTagKey>();
        public virtual IList<InfluxIdentificationTemplate> InfluxIdentificationTemplates { get; set; } = new ObservableCollection<InfluxIdentificationTemplate>();

        [NotMapped]
        public IEnumerable<InfluxIdentificationInstance> InfluxIdentificationInstances => InfluxIdentificationTemplates.SelectMany(x => x.InfluxIdentificationInstances);

        [NotMapped]
        public IEnumerable<AssetAdministrationShell> AssetAdministrationShells => InfluxIdentificationInstances.Select(x => x.AssetAdministrationShell);

        public async Task GetFields()
        {
            var results = await InfluxDBService.GetInfluxFieldsForMeasurement(Name);

            foreach(var result in results)
            {
                if(InfluxFields.Any(f => f.Name == result.Name))
                {
                    continue;
                }

                var field = ObjectSpace.CreateObject<InfluxField>();
                field.Name = result.Name;
                InfluxFields.Add(field);
            }

            ObjectSpace.CommitChanges();
        }

        public async Task GetTagKeys()
        {
            var results = await InfluxDBService.GetInfluxTagKeysForMeasurement(Name);

            foreach (var result in results)
            {
                if (InfluxTagKeys.Any(f => f.Name == result.Name) || result.Name.StartsWith("_"))
                {
                    continue;
                }
                var tag = ObjectSpace.CreateObject<InfluxTagKey>();
                tag.Name = result.Name;
                InfluxTagKeys.Add(tag);
            }

            ObjectSpace.CommitChanges();
        }
    }
}
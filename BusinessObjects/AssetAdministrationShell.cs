using DevExpress.Data.Helpers;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using SWMS.Influx.Module.Services;
using System.Collections.ObjectModel;

namespace SWMS.Influx.Module.BusinessObjects
{
    [DefaultClassOptions]
    //[ImageName("BO_Contact")]
    [NavigationItem("Influx")]
    public abstract class AssetAdministrationShell : BaseObject
    {
        private AssetCategory _AssetCategory;
        [RuleRequiredField(DefaultContexts.Save)]
        public virtual AssetCategory AssetCategory
        { 
            get
            {
                return _AssetCategory;
            } 
            set
            {
                _AssetCategory = value;
                CreateInfluxIdentificationInstances();
                ObjectSpace.CommitChanges();
            } 
        }

        [ExpandObjectMembers(ExpandObjectMembers.InListView)]
        public virtual IList<InfluxIdentificationInstance> InfluxIdentificationInstances { get; set; } = new ObservableCollection<InfluxIdentificationInstance>();


        [Action(
            Caption = "Update Identification",
            AutoCommit = true,
            TargetObjectsCriteria = "AssetCategory != null",
            ImageName = "Action_Refresh"
        )]
        public void CreateInfluxIdentificationInstances()
        {
            if (AssetCategory == null)
            {
                return;
            }
            while (InfluxIdentificationInstances.Count > 0)
            {
                InfluxIdentificationInstances.RemoveAt(0);
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
        }

    }
}
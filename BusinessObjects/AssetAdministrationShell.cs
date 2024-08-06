using DevExpress.CodeParser;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using SWMS.Influx.Module.Models;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWMS.Influx.Module.BusinessObjects
{
    [DefaultClassOptions]
    //[ImageName("BO_Contact")]
    [NavigationItem("Influx")]
    public abstract class AssetAdministrationShell : BaseObject
    {
        private AssetCategory _AssetCategory;
        [RuleRequiredField(DefaultContexts.Save)]
        public virtual AssetCategory AssetCategory { get; set; }
    

        [ExpandObjectMembers(ExpandObjectMembers.InListView)]
        public virtual IList<InfluxIdentificationInstance> InfluxIdentificationInstances { get; set; } = new ObservableCollection<InfluxIdentificationInstance>();

        [NotMapped]
        public IList<InfluxField> InfluxFields => AssetCategory.RelevantInfluxFields;

        [NotMapped]
        public IList<InfluxDatapoint> InfluxDatapoints => InfluxFields.SelectMany(f => f.Datapoints).ToList();


        [Action(
            Caption = "Refresh Datapoints",
            AutoCommit = true,
            ImageName = "Action_Refresh"
        )]
        public async void RefreshDatapoints()
        {
            var fluxRange = new FluxRange(AssetCategory.RangeStart, AssetCategory.RangeEnd);
            var aggregateWindow = new FluxAggregateWindow(AssetCategory.AggregateWindow, AssetCategory.AggregateFunction);

            foreach (var field in InfluxFields)
            {
                await field.GetDatapoints(fluxRange, aggregateWindow, this);
            }
        }

        public void CreateInfluxIdentificationInstances()
        {
            if (AssetCategory == null)
            {
                return;
            }

            InfluxIdentificationInstances.ToList().ForEach(ObjectSpace.Delete);

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

            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();
        }

        public override void OnSaving()
        {
            base.OnSaving();

            CreateInfluxIdentificationInstances();
        }

    }
}
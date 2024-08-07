using Aqua.EnumerableExtensions;
using DevExpress.CodeParser;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using SWMS.Influx.Module.Models;
using SWMS.Influx.Module.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace SWMS.Influx.Module.BusinessObjects
{
    [DefaultClassOptions]
    //[ImageName("BO_Contact")]
    [NavigationItem("Influx")]
    public abstract class AssetAdministrationShell : BaseObject, INotifyPropertyChanged
    {
        private AssetCategory _AssetCategory;
        [RuleRequiredField(DefaultContexts.Save)]
        public virtual AssetCategory AssetCategory { 
        get => _AssetCategory;
            set {
                _AssetCategory = value;
                CreateInfluxIdentificationInstances();
                OnPropertyChanged();
            }
        }
    

        [ExpandObjectMembers(ExpandObjectMembers.InListView)]
        public virtual IList<InfluxIdentificationInstance> InfluxIdentificationInstances { get; set; } = new ObservableCollection<InfluxIdentificationInstance>();

        [NotMapped]
        public IList<InfluxField> InfluxFields => AssetCategory?.RelevantInfluxFields;

        [NotMapped]
        public IList<InfluxDatapoint> InfluxDatapoints => InfluxFields.SelectMany(f => f.Datapoints).ToList();

        public InfluxIdentificationInstance? GetInfluxIdentificationInstanceForMeasurement(string measurement)
        {
            return InfluxIdentificationInstances.Where(i => i.InfluxMeasurement.Name == measurement).FirstOrDefault();
        }

        public double? GetLastDatapoint(string measurement, string field)
        {
            var identification = GetInfluxIdentificationInstanceForMeasurement(measurement);
            if (identification == null)
                return null;

            var influxField = identification?.InfluxMeasurement.InfluxFields.Where(f => f.Name == field).FirstOrDefault();
            if (influxField == null)
                return null;

            return InfluxDBService.GetLastDatapointForField(influxField, identification)?.Value;
        }


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
            InfluxIdentificationInstances.ToList().ForEach(ObjectSpace.Delete);

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
            ObjectSpace.CommitChanges();
            ObjectSpace.Refresh();

            OnPropertyChanged(nameof(InfluxIdentificationInstances));
        }
        public override void OnLoaded()
        {
            base.OnLoaded();

            InfluxFields.ForEach(f =>
            {
                f.Datapoints.CollectionChanged += (sender, e) => OnPropertyChanged(nameof(InfluxDatapoints));
            });
        }
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        #region INotifyPropertyChanged members (see http://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged(v=vs.110).aspx)
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

    }
}
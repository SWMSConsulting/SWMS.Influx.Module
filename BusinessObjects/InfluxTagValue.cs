using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using FastMember;
using SWMS.Influx.Module.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace SWMS.Influx.Module.BusinessObjects
{
    [DefaultClassOptions]
    [NavigationItem("Influx")]
    public class InfluxTagValue : BaseObject, INotifyPropertyChanged
    {
        public InfluxTagValue() { }
        public InfluxTagValue(InfluxTagKeyPropertyBinding binding, object assetAdministrationShell)
        {
            string propName = binding.ImplementingClassPropertyName;
            if (assetAdministrationShell.GetType().GetProperty(propName) == null)
            {
                return;
            } 
            var wrapped = ObjectAccessor.Create(assetAdministrationShell);
            var propValue = wrapped[propName];
            InfluxTagKey = binding.InfluxTagKey;
            Value = propValue?.ToString();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [ExpandObjectMembers(ExpandObjectMembers.InListView)]
        public virtual InfluxTagKey InfluxTagKey { get; set; }
        public virtual string Value { get; set; }
        public KeyValuePair<string, object> KeyValuePair => new(InfluxTagKey.Identifier , Value);
        public virtual IList<InfluxIdentificationInstance> InfluxIdentificationInstances { get; set; } = new ObservableCollection<InfluxIdentificationInstance>();

        private BindingList<InfluxDatapoint> _Datapoints = new();
        [NotMapped]
        public BindingList<InfluxDatapoint> Datapoints
        {
            get { return _Datapoints; }
            set
            {
                if (_Datapoints != value)
                {
                    _Datapoints = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public override string ToString()
        {
            return InfluxDBService.KeyValuePairToString(InfluxTagKey.Identifier, Value);
        }


        #region INotifyPropertyChanged members (see http://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged(v=vs.110).aspx)
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

    }
}
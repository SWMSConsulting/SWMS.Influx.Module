using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using FastMember;
using SWMS.Influx.Module.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SWMS.Influx.Module.BusinessObjects
{
    // Register this entity in your DbContext (usually in the BusinessObjects folder of your project) with the "public DbSet<InfluxTagValue> InfluxTagValues { get; set; }" syntax.
    [DefaultClassOptions]
    [NavigationItem("Influx")]
    public class InfluxTagValue : BaseObject, INotifyPropertyChanged
    {
        public InfluxTagValue()
        {

        }
        public InfluxTagValue(InfluxTagKeyPropertyBinding binding, object assetAdministrationShell)
        {
            var wrapped = ObjectAccessor.Create(assetAdministrationShell);
            string propName = binding.ImplementingClassPropertyName;
            var propValue = wrapped[propName];
            InfluxTagKey = binding.InfluxTagKey;
            Value = propValue.ToString();
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [ExpandObjectMembers(ExpandObjectMembers.InListView)]
        public virtual InfluxTagKey InfluxTagKey { get; set; }
        public virtual string Value { get; set; }
        public KeyValuePair<string, object> KeyValuePair => new(InfluxTagKey.Name , Value);
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
            return InfluxDBService.KeyValuePairToString(InfluxTagKey.Name, Value);
        }


        #region INotifyPropertyChanged members (see http://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged(v=vs.110).aspx)
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

    }
}
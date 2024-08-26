using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using SWMS.Influx.Module.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SWMS.Influx.Module.BusinessObjects
{
    [DomainComponent]
    [DefaultClassOptions]
    [NavigationItem("Influx")]
    //[ImageName("BO_Unknown")]
    //[DefaultProperty("SampleProperty")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewOnly, false, NewItemRowPosition.None)]
    // Specify more UI options using a declarative approach (https://documentation.devexpress.com/#eXpressAppFramework/CustomDocument112701).
    public class InfluxDatapoint : IXafEntityObject/*, IObjectSpaceLink*/, INotifyPropertyChanged
    {
        //private IObjectSpace objectSpace;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public InfluxDatapoint()
        {
            Oid = Guid.NewGuid();
        }

        [DevExpress.ExpressApp.Data.Key]
        [Browsable(false)]  // Hide the entity identifier from UI.
        public Guid Oid { get; set; }

        private DateTime _Time;
        [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime Time
        {
            get { return _Time; }
            set
            {
                if (_Time != value)
                {
                    _Time = value;
                    OnPropertyChanged();
                }
            }
        }

        private double _Value;
        public double Value
        {
            get { return _Value; }
            set
            {
                if (_Value != value)
                {
                    _Value = value;
                    OnPropertyChanged();
                }
            }
        }

        private InfluxField _InfluxField;
        [ExpandObjectMembers(ExpandObjectMembers.InListView)]
        public InfluxField InfluxField
        {
            get { return _InfluxField; }
            set
            {
                if (_InfluxField != value)
                {
                    _InfluxField = value;
                    OnPropertyChanged();
                }
            }
        }

        private BindingList<InfluxTagValue> _InfluxTagValues = new();
        public BindingList<InfluxTagValue> InfluxTagValues
        {
            get { return _InfluxTagValues; }
            set
            {
                if (_InfluxTagValues != value)
                {
                    _InfluxTagValues = value;
                    this.OnPropertyChanged();
                }
            }
        }

        public string TagSetString => InfluxDBService.GetTagSetString(InfluxTagValues);

        public string InfluxMetaData
        {
            get
            {
                var measurement = InfluxField.InfluxMeasurement.DisplayName;
                var field = InfluxField.DisplayName;
                return $"{field} ({measurement}): {TagSetString}";
            }
        }

        public string LineProtocol
        {
            get
            {
                // Example lineprotocol: measurement,tag1=val1,tag2=val2 field1="v1",field2=1i 0000000000000000000
                var measurement = InfluxField.InfluxMeasurement.Identifier;
                var tagSetString = TagSetString;
                var fieldSetString = $"{InfluxField.DisplayName}={Value}";
                var timeStamp = ((DateTimeOffset)Time).ToUnixTimeSeconds();
                return $"{measurement},{tagSetString} {fieldSetString} {timeStamp}";
            }
        }

        #region IXafEntityObject members (see https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppIXafEntityObjecttopic.aspx)
        void IXafEntityObject.OnCreated()
        {
            // Place the entity initialization code here.
            // You can initialize reference properties using Object Space methods; e.g.:
            // this.Address = objectSpace.CreateObject<Address>();
        }
        void IXafEntityObject.OnLoaded()
        {
            // Place the code that is executed each time the entity is loaded here.
        }
        void IXafEntityObject.OnSaving()
        {
            // Place the code that is executed each time the entity is saved here.
        }
        #endregion

        #region IObjectSpaceLink members (see https://documentation.devexpress.com/eXpressAppFramework/clsDevExpressExpressAppIObjectSpaceLinktopic.aspx)
        // If you implement this interface, handle the NonPersistentObjectSpace.ObjectGetting event and find or create a copy of the source object in the current Object Space.
        // Use the Object Space to access other entities (see https://documentation.devexpress.com/eXpressAppFramework/CustomDocument113707.aspx).
        //IObjectSpace IObjectSpaceLink.ObjectSpace {
        //    get { return objectSpace; }
        //    set { objectSpace = value; }
        //}
        #endregion

        #region INotifyPropertyChanged members (see http://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged(v=vs.110).aspx)
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
    }
}
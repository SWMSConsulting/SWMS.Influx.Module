using DevExpress.Persistent.BaseImpl.EF;
using System.Collections.ObjectModel;

namespace SWMS.Influx.Module.BusinessObjects
{
    public class InfluxTagTemplate: BaseObject
    {
        public virtual IList<AssetCategory> AssetCategory { get; set; } = new ObservableCollection<AssetCategory>();

        public virtual string Identifier { get; set; }

        public virtual string Description { get; set; }
    }
}

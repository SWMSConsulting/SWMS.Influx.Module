using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
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
    // Register this entity in your DbContext (usually in the BusinessObjects folder of your project) with the "public DbSet<InfluxTag> InfluxTags { get; set; }" syntax.
    [DefaultClassOptions]
    [NavigationItem("Influx")]
    [DefaultProperty(nameof(Id))]
    public class InfluxTagKey : BaseObject
    {
        public InfluxTagKey()
        {

        }

        public string Id => ToString();

        public virtual string Name { get; set; }
        [ExpandObjectMembers(ExpandObjectMembers.InListView)]
        public virtual InfluxMeasurement InfluxMeasurement { get; set; }
        public virtual IList<InfluxTagKeyPropertyBinding> InfluxTagKeyPropertyBindings { get; set; } = new ObservableCollection<InfluxTagKeyPropertyBinding>();
        public virtual IList<InfluxTagValue> InfluxTagValues { get; set; } = new ObservableCollection<InfluxTagValue>();
        public override string ToString()
        {
            if (InfluxMeasurement == null)
            {
                return "";
            }
            return $"{InfluxMeasurement.Name} - {Name}";
        }
    }
}
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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SWMS.Influx.Module.BusinessObjects
{
    // Register this entity in your DbContext (usually in the BusinessObjects folder of your project) with the "public DbSet<InfluxIdentification> InfluxIdentifications { get; set; }" syntax.
    [DefaultClassOptions]
    [NavigationItem("Influx")]
    public class InfluxIdentificationTemplate : BaseObject
    {
        public InfluxIdentificationTemplate()
        {

        }

        public string Name => ToString();

        public virtual AssetCategory AssetCategory { get; set; }

        [ExpandObjectMembers(ExpandObjectMembers.InListView)]
        public virtual InfluxMeasurement InfluxMeasurement { get; set; }

        [DataSourceProperty("InfluxMeasurement.InfluxTagKeys", DataSourcePropertyIsNullMode.SelectAll)]
        public virtual IList<InfluxTagKeyPropertyBinding> InfluxTagKeyPropertyBindings { get; set; } = new ObservableCollection<InfluxTagKeyPropertyBinding>();

        public override string ToString()
        {
            if(InfluxMeasurement == null || AssetCategory == null)
            {
                return "";
            }
            return $"{InfluxMeasurement.Name} - {AssetCategory.Name}";
        }
    }
}
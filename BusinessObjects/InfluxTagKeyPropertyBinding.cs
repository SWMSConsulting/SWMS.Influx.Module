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
    // Register this entity in your DbContext (usually in the BusinessObjects folder of your project) with the "public DbSet<InfluxTagKeyPropertyBinding> InfluxTagKeyPropertyBindings { get; set; }" syntax.
    [DefaultClassOptions]
    [NavigationItem("Influx")]
    public class InfluxTagKeyPropertyBinding : BaseObject
    {
        public InfluxTagKeyPropertyBinding()
        {
            // In the constructor, initialize collection properties, e.g.: 
            // this.AssociatedEntities = new ObservableCollection<AssociatedEntityObject>();
        }        

        [ExpandObjectMembers(ExpandObjectMembers.InListView)]
        public virtual InfluxTagKey InfluxTagKey { get; set; }
        public virtual InfluxIdentificationTemplate InfluxIdentificationTemplate { get; set; }
        public virtual string ImplementingClassPropertyName { get; set; }

    }
}
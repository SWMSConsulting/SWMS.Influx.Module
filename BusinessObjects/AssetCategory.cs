using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using SWMS.Influx.Module.Models;
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
    // Register this entity in your DbContext (usually in the BusinessObjects folder of your project) with the "public DbSet<AssetCategory> AssetCategorys { get; set; }" syntax.
    [DefaultClassOptions]
    [NavigationItem("Influx")]
    //[ImageName("BO_Contact")]
    //[DefaultProperty("Name")]
    //[DefaultListViewOptions(MasterDetailMode.ListViewOnly, false, NewItemRowPosition.None)]
    // Specify more UI options using a declarative approach (https://documentation.devexpress.com/#eXpressAppFramework/CustomDocument112701).
    // You do not need to implement the INotifyPropertyChanged interface - EF Core implements it automatically.
    // (see https://learn.microsoft.com/en-us/ef/core/change-tracking/change-detection#change-tracking-proxies for details).
    public class AssetCategory : BaseObject
    {
        public virtual string Name { get; set; }
        public virtual string RangeStart { get; set; }
        public virtual string RangeEnd { get; set; }
        public virtual string AggregateWindow { get; set; }
        public virtual FluxAggregateFunction AggregateFunction { get; set; }

        public virtual IList<AssetAdministrationShell> AssetAdministrationShells { get; set; } = new ObservableCollection<AssetAdministrationShell>();

        public virtual IList<InfluxTagTemplate> InfluxTagTemplates { get; set; } = new ObservableCollection<InfluxTagTemplate>();

        [Browsable(false)]
        [RuleFromBoolProperty("ValidFluxDuration", DefaultContexts.Save, "Invalid flux duration!", UsedProperties = "AggregateWindow")]
        public virtual bool AggregateWindowIsValid
        {
            get
            {
                return InfluxDBService.IsValidFluxDuration(AggregateWindow);
            }
        }
    }
}
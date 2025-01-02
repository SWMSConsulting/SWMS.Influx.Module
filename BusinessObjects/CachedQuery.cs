using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using SWMS.Influx.Module.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SWMS.Influx.Module.BusinessObjects;

[DefaultProperty(nameof(Name))]
[NavigationItem("Influx")]
public class CachedQuery: BaseObject
{
    [RuleRequiredField]
    public virtual string Identifier { get; set; }

    public virtual string Name { get; set; }

    public virtual IList<InfluxField> InfluxFields { get; set; } = new ObservableCollection<InfluxField>();

    [RuleRequiredField]
    public virtual PredefinedQuerySettings QuerySettings { get; set; }

    public override void OnSaving()
    {
        base.OnSaving();

        InfluxDBService.RefreshCachedQuery(this);
    }
}

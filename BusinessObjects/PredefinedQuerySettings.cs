using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using SWMS.Influx.Module.Models;
using SWMS.Influx.Module.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SWMS.Influx.Module.BusinessObjects;

[DefaultClassOptions]
[NavigationItem("Influx")]
public class PredefinedQuerySettings: BaseObject
{
    public virtual string Name { get; set; }

    public virtual IList<InfluxMeasurement> InfluxMeasurements { get; set; } = new ObservableCollection<InfluxMeasurement>();

    public virtual string RangeQuantifierStart { get; set; }
    public virtual string RangeQuantifierEnd{ get; set; }



    [RuleRequiredField(DefaultContexts.Save)]
    public virtual string AggregateWindow { get; set; }

    public virtual FluxAggregateFunction AggregateFunction { get; set; }


    public DateTime? RangeStart => FluxService.FluxRangeToDateTime(RangeQuantifierStart);
    
    public DateTime? RangeEnd => FluxService.FluxRangeToDateTime(RangeQuantifierEnd);


    [Browsable(false)]
    [RuleFromBoolProperty("IsValidAggregateWindow", DefaultContexts.Save, "Invalid flux Aggregate Window!", UsedProperties = "AggregateWindow")]
    public bool IsValidAggregateWindow
    {
        get
        {
            return FluxService.IsValidFluxDuration(AggregateWindow);
        }
    }

    [Browsable(false)]
    [RuleFromBoolProperty("IsValidRangeQuantifierStart", DefaultContexts.Save, "Invalid RangeQuantifierStart!", UsedProperties = "RangeQuantifierStart")]
    public bool IsValidRangeQuantifierStart
    {
        get
        {
            return string.IsNullOrEmpty(RangeQuantifierStart) || RangeStart != null;
        }
    }

    [Browsable(false)]
    [RuleFromBoolProperty("IsValidRangeQuantifierEnd", DefaultContexts.Save, "Invalid RangeQuantifierEnd!", UsedProperties = "RangeQuantifierEnd")]
    public bool IsValidRangeQuantifierEnd
    {
        get
        {
            return string.IsNullOrEmpty(RangeQuantifierEnd) || RangeEnd != null;
        }
    }

    [Browsable(false)]
    [RuleFromBoolProperty("IsValidRange", DefaultContexts.Save, "Invalid Range!", UsedProperties = "RangeQuantifierStart, RangeQuantifierEnd")]
    public bool IsValidRange
    {
        get
        {
            return RangeStart != null && RangeEnd != null && RangeStart < RangeEnd;
        }
    }
}

namespace SWMS.Influx.Module.Models;

// Documentation: https://docs.influxdata.com/flux/v0/stdlib/universe/aggregatewindow/
public class FluxAggregateWindow
{
    public string Every {  get; set; }
    public FluxAggregateFunction Fn { get; set; }

    public bool CreateEmpty { get; set; } = false;
    public FluxAggregateWindow(string every, FluxAggregateFunction fn)
    {
        Every = every;
        Fn = fn;
    }
}

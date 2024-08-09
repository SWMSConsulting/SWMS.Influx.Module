using DevExpress.CodeParser;
using SWMS.Influx.Module.BusinessObjects;

namespace SWMS.Influx.Module.Models;

public class FluxQueryBuilder()
{
    private string? FluxBucket = null;
    private string? FluxRange = null;

    private string FluxAggregation = "";
    private string FluxFilter = "";
    private string FluxPipe = "";

    private readonly string combinatorAnd = " and ";
    private readonly string combinatorOr = " or ";

    private readonly string fieldFlux = "_field";
    private readonly string measurementFlux = "_measurement";

    public FluxQueryBuilder AddBucket (string bucket)
    {
        FluxBucket = $"from(bucket:\"{bucket}\")";
        return this;
    }

    public FluxQueryBuilder AddRange(FluxRange range)
    {
        return AddRange(range.Start, range.Stop);
    }

    public FluxQueryBuilder AddRange(string start, string end)
    {
        FluxRange = $"|> range(start: {start}, stop: {end})";
        return this;
    }

    public FluxQueryBuilder AddAggregation(FluxAggregateWindow? aggregateWindow = null)
    {
        if (aggregateWindow == null)
        {
            return this;
        }

        string aggregateFunction = aggregateWindow.Fn.ToString().ToLower();
        string createEmpty = aggregateWindow.CreateEmpty ? "true" : "false";

        FluxAggregation = $"|> aggregateWindow(every: {aggregateWindow.Every}, fn: {aggregateFunction}, createEmpty: {createEmpty})";
        return this;
    }

    public FluxQueryBuilder AddMeasurementFilter(InfluxMeasurement measurement)
    {
        AppendFilter(FluxCompare(measurementFlux, measurement.Name));
        return this;
    }

    public FluxQueryBuilder AddMeasurementFilter(IEnumerable<InfluxMeasurement>? measurements)
    {
        if(measurements == null || !measurements.Any())
        {
            return this;
        }
        var measurementFilters = measurements.Select(m => FluxCompare(measurementFlux, m.Name));
        AppendFilter(JoinFilters(measurementFilters, combinatorOr));
        return this;
    }

    public FluxQueryBuilder AddFieldFilter(InfluxField field)
    {
        AppendFilter(FluxCompare(fieldFlux, field.Name));
        return this;
    }

    public FluxQueryBuilder AddFieldFilter(IEnumerable<InfluxField>? fields)
    {
        if(fields == null || !fields.Any())
        {
            return this;
        }
        var fieldFilters = fields.Select(f => FluxCompare(fieldFlux, f.Name));
        AppendFilter(JoinFilters(fieldFilters, combinatorOr));
        return this;
    }

    public FluxQueryBuilder AddTagFilter(IEnumerable<InfluxIdentificationInstance>? identifications)
    {
        if(identifications == null || !identifications.Any())
        {
            return this;
        }
        var tagFilters = identifications.Select(i =>
        {
            return JoinFilters(i.InfluxTagValues.Select(t => {
                return FluxCompare(t.InfluxTagKey.Name, t.Value);
            }), combinatorAnd);
        });

        AppendFilter(JoinFilters(tagFilters, combinatorOr));
        return this;
    }

    public FluxQueryBuilder AddPipe(FluxQueryPipe? pipe)
    {
        FluxPipe = pipe switch
        {
            FluxQueryPipe.Last => "|> last()",
            _ => ""
        };
        return this;
    }
    public FluxQueryBuilder AddPipe(string? pipe)
    {
        FluxPipe = pipe ?? "";
        return this;
    }

    private void AppendFilter(string filter)
    {
        if(!string.IsNullOrEmpty(FluxFilter))
        {
            FluxFilter += "\n";
        }
        FluxFilter += $"|> filter(fn: (r) => {filter})";
    }

    private string FluxCompare(string key, string value)
    {
        return $"r[\"{key}\"] == \"{value}\"";
    }

    private string JoinFilters(IEnumerable<string> filters, string combinator)
    {
        return string.Join($" {combinator} ", filters);
    }

    public string Build()
    {
        if(FluxBucket == null)
        {
            throw new ArgumentNullException("Bucket is required");
        }

        if(FluxRange == null)
        {
            throw new ArgumentNullException("Range is required");
        }

        var queryParameters = new List<string> { 
            FluxBucket, 
            FluxRange, 
            FluxFilter,
            FluxAggregation,
            FluxPipe
        }.Where(s => !string.IsNullOrEmpty(s)).ToList();

        return string.Join("\n", queryParameters);
    }
}

public enum FluxQueryPipe
{
    Last
}
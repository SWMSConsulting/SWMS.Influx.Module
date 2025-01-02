namespace SWMS.Influx.Module.Attributes;


[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class CachedQueryAttribute : Attribute
{
    public string CachedQueryIdentifier { get; }
    public string FieldIndentifier { get; }
    public string? MeasurementIdentifier { get; }

    public CachedQueryAttribute(string cachedQueryIdentifier, string fieldIdentifier, string? measurementIdentifier = null)
    {
        CachedQueryIdentifier = cachedQueryIdentifier;
        FieldIndentifier = fieldIdentifier;
        MeasurementIdentifier = measurementIdentifier;
    }
}


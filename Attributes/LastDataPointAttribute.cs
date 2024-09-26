namespace SWMS.Influx.Module.Attributes;


[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class LastDatapointAttribute : Attribute
{
    public string FieldIndentifier { get; }
    public string? MeasurementIdentifier { get; }

    public LastDatapointAttribute(string fieldIdentifier, string? measurementIdentifier = null)
    {
        FieldIndentifier = fieldIdentifier;
        MeasurementIdentifier = measurementIdentifier;
    }
}

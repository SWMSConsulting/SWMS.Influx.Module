namespace SWMS.Influx.Module.Attributes;


[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class LastDatapointAttribute : Attribute
{
    public string FieldIndentifier { get; }

    public LastDatapointAttribute(string fieldIdentifier)
    {
        FieldIndentifier = fieldIdentifier;
    }
}

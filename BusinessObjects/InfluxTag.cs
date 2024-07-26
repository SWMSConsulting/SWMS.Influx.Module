namespace SWMS.Influx.Module.BusinessObjects
{
    public class InfluxTag
    {
        public InfluxTag(InfluxTagTemplate influxTagTemplate, string value)
        {
            Identifier = influxTagTemplate.Identifier;
            Value = value;
        }

        public string Identifier { get; set; }
        public string Value { get; set; }

        
        public static string ToFluxFilter(IList<InfluxTag> influxTags) {
            if (influxTags == null || influxTags.Count == 0)
                return "";

            var filter = influxTags.Select(x => $"r[\"{x.Identifier}\"] == \"{x.Value}\"").Aggregate((x, y) => $"{x} AND {y}");
            filter = $"|> filter(fn: (r) => {filter})";
            return filter;
        }
    }
}

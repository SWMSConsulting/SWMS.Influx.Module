﻿using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using SWMS.Influx.Module.Services;
using System.ComponentModel;

namespace SWMS.Influx.Module.BusinessObjects
{
    [DomainComponent]
    [DefaultClassOptions]
    [NavigationItem("Influx")]
    [ImageName("ChartType_Line")]
    public class InfluxDatapoint : BaseObject
    {
        public InfluxDatapoint(DateTime time, object value)
        {
            Time = time;
            StringValue = value.ToString();
            try
            {
                Value = Convert.ToDouble(value);
            }
            catch { }
        }

        [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy HH:mm:ss}")]
        public DateTime Time { get; set; }

        public double? Value { get; set; }

        public string StringValue { get; set; }

        [ExpandObjectMembers(ExpandObjectMembers.InListView)]
        public InfluxField InfluxField { get; set; }

        public BindingList<InfluxTagValue> InfluxTagValues { get; set; } = new BindingList<InfluxTagValue>();

        public InfluxMeasurement InfluxMeasurement => InfluxField?.InfluxMeasurement;

        public string TagSetString => InfluxDBService.GetTagSetString(InfluxTagValues);

        public string InfluxMetaData
        {
            get
            {
                var measurement = InfluxField.InfluxMeasurement.DisplayName;
                var field = InfluxField.DisplayName;
                return $"{field} ({measurement}): {TagSetString}";
            }
        }

        public string LineProtocol
        {
            get
            {
                // Example lineprotocol: measurement,tag1=val1,tag2=val2 field1="v1",field2=1i 0000000000000000000
                var measurement = InfluxField.InfluxMeasurement.Identifier;
                var tagSetString = TagSetString;
                var fieldSetString = $"{InfluxField.DisplayName}={Value}";
                var timeStamp = ((DateTimeOffset)Time).ToUnixTimeSeconds();
                return $"{measurement},{tagSetString} {fieldSetString} {timeStamp}";
            }
        }
    }
}
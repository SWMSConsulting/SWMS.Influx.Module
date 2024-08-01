using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.Persistent.Validation;
using FastMember;
using SWMS.Influx.Module.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SWMS.Influx.Module.BusinessObjects
{
    [DefaultClassOptions]
    [NavigationItem("Influx")]
    public class InfluxIdentificationInstance : BaseObject
    {        
        public InfluxIdentificationInstance()
        {

        }

        public virtual AssetAdministrationShell AssetAdministrationShell { get; set; }
        public virtual InfluxIdentificationTemplate InfluxIdentificationTemplate { get; set; }

        public virtual IList<InfluxTagValue> InfluxTagValues { get; set; } = new ObservableCollection<InfluxTagValue>();
        public string TagSetString => InfluxDBService.GetTagSetString(InfluxTagValues);
        public List<KeyValuePair<string, object>> TagKeyValuePairs => InfluxTagValues.Select(x => x.KeyValuePair).ToList();
    }
}
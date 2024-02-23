using System;
using System.Linq;
using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using System.Collections.Generic;
using DevExpress.ExpressApp.Editors;
using DevExpress.ExpressApp.SystemModule;
using SWMS.Influx.Module.BusinessObjects;

namespace SWMS.Influx.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out http://documentation.devexpress.com/#Xaf/clsDevExpressExpressAppViewControllertopic.
    public class InfluxDashboardViewController : ViewController<DashboardView>
    {
        private const string DashboardViewId = "InfluxDashboardView";
        private DashboardViewItem AssetAdministrationShellViewItem;
        private DashboardViewItem InfluxMeasurementViewItem;
        private DashboardViewItem InfluxFieldViewItem;
        private const string CriteriaName = "Test";

        private void FilterDetailListView(ListView masterListView, ListView detailListView)
        {
            detailListView.CollectionSource.Criteria.Clear();
            List<object> searchedObjects = new List<object>();
            foreach (object obj in masterListView.SelectedObjects)
            {
                searchedObjects.Add(detailListView.ObjectSpace.GetKeyValue(obj));
            }
            if (searchedObjects.Count > 0)
            {
                if (masterListView.ObjectTypeInfo.Name == "AssetAdministrationShell")
                {
                    detailListView.CollectionSource.Criteria[CriteriaName] = CriteriaOperator.FromLambda<InfluxMeasurement>(x => searchedObjects.Contains(x.AssetAdministrationShell.ID));
                }
                else if (masterListView.ObjectTypeInfo.Name == "InfluxMeasurement")
                {
                    detailListView.CollectionSource.Criteria[CriteriaName] = CriteriaOperator.FromLambda<InfluxField>(x => searchedObjects.Contains(x.InfluxMeasurement.ID));
                }
            }
        }
        private void SourceItem_ControlCreated(object sender, EventArgs e)
        {
            DashboardViewItem dashboardItem = (DashboardViewItem)sender;
            ListView innerListView = dashboardItem.InnerView as ListView;
            if (innerListView != null)
            {
                innerListView.SelectionChanged -= innerListView_SelectionChanged;
                innerListView.SelectionChanged += innerListView_SelectionChanged;
            }
        }
        private void innerListView_SelectionChanged(object sender, EventArgs e)
        {
            ListView listView = (ListView)sender;
            if (listView.ObjectTypeInfo.Name == "AssetAdministrationShell")
            {
                FilterDetailListView((ListView)AssetAdministrationShellViewItem.InnerView, (ListView)InfluxMeasurementViewItem.InnerView);
            }
            else if (listView.ObjectTypeInfo.Name == "InfluxMeasurement")
            {
                FilterDetailListView((ListView)InfluxMeasurementViewItem.InnerView, (ListView)InfluxFieldViewItem.InnerView);
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            if (View.Id == DashboardViewId)
            {
                AssetAdministrationShellViewItem = (DashboardViewItem)View.FindItem(AssetAdministrationShellViewId);
                InfluxMeasurementViewItem = (DashboardViewItem)View.FindItem(InfluxMeasurementViewId);
                InfluxFieldViewItem = (DashboardViewItem)View.FindItem(InfluxFieldViewId);
                if (AssetAdministrationShellViewItem != null)
                {
                    AssetAdministrationShellViewItem.ControlCreated += SourceItem_ControlCreated;
                }
                if (InfluxMeasurementViewItem != null)
                {
                    InfluxMeasurementViewItem.ControlCreated += SourceItem_ControlCreated;
                }
            }
        }
        protected override void OnDeactivated()
        {
            if (AssetAdministrationShellViewItem != null)
            {
                AssetAdministrationShellViewItem.ControlCreated -= SourceItem_ControlCreated;
                AssetAdministrationShellViewItem = null;
            }
            if (InfluxMeasurementViewItem != null)
            {
                InfluxMeasurementViewItem.ControlCreated -= SourceItem_ControlCreated;
                InfluxMeasurementViewItem = null;
            }
            InfluxFieldViewItem = null;
            base.OnDeactivated();
        }
        public InfluxDashboardViewController()
        {
            AssetAdministrationShellViewId = "AssetAdministrationShellView";
            InfluxMeasurementViewId = "InfluxMeasurementView";
            InfluxFieldViewId = "InfluxFieldView";
        }
        public string AssetAdministrationShellViewId { get; set; }
        public string InfluxMeasurementViewId { get; set; }
        public string InfluxFieldViewId { get; set; }
    }
}
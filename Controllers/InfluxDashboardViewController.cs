﻿using DevExpress.ExpressApp;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp.Editors;
using SWMS.Influx.Module.BusinessObjects;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using System.ComponentModel;
using SWMS.Influx.Module.Models;

namespace SWMS.Influx.Module.Controllers
{
    // For more typical usage scenarios, be sure to check out http://documentation.devexpress.com/#Xaf/clsDevExpressExpressAppViewControllertopic.
    public class InfluxDashboardViewController : ViewController<DashboardView>
    {
        private const string DashboardViewId = "InfluxDashboardView";
        private DashboardViewItem AssetAdministrationShellViewItem;
        private DashboardViewItem InfluxMeasurementViewItem;
        private DashboardViewItem InfluxFieldViewItem;
        private DashboardViewItem InfluxDatapointListViewItem;
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
                //if (masterListView.ObjectTypeInfo.Name == "AssetAdministrationShell")
                //{
                //    detailListView.CollectionSource.Criteria[CriteriaName] = CriteriaOperator.FromLambda<InfluxMeasurement>(x => searchedObjects.Contains(x.AssetAdministrationShell.ID));
                //}
                if (masterListView.ObjectTypeInfo.Name == "InfluxMeasurement")
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
                InfluxDatapointListViewItem = (DashboardViewItem)View.FindItem(InfluxDatapointListViewId);
                if (AssetAdministrationShellViewItem != null)
                {
                    AssetAdministrationShellViewItem.ControlCreated += SourceItem_ControlCreated;
                }
                if (InfluxMeasurementViewItem != null)
                {
                    InfluxMeasurementViewItem.ControlCreated += SourceItem_ControlCreated;
                }
                if (InfluxFieldViewItem != null)
                {
                    InfluxFieldViewItem.ControlCreated += SourceItem_ControlCreated;
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
            if (InfluxFieldViewItem != null)
            {
                InfluxFieldViewItem.ControlCreated -= SourceItem_ControlCreated;
                InfluxFieldViewItem = null;
            }
            InfluxDatapointListViewItem = null;
            base.OnDeactivated();
        }
        public InfluxDashboardViewController()
        {
            AssetAdministrationShellViewId = "AssetAdministrationShellView";
            InfluxMeasurementViewId = "InfluxMeasurementView";
            InfluxFieldViewId = "InfluxFieldView";
            InfluxDatapointListViewId = "InfluxDatapointListView";

            SimpleAction mySimpleAction = new SimpleAction(this, "LoadDatapointsAction", PredefinedCategory.View)
            {
                Caption = "Load Datapoints",
                ImageName = "Action_Refresh"
            };
            mySimpleAction.Execute += LoadDatapointsAction;

        }

        private async void LoadDatapointsAction(object sender, SimpleActionExecuteEventArgs e)
        {
            await LoadDatapoints();
        }
        private async Task LoadDatapoints()
        {
            ListView influxFieldListView = (ListView)InfluxFieldViewItem.InnerView;
            DetailView influxDatapointListView = (DetailView)InfluxDatapointListViewItem.InnerView;

            List<InfluxDatapoint> datapoints = new();

            influxDatapointListView.CurrentObject = new InfluxDatapointList();
            var influxDatapointList = (InfluxDatapointList)influxDatapointListView.CurrentObject;

            var start = "-3h";
            var stop = "now()";
            var fluxRange = new FluxRange(start, stop);
            var aggregateTime = "1m";
            var aggregateFunction = FluxAggregateFunction.Mean;
            var aggregateWindow = new FluxAggregateWindow(aggregateTime, aggregateFunction);
            // TODO: include data loading from InfluxDB service
            throw new NotImplementedException();
            foreach (object obj in influxFieldListView.SelectedObjects)
            {
                InfluxField influxField = (InfluxField)obj;

                //var start = influxField.InfluxMeasurement.AssetAdministrationShell.AssetCategory.RangeStart;
                //var stop = influxField.InfluxMeasurement.AssetAdministrationShell.AssetCategory.RangeEnd;
                //var fluxRange = new FluxRange(start, stop);
                //var aggregateTime = influxField.InfluxMeasurement.AssetAdministrationShell.AssetCategory.AggregateWindow;
                //var aggregateFunction = influxField.InfluxMeasurement.AssetAdministrationShell.AssetCategory.AggregateFunction;

                //await influxField.GetDatapoints(fluxRange, aggregateWindow);
                //datapoints.AddRange(influxField.Datapoints);
            }

            influxDatapointList.Datapoints = new BindingList<InfluxDatapoint>(datapoints);

        }
        public string AssetAdministrationShellViewId { get; set; }
        public string InfluxMeasurementViewId { get; set; }
        public string InfluxFieldViewId { get; set; }
        public string InfluxDatapointListViewId { get; set; }
    }
}
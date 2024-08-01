using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp;
using SWMS.Influx.Module.BusinessObjects;
using DevExpress.Persistent.Base;
using SWMS.Influx.Module.Models;

namespace SWMS.Influx.Module.Controllers
{
    public class InfluxFieldDetailViewController : ViewController
    {
        public InfluxFieldDetailViewController()
        {
            TargetViewType = ViewType.DetailView;
            TargetObjectType = typeof(InfluxField);

            SimpleAction mySimpleAction = new SimpleAction(this, "GetDatapointsAction", PredefinedCategory.View)
            {
                Caption = "Refresh Datapoints",
                //ConfirmationMessage = "Refresh Datapoints for this Field",
                ImageName = "Action_Refresh"
            };
            mySimpleAction.Execute += GetDatapointsAction;
        }
        private async void GetDatapointsAction(object sender, SimpleActionExecuteEventArgs e)
        {
            await GetDatapointsForCurrentObject();
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
        }
        protected override async void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            // Access and customize the target View control.
            await GetDatapointsForCurrentObject();
        }

        async Task GetDatapointsForCurrentObject()
        {
            InfluxField currentObject = View.CurrentObject as InfluxField;

            //var start = currentObject.InfluxMeasurement.AssetAdministrationShell.AssetCategory.RangeStart;
            //var stop = currentObject.InfluxMeasurement.AssetAdministrationShell.AssetCategory.RangeEnd;
            //var fluxRange = new FluxRange(start, stop); 
            //var aggregateTime = currentObject.InfluxMeasurement.AssetAdministrationShell.AssetCategory.AggregateWindow;
            //var aggregateFunction = currentObject.InfluxMeasurement.AssetAdministrationShell.AssetCategory.AggregateFunction;
            var start = "-3h";
            var stop = "now()";
            var fluxRange = new FluxRange(start, stop);
            var aggregateTime = "1m";
            var aggregateFunction = FluxAggregateFunction.Mean;
            var aggregateWindow = new FluxAggregateWindow(aggregateTime, aggregateFunction);

            await currentObject.GetDatapoints(fluxRange, aggregateWindow);
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }

    }
}

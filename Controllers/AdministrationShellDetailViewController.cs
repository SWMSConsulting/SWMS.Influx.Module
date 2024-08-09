using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using SWMS.Influx.Module.BusinessObjects;

namespace SWMS.Influx.Module.Controllers
{
    public class AdministrationShellDetailViewController : ViewController
    {
        public AdministrationShellDetailViewController()
        {
            TargetViewType = ViewType.DetailView;
            TargetObjectType = typeof(AssetAdministrationShell);

            /*
            SimpleAction mySimpleAction = new SimpleAction(this, "GetMeasurementsAction", PredefinedCategory.View)
            {
                Caption = "Refresh Measurements",
                //ConfirmationMessage = "Refresh Measurements for this AssetAdministrationShell",
                ImageName = "Action_Refresh"
            };
            mySimpleAction.Execute += GetMeasurementsAction;
            */
        }
        private async void GetMeasurementsAction(object sender, SimpleActionExecuteEventArgs e)
        {
            AssetAdministrationShell currentObject = View.CurrentObject as AssetAdministrationShell;
            //await currentObject.GetMeasurements();
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
            //var currentObject = View.CurrentObject as AssetAdministrationShell;
            //if (currentObject.InfluxMeasurements.Count > 0)
            //{
            //    return;
            //}
            //await currentObject.GetMeasurements();
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }
    }
}
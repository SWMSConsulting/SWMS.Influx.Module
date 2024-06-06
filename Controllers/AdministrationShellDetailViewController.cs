using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp;
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

            SimpleAction mySimpleAction = new SimpleAction(this, "ReloadData", PredefinedCategory.View)
            {
                Caption = "Refresh Data",
                ImageName = "Action_Refresh"
            };
            mySimpleAction.Execute += GetFieldsAction;
        }
        private async void GetFieldsAction(object sender, SimpleActionExecuteEventArgs e)
        {
            await RefreshObjectData();
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
            await RefreshObjectData();
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }
        private async Task RefreshObjectData()
        {
            var currentObject = View.CurrentObject as AssetAdministrationShell;
            await currentObject.RefreshData();
            View.Refresh();
            Console.WriteLine("Data Refreshed");
        }
    }
}


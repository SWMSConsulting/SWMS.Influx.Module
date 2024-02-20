using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SWMS.Influx.Module.BusinessObjects;
using DevExpress.Persistent.Base;

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
            InfluxField currentObject = View.CurrentObject as InfluxField;
            await currentObject.GetDatapoints();
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
            var currentObject = View.CurrentObject as InfluxField;
            await currentObject.GetDatapoints();
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }

    }
}

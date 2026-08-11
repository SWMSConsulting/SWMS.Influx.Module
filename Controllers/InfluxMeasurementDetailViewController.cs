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
    public class InfluxMeasurementDetailViewController : ViewController
    {
        public InfluxMeasurementDetailViewController()
        {
            TargetViewType = ViewType.DetailView;
            TargetObjectType = typeof(InfluxMeasurement);

            SimpleAction mySimpleAction = new SimpleAction(this, "GetFieldsAction", PredefinedCategory.View)
            {
                Caption = "Refresh Fields",
                //ConfirmationMessage = "Refresh Fields for this IotMeasurement",
                ImageName = "Action_Refresh"
            };
            mySimpleAction.Execute += GetFieldsAction;
        }
        private void GetFieldsAction(object sender, SimpleActionExecuteEventArgs e)
        {
            _ = GetFieldsActionAsync(e);
        }

        private async Task GetFieldsActionAsync(SimpleActionExecuteEventArgs e)
        {
            InfluxMeasurement currentObject = View.CurrentObject as InfluxMeasurement;
            if (currentObject != null)
            {
                await currentObject.GetFields();
            }
        }

        protected override void OnActivated()
        {
            base.OnActivated();
            // Perform various tasks depending on the target View.
        }
        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();
            _ = OnViewControlsCreatedAsync();
        }

        private async Task OnViewControlsCreatedAsync()
        {
            // Access and customize the target View control.
            var currentObject = View.CurrentObject as InfluxMeasurement;
            if (currentObject?.InfluxFields.Count > 0)
            {
                return;
            }
            if (currentObject != null)
            {
                await currentObject.GetFields();
            }
        }
        protected override void OnDeactivated()
        {
            // Unsubscribe from previously subscribed events and release other references and resources.
            base.OnDeactivated();
        }

    }
}

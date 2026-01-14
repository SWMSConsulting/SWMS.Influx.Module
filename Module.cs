using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Updating;

namespace SWMS.Influx.Module;

// For more typical usage scenarios, be sure to check out https://docs.devexpress.com/eXpressAppFramework/DevExpress.ExpressApp.ModuleBase.
public sealed class InfluxModule : ModuleBase {
    public InfluxModule() {
		// 
		// InfluxModule
		// 
		RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.SystemModule.SystemModule));
		//RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Objects.BusinessClassLibraryCustomizationModule)); //https://devexpress.com/kb=T1312589
		RequiredModuleTypes.Add(typeof(DevExpress.ExpressApp.Validation.Blazor.ValidationBlazorModule));
    }
    public override IEnumerable<ModuleUpdater> GetModuleUpdaters(IObjectSpace objectSpace, Version versionFromDB) {
        ModuleUpdater updater = new DatabaseUpdate.Updater(objectSpace, versionFromDB);
        return new ModuleUpdater[] { updater };
    }
    public override void Setup(XafApplication application) {
        base.Setup(application);
        // Manage various aspects of the application UI and behavior at the module level.
    }
}

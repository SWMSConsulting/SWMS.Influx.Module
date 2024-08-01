using DevExpress.ExpressApp.EFCore.Updating;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using DevExpress.Persistent.BaseImpl.EF.PermissionPolicy;
using DevExpress.Persistent.BaseImpl.EF;
using DevExpress.ExpressApp.Design;
using DevExpress.ExpressApp.EFCore.DesignTime;

namespace SWMS.Influx.Module.BusinessObjects;

// This code allows our Model Editor to get relevant EF Core metadata at design time.
// For details, please refer to https://supportcenter.devexpress.com/ticket/details/t933891.
public class InfluxContextInitializer : DbContextTypesInfoInitializerBase {
	protected override DbContext CreateDbContext() {
		var optionsBuilder = new DbContextOptionsBuilder<InfluxEFCoreDbContext>()
            .UseSqlServer(";")
            .UseChangeTrackingProxies()
            .UseObjectSpaceLinkProxies();
        return new InfluxEFCoreDbContext(optionsBuilder.Options);
	}
}
//This factory creates DbContext for design-time services. For example, it is required for database migration.
public class InfluxDesignTimeDbContextFactory : IDesignTimeDbContextFactory<InfluxEFCoreDbContext> {
	public InfluxEFCoreDbContext CreateDbContext(string[] args) {
		throw new InvalidOperationException("Make sure that the database connection string and connection provider are correct. After that, uncomment the code below and remove this exception.");
		//var optionsBuilder = new DbContextOptionsBuilder<InfluxEFCoreDbContext>();
		//optionsBuilder.UseSqlServer("Integrated Security=SSPI;Data Source=(localdb)\\mssqllocaldb;Initial Catalog=SWMS.Influx");
        //optionsBuilder.UseChangeTrackingProxies();
        //optionsBuilder.UseObjectSpaceLinkProxies();
		//return new InfluxEFCoreDbContext(optionsBuilder.Options);
	}
}
[TypesInfoInitializer(typeof(InfluxContextInitializer))]
public class InfluxEFCoreDbContext : DbContext {
	public InfluxEFCoreDbContext(DbContextOptions<InfluxEFCoreDbContext> options) : base(options) {
	}

    protected InfluxEFCoreDbContext(DbContextOptions options)
        : base(options) {
    }

	//public DbSet<ModuleInfo> ModulesInfo { get; set; }
	public DbSet<AssetAdministrationShell> AssetAdministrationShell { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
        modelBuilder.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);

        modelBuilder.Entity<AssetAdministrationShell>()
            .HasMany(r => r.InfluxIdentificationInstances)
            .WithOne(x => x.AssetAdministrationShell)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InfluxMeasurement>()
            .HasMany(r => r.InfluxFields)
            .WithOne(x => x.InfluxMeasurement)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InfluxMeasurement>()
            .HasMany(r => r.InfluxTagKeys)
            .WithOne(x => x.InfluxMeasurement)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<InfluxTagKey>()
            .HasMany(r => r.InfluxTagKeyPropertyBindings)
            .WithOne(x => x.InfluxTagKey)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<AssetCategory>()
            .Property(d => d.AggregateFunction)
            .HasConversion<string>();
    }
}

using DevExpress.CodeParser;
using Microsoft.EntityFrameworkCore;

namespace SWMS.Influx.Module.BusinessObjects;

public static class InfluxDbContextExtention
{
    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssetAdministrationShell>()
            .UseTpcMappingStrategy();

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
            .HasMany(x => x.InfluxIdentificationTemplates)
            .WithOne(x => x.AssetCategory)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<InfluxIdentificationTemplate>()
            .HasMany(x => x.InfluxTagKeyPropertyBindings)
            .WithOne(x => x.InfluxIdentificationTemplate)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder
            .Entity<PredefinedQuerySettings>()
            .Property(d => d.AggregateFunction)
            .HasConversion<string>();
    }
}

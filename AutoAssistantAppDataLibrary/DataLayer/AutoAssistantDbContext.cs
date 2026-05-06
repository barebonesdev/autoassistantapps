using AutoAssistantAppDataLibrary.DataLayer.DataItems;
using AutoAssistantAppDataLibrary.DataLayer.DataItems.BaseItems;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoAssistantAppDataLibrary.DataLayer
{
    /// <summary>
    /// Entity Framework Core DbContext for Power Planner database.
    /// Replaces the legacy sqlite-net-pcl implementation.
    /// </summary>
    public class AutoAssistantDbContext : DbContext
    {
        private readonly string _databasePath;
        public AccountDataItem Account { get; private set; }

        public AutoAssistantDbContext(string databasePath, AccountDataItem account)
        {
            _databasePath = databasePath;
            Account = account;
        }

        // DbSets for all entity types
        public DbSet<DataItemFuelEntry> FuelEntries { get; set; }
        public DbSet<DataItemMaintenanceRecordEntry> MaintenanceRecordEntries { get; set; }
        public DbSet<DataItemMaintenanceScheduleItem> MaintenanceScheduleItems { get; set; }
        public DbSet<DataItemVehicle> Vehicles { get; set; }
        public DbSet<AccountDataStore.DataInfo> DataInfos { get; set; }

        /// <summary>
        /// Value converter for DateTime properties.
        /// The legacy sqlite-net-pcl library stored DateTime values as ticks (long).
        /// EF Core SQLite expects text format by default. This converter ensures
        /// we read/write ticks to remain compatible with legacy databases.
        /// </summary>
        private static readonly ValueConverter<DateTime, long> DateTimeToTicksConverter =
            new ValueConverter<DateTime, long>(
                v => v.Ticks,
                v => new DateTime(v, DateTimeKind.Utc));

        /// <summary>
        /// Value converter for nullable TimeSpan properties.
        /// The legacy sqlite-net-pcl library stored TimeSpan values as ticks (long).
        /// </summary>
        private static readonly ValueConverter<TimeSpan?, long?> NullableTimeSpanToTicksConverter =
            new ValueConverter<TimeSpan?, long?>(
                v => v.HasValue ? v.Value.Ticks : null,
                v => v.HasValue ? new TimeSpan(v.Value) : null);

        /// <summary>
        /// Value converter for Guid properties.
        /// The legacy sqlite-net-pcl library stored Guid values as text strings.
        /// EF Core SQLite stores Guids as BLOB by default, which causes equality
        /// comparisons to fail against text-formatted GUIDs in legacy databases.
        /// </summary>
        private static readonly ValueConverter<Guid, string> GuidToStringConverter =
            new ValueConverter<Guid, string>(
                v => v.ToString(),
                v => Guid.Parse(v));

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite($"Data Source={_databasePath}");

                // Performance optimizations
                optionsBuilder.EnableSensitiveDataLogging(false);
                optionsBuilder.EnableDetailedErrors(false);

                // Disable the thread-safety check. The app uses its own read/write lock
                // (Locks.LockDataForReadAsync/LockDataForWriteAsync) to coordinate access,
                // allowing concurrent reads but exclusive writes - the same pattern used
                // with the previous sqlite-net-pcl implementation. SQLite in WAL mode
                // supports concurrent readers natively.
                optionsBuilder.EnableThreadSafetyChecks(false);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure DataItemVehicle
            modelBuilder.Entity<DataItemVehicle>(entity =>
            {
                entity.ToTable("DataItemVehicle");
                entity.HasKey(e => e.Identifier);
                entity.HasIndex(e => e.Identifier).HasDatabaseName("Index_DataItemVehicle_Identifier");

                entity.Property(e => e.Identifier).HasColumnName("Identifier");
                entity.Property(e => e.DateCreated).HasColumnName("DateCreated");
                entity.Property(e => e.Updated).HasColumnName("Updated");
                entity.Property(e => e.Nickname).HasColumnName("Nickname");
                entity.Property(e => e.Make).HasColumnName("Make");
                entity.Property(e => e.Model).HasColumnName("Model");
                entity.Property(e => e.Year).HasColumnName("Year");
                entity.Property(e => e.LicensePlate).HasColumnName("LicensePlate");
                entity.Property(e => e.VIN).HasColumnName("VIN");
                entity.Property(e => e.Notes).HasColumnName("Notes");
                entity.Property(e => e.DatePurchased).HasColumnName("DatePurchased");
                entity.Property(e => e.InitialMileage).HasColumnName("InitialMileage");
                entity.Property(e => e.PurchasedFrom).HasColumnName("PurchasedFrom");
                entity.Property(e => e.AmountPurchasedFor).HasColumnName("AmountPurchasedFor");
                entity.Property(e => e.FuelAddingOption_ShowTotalCost).HasColumnName("FuelAddingOption_ShowTotalCost");
                entity.Property(e => e.FuelAddingOption_ShowMileage).HasColumnName("FuelAddingOption_ShowMileage");
                entity.Property(e => e.FuelAddingOption_FuelType).HasColumnName("FuelAddingOption_FuelType");

                // Ignore navigation properties that aren't stored in database
                entity.Ignore(e => e.Account);
            });

            // Configure DataItemFuelEntry
            modelBuilder.Entity<DataItemFuelEntry>(entity =>
            {
                entity.ToTable("DataItemFuelEntry");
                entity.HasKey(e => e.Identifier);
                entity.HasIndex(e => e.Identifier).HasDatabaseName("Index_DataItemFuelEntry_Identifier");
                entity.HasIndex(e => e.VehicleIdentifier).HasDatabaseName("Index_DataItemFuelEntry_VehicleIdentifier");

                entity.Property(e => e.Identifier).HasColumnName("Identifier");
                entity.Property(e => e.VehicleIdentifier).HasColumnName("VehicleIdentifier");
                entity.Property(e => e.DateCreated).HasColumnName("DateCreated");
                entity.Property(e => e.Updated).HasColumnName("Updated");
                entity.Property(e => e.Date).HasColumnName("Date");
                entity.Property(e => e.CostPerGallon).HasColumnName("CostPerGallon");
                entity.Property(e => e.Gallons).HasColumnName("Gallons");
                entity.Property(e => e.StoreName).HasColumnName("StoreName");
                entity.Property(e => e.Location).HasColumnName("Location");
                entity.Property(e => e.FuelType).HasColumnName("FuelType");
                entity.Property(e => e.Mileage).HasColumnName("Mileage");
                entity.Property(e => e.PartialFill).HasColumnName("PartialFill");
                entity.Property(e => e.SkippedEnteringPreviousFillup).HasColumnName("SkippedEnteringPreviousFillup");
                entity.Property(e => e.Notes).HasColumnName("Notes");

                entity.Ignore(e => e.Account);
            });

            // Configure DataItemMaintenanceRecordEntry
            modelBuilder.Entity<DataItemMaintenanceRecordEntry>(entity =>
            {
                entity.ToTable("DataItemMaintenanceRecordEntry");
                entity.HasKey(e => e.Identifier);
                entity.HasIndex(e => e.Identifier).HasDatabaseName("Index_DataItemMaintenanceRecordEntry_Identifier");
                entity.HasIndex(e => e.VehicleIdentifier).HasDatabaseName("Index_DataItemMaintenanceRecordEntry_VehicleIdentifier");

                entity.Property(e => e.Identifier).HasColumnName("Identifier");
                entity.Property(e => e.VehicleIdentifier).HasColumnName("VehicleIdentifier");
                entity.Property(e => e.DateCreated).HasColumnName("DateCreated");
                entity.Property(e => e.Updated).HasColumnName("Updated");
                entity.Property(e => e.Details).HasColumnName("Details");
                entity.Property(e => e.Title).HasColumnName("Title");
                entity.Property(e => e.DoneBy).HasColumnName("DoneBy");
                entity.Property(e => e.Mileage).HasColumnName("Mileage");
                entity.Property(e => e.Date).HasColumnName("Date");
                entity.Property(e => e.Cost).HasColumnName("Cost");
                entity.Property(e => e.RawServicesPerformed).HasColumnName("RawServices");

                entity.Ignore(e => e.ServicesPerformed);
                entity.Ignore(e => e.Account);
            });

            // Configure DataItemMaintenanceScheduleItem
            modelBuilder.Entity<DataItemMaintenanceScheduleItem>(entity =>
            {
                entity.ToTable("DataItemMaintenanceScheduleItem");
                entity.HasKey(e => e.Identifier);
                entity.HasIndex(e => e.Identifier).HasDatabaseName("Index_DataItemMaintenanceScheduleItem_Identifier");
                entity.HasIndex(e => e.VehicleIdentifier).HasDatabaseName("Index_DataItemMaintenanceScheduleItem_VehicleIdentifier");

                entity.Property(e => e.Identifier).HasColumnName("Identifier");
                entity.Property(e => e.VehicleIdentifier).HasColumnName("VehicleIdentifier");
                entity.Property(e => e.DateCreated).HasColumnName("DateCreated");
                entity.Property(e => e.Updated).HasColumnName("Updated");
                entity.Property(e => e.Details).HasColumnName("Details");
                entity.Property(e => e.Title).HasColumnName("Title");
                entity.Property(e => e.MileageInterval).HasColumnName("MileageInterval");
                entity.Property(e => e.MonthInterval).HasColumnName("MonthInterval");
                entity.Property(e => e.EstimatedCost).HasColumnName("EstimatedCost");

                entity.Ignore(e => e.Account);
            });

            // Configure DataInfo
            modelBuilder.Entity<AccountDataStore.DataInfo>(entity =>
            {
                entity.ToTable("DataInfo");
                entity.HasKey(e => e.Key);
            });

            // Apply ticks-based DateTime converter globally for all DateTime properties
            // to maintain compatibility with legacy sqlite-net-pcl databases
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime))
                    {
                        property.SetValueConverter(DateTimeToTicksConverter);
                    }
                    else if (property.ClrType == typeof(Guid))
                    {
                        property.SetValueConverter(GuidToStringConverter);
                    }
                }
            }
        }

        /// <summary>
        /// Helper method to apply account to entities after they're loaded
        /// </summary>
        public T ApplyAccount<T>(T entity) where T : BaseDataItem
        {
            if (entity != null)
            {
                entity.Account = Account;
            }
            return entity;
        }
    }
}

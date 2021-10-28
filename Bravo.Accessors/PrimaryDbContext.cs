using Bravo.Accessors.EntityFramework;
using Bravo.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Validation;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Bravo.Accessors
{
    class PrimaryDBContext : DbContext
    {
        private static readonly ILog Logger = Logging.GetLogger(typeof(PrimaryDBContext));
        private static readonly ILog SqlLogger = Logging.GetLogger(typeof(PrimaryDBContext), "Sql");
        public PrimaryDBContext()
        {
            EntityConnectionStringBuilder connectionString = new EntityConnectionStringBuilder
            {
                Provider = "System.Data.SqlClient",
                ProviderConnectionString = ConfigurationHelper.ConnectionStrings.BravoPrimaryConnectionString
            };

            Database.Connection.ConnectionString = connectionString.ProviderConnectionString;

            Configuration.ProxyCreationEnabled = false;
            Configuration.LazyLoadingEnabled = false;

            System.Data.Entity.Database.SetInitializer<PrimaryDBContext>(null);

            //Uncomment to get db statements
            //Database.Log = Console.Write;
        }

        #region Entities
        public virtual DbSet<ModelStressPeriodCustomStartDate> ModelStressPeriodCustomStartDates { get; set; }

        public virtual DbSet<ModelScenario> ModelScenarios { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        public virtual DbSet<Model> Models { get; set; }
        public virtual DbSet<Run> Runs { get; set; }
        public virtual DbSet<RunBucket> RunBuckets { get; set; }
        public virtual DbSet<RunBucketRun> RunBucketRuns { get; set; }
        public virtual DbSet<Scenario> Scenarios { get; set; }
        public virtual DbSet<RunGeography> RunGeographies { get; set; }
        #endregion

        #region Model Init
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Image>()
                .HasMany(e => e.Models)
                .WithRequired(e => e.Image)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Image>()
                .HasMany(e => e.Runs)
                .WithRequired(e => e.Image)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Model>()
                .HasMany(e => e.ModelStressPeriodCustomStartDates)
                .WithRequired(e => e.Model)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Model>()
                .HasMany(e => e.ModelScenarios)
                .WithRequired(e => e.Model)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Model>()
                .HasMany(e => e.Runs)
                .WithRequired(e => e.Model)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Run>()
                .HasMany(e => e.Geographies)
                .WithRequired(e => e.Run)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<RunBucket>()
                .HasMany(e => e.RunBucketRuns)
                .WithRequired(e => e.RunBucket)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<Scenario>()
                .HasMany(e => e.ModelScenarios)
                .WithRequired(e => e.Scenario)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Scenario>()
                .HasMany(e => e.Runs)
                .WithRequired(e => e.Scenario)
                .WillCascadeOnDelete(false);

            // Turn this on to write Entity Framework queries to the console for debugging
            Database.Log = (query) => SqlLogger.Debug(query);
        }
        #endregion

        #region Overrides

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                // Retrieve the error messages as a list of strings.
                var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);

                // Join the list to a single string.
                var fullErrorMessage = string.Join("; ", errorMessages);

                // Combine the original exception message with the new one.
                var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                // Throw a new DbEntityValidationException with the improved exception message.
                throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
            }
        }

        #endregion
    }
}

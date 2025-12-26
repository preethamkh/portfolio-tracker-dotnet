using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Core.Entities;

namespace PortfolioTracker.Infrastructure.Data
{
    /// <summary>
    /// The application's database context for managing data access to the Portfolio Tracker application.
    /// This class manages the connection to the PostgreSQL database and provides methods for querying and saving data / access to all entities.
    /// <remarks>
    /// DbContext is the primary class for interacting with the database using Entity Framework Core.
    /// Represents a session with the database and can be used to query and save instances of our entities.
    /// </remarks>
    /// </summary>

    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Constructor that accepts DbContextOptions.
        /// This allows us to configure the database connection via dependency injection.
        /// </summary>
        /// <param name="options">Configuration options for the DbContext</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            
        }

        // DbSets for each entity in the application would go here.
        // DbSet properties - Each represents a table in the database.
        // These are used to query and save instances of the entities.

        /// <summary>
        /// Users table - stores user information.
        /// </summary>
        public DbSet<User> Users { get; set; }

        /// <summary>
        /// Portfolios table - Stores investment portfolios
        /// </summary>
        public DbSet<Portfolio> Portfolios { get; set; }

        /// <summary>
        /// Holdings table - Stores individual stock/ETF positions.
        /// </summary>
        public DbSet<Holding> Holdings { get; set; }

        /// <summary>
        /// Securities table - Master data for stocks/ETFs.
        /// </summary>
        public DbSet<Security> Securities { get; set; }

        /// <summary>
        /// Transactions table - Buy/sell transaction history.
        /// </summary>
        public DbSet<Transaction> Transactions { get; set; }

        /// <summary>
        /// PriceHistory table - Historical price data cache.
        /// </summary>
        public DbSet<PriceHistory> PriceHistory { get; set; }

        /// <summary>
        /// Dividends table - Dividend payment records.
        /// </summary>
        public DbSet<Dividend> Dividends { get; set; }

        /// <summary>
        /// PortfolioSnapshots table - Daily portfolio value snapshots.
        /// </summary>
        public DbSet<PortfolioSnapshot> PortfolioSnapshots { get; set; }


        /// <summary>
        /// OnModelCreating is called when the model is being created.
        /// This is where we configure entity relationships, indexes, constraints, etc.
        /// This is the "Fluent API" approach (alternative to data annotations).
        /// </summary>
        /// <param name="modelBuilder">The builder used to construct the model</param>
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // Apply all configurations from separate configuration classes
            // This keeps the DbContext clean and configurations organized
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Configure table names (optional - EF uses entity names by default)
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Portfolio>().ToTable("portfolios");
            modelBuilder.Entity<Holding>().ToTable("holdings");
            modelBuilder.Entity<Security>().ToTable("securities");
            modelBuilder.Entity<Transaction>().ToTable("transactions");
            modelBuilder.Entity<PriceHistory>().ToTable("priceHistory");
            modelBuilder.Entity<Dividend>().ToTable("dividends");
            modelBuilder.Entity<PortfolioSnapshot>().ToTable("portfolio_snapshots");

            // Configure indexes for better query performance
            ConfigureIndexes(modelBuilder);

            // Configure unique constraints
            ConfigureUniqueConstraints(modelBuilder);

            // Configure relationships between entities
            ConfigureRelationships(modelBuilder);

            // Configure default values and computed columns
            ConfigureDefaults(modelBuilder);
        }

        /// <summary>
        /// Configures database indexes to improve query performance.
        /// Indexes speed up SELECT queries but slow down INSERT/UPDATE.
        /// Add indexes on columns frequently used in WHERE clauses.
        /// </summary>
        private void ConfigureDefaults(ModelBuilder modelBuilder)
        {
            // User indexes
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasDatabaseName("ix_users_email");

            // Portfolio indexes
            modelBuilder.Entity<Portfolio>()
                .HasIndex(p => p.UserId)
                .HasDatabaseName("ix_portfolios_user_id");

            modelBuilder.Entity<Portfolio>()
                .HasIndex(p => new { p.UserId, p.IsDefault })
                //.IsUnique()
                .HasDatabaseName("ix_portfolios_user_default");

            // Holding indexes
            modelBuilder.Entity<Holding>()
                .HasIndex(h => h.PortfolioId)
                .HasDatabaseName("ix_holdings_portfolio_id");

            modelBuilder.Entity<Holding>()
                .HasIndex(h => h.SecurityId)
                .HasDatabaseName("ix_holdings_security_id");

            // PortfolioSnapshot indexes
            modelBuilder.Entity<PortfolioSnapshot>()
                .HasIndex(ps => ps.PortfolioId)
                .HasDatabaseName("ix_snapshots_portfolio_id");

            modelBuilder.Entity<PortfolioSnapshot>()
                .HasIndex(ps => new { ps.PortfolioId, ps.SnapshotDate })
                .HasDatabaseName("ix_snapshots_portfolio_date");
        }

        /// <summary>
        /// Configures unique constraints to enforce data integrity.
        /// Prevents duplicate records where they shouldn't exist.
        /// </summary
        private void ConfigureUniqueConstraints(ModelBuilder modelBuilder)
        {
            // Only one holding per security per portfolio
            // todo: Q(curveball thinking): what if one had multiple HIN numbers - overkill for now / this project! (- keep it simple)
            modelBuilder.Entity<Holding>()
                .HasIndex(h => new { h.PortfolioId, h.SecurityId })
                .IsUnique()
                .HasDatabaseName("uq_holdings_portfolio_security");

            // One only snapshot per portfolio per date
            modelBuilder.Entity<PortfolioSnapshot>()
                .HasIndex(ps => new {ps.PortfolioId, ps.SnapshotDate})
                .IsUnique()
                .HasDatabaseName("uq_snapshots_portfolio_date");

            // Only one price record per security per date
            // DECISION: Not considering intraday prices for now - changes the data model significantly
            // todo: flesh out remaining entities
        }

        private void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            // User -> Portfolio (one-to-many)
            modelBuilder.Entity<User>()
                .HasMany(u => u.Portfolios)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                // Delete portfolios when user is deleted
                .OnDelete(DeleteBehavior.Cascade);

            // Portfolio -> Holdings (One-to-Many)
            modelBuilder.Entity<Portfolio>()
                .HasMany(p => p.Holdings)
                .WithOne(h => h.Portfolio)
                .HasForeignKey(h => h.PortfolioId)
                // Delete holdings when portfolio is deleted
                .OnDelete(DeleteBehavior.Cascade);

            // Portfolio -> Snapshots (One-to-Many)
            modelBuilder.Entity<Portfolio>()
                .HasMany(p => p.Snapshots)
                .WithOne(s => s.Portfolio)
                .HasForeignKey(s => s.PortfolioId)
                .OnDelete(DeleteBehavior.Cascade);

            //// Security -> Holdings (One-to-Many)
            //modelBuilder.Entity<Security>()
            //    .HasMany(s => s.Holdings)
            //    .WithOne(h => h.Security)
            //    .HasForeignKey(h => h.SecurityId)
            //    .OnDelete(DeleteBehavior.Restrict); // Don't allow deleting security if holdings exist

            //// Security -> PriceHistory (One-to-Many)
            //modelBuilder.Entity<Security>()
            //    .HasMany(s => s.PriceHistory)
            //    .WithOne(ph => ph.Security)
            //    .HasForeignKey(ph => ph.SecurityId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //// Holding -> Transactions (One-to-Many)
            //modelBuilder.Entity<Holding>()
            //    .HasMany(h => h.Transactions)
            //    .WithOne(t => t.Holding)
            //    .HasForeignKey(t => t.HoldingId)
            //    .OnDelete(DeleteBehavior.Cascade);

            //// Holding -> Dividends (One-to-Many)
            //modelBuilder.Entity<Holding>()
            //    .HasMany(h => h.Dividends)
            //    .WithOne(d => d.Holding)
            //    .HasForeignKey(d => d.HoldingId)
            //    .OnDelete(DeleteBehavior.Cascade);
        }

        private void ConfigureIndexes(ModelBuilder modelBuilder)
        {
            // Default timestamps to current UTC time
            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            modelBuilder.Entity<User>()
                .Property(u => u.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // todo: Similar for other entities...
            // (EF will use the default values from entity properties)
        }

        /// <summary>
        /// SaveChangesAsync override to automatically update UpdatedAt timestamps.
        /// This is called every time data is saved to the database.
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            // Get all entities being tracked that have an UpdatedAt property
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                // Automatically set UpdatedAt when entity is modified
                if (entry.State == EntityState.Modified && entry.Entity.GetType().GetProperty("UpdatedAt") != null)
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
                }

                // Ensure CreatedAt is set for new entities
                if (entry.State == EntityState.Added && entry.Entity.GetType().GetProperty("CreatedAt") != null)
                {
                    var createdAtValue = entry.Property("CreatedAt").CurrentValue;
                    if (createdAtValue == null ||
                        (createdAtValue is DateTime dt && dt == default))
                    {
                        entry.Property("CreatedAt").CurrentValue = DateTime.UtcNow;
                    }
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}

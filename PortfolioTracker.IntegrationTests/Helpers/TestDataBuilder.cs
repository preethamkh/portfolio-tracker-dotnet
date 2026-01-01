using PortfolioTracker.Core.Entities;
using PortfolioTracker.Infrastructure.Data;

namespace PortfolioTracker.IntegrationTests.Helpers
{
    /// <summary>
    /// Helper class for creating test data entities.
    /// Provides methods to quickly create users, portfolios, etc. for tests.
    /// </summary>
    /// <remarks>
    /// Why this:
    /// - DRY Principle
    /// - Consistency - all tests use the same data structure
    /// - Maintainability - change entity structure once, affects all tests
    /// - Readability - tests focus on behaviour, not setup
    ///
    /// Pattern: Builder pattern + factory pattern
    /// - static methods act as factories
    /// - optional parameters allow customization
    /// - provides sensible defaults for all fields
    ///
    /// Usage in tests:
    /// var user = await TestDataBuilder.CreateUser(context);
    /// var portfolio = await TestDataBuilder.CreatePortfolio(context, userId);
    /// </remarks>
    public static class TestDataBuilder
    {
        #region User Creation

        /// <summary>
        /// Creates a test user with default values.
        /// User is saved to database immediately.
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="email">optional custom email (generates unique if not provided)</param>
        /// <param name="fullName">optional custom name</param>
        /// <param name="password">optional custom password hash</param>
        /// <returns>created user with generated ID</returns>
        /// <remarks>
        /// Why email has GUID:
        /// - prevents "email already exists" conflicts
        /// - tests can run in parallel without conflicts
        /// </remarks>
        // Entity builders Task<User> are the standard approach for integration tests (or tests)
        public static async Task<User> CreateUser(ApplicationDbContext context, string? email = null, string? fullName = null, string? password = null)
        {
            var user = new User
            {
                // Generate unique email if not provided
                // :N format = no hyphens in guid
                Email = email ?? $"test{Guid.NewGuid():N}@test.com",
                FullName = fullName ?? "Test User",
                PasswordHash = password ?? "hashedPassword123",
            };

            // Add to database
            await context.Users.AddAsync(user);
            // assign the ID (db generated)
            await context.SaveChangesAsync();

            return user;
        }

        /// <summary>
        /// Create multiple tests users at once.
        /// Useful for tests that need multiple users (authorization tests, etc.)
        /// </summary>
        /// <param name="context"></param>
        /// <param name="count"></param>
        /// <returns>List of created users</returns>
        public static async Task<List<User>> CreateUsers(ApplicationDbContext context, int count)
        {
            var users = new List<User>();

            // todo: consider parallel creation if performance is an issue
            for (int i = 0; i < count; i++)
            {
                var user = await CreateUser(context);
                users.Add(user);
            }

            return users;

            // Alternative parallel approach (commented out for clarity)
            //var tasks = new List<Task<User>>();

            //for (int i = 0; i < count; i++)
            //{
            //    // Each task creates and saves a user
            //    tasks.Add(CreateUser(context));
            //}

            //// Wait for all users to be created in parallel
            //var users = await Task.WhenAll(tasks);
            //return users.ToList();
        }

        #endregion

        #region Portfolio Creation

        /// <summary>
        /// Creates a test portfolio for a given user.
        /// </summary>
        /// <remarks>
        /// Why name has GUID:
        /// - Same user might have multiple portfolios in tests
        /// - Prevents "duplicate name" business rule violations
        /// - Tests remain independent
        /// </remarks>
        public static async Task<Portfolio> CreatePortfolio(ApplicationDbContext context, Guid userId, string? name = null,
            string? description = null, string currency = "AUD", bool isDefault = false)
        {
            var portfolio = new Portfolio
            {
                UserId = userId,
                Name = name ?? $"Test Portfolio {Guid.NewGuid():N}",
                Description = description ?? "Test portfolio description",
                Currency = currency,
                IsDefault = isDefault
            };

            await context.Portfolios.AddAsync(portfolio);
            await context.SaveChangesAsync();

            return portfolio;
        }

        /// <summary>
        /// Creates multiple portfolios for a given user.
        /// </summary>
        public static async Task<List<Portfolio>> CreatePortfolios(ApplicationDbContext context, Guid userId, int count,
            bool makeFirstDefault = false)
        {
            var tasks = new List<Task<Portfolio>>();

            for (int i = 0; i < count; i++)
            {
                tasks.Add(CreatePortfolio(context, userId));
            }

            // Wait for all portfolios to be created in parallel
            var portfolios = await Task.WhenAll(tasks);
            return portfolios.ToList();
        }

        #endregion

        #region Security Creation

        /// <summary>
        /// Creates a test security
        /// </summary>
        /// <returns>Created security with generated ID</returns>
        /// <remarks>
        /// To use this later when testing holdings and transactions.
        /// Create basic security structure for now.
        /// </remarks>
        public static async Task<Security> CreateSecurity(ApplicationDbContext context, string? symbol = null,
            string? name = null, string securityType = "STOCK", string currency = "AUD", string exchange = "NYSE")
        {
            var security = new Security()
            {
                Name = name ?? "Test Security",
                Symbol = symbol ?? $"TEST{Guid.NewGuid():N}".Substring(0, 8).ToUpper(),
                SecurityType = securityType,
                Currency = currency,
                Exchange = exchange
            };

            await context.Securities.AddAsync(security);
            await context.SaveChangesAsync();

            return security;
        }

        #endregion

        #region Complex Test Scenarios

        /// <summary>
        /// Creates a complete test scenario: User with portfolios.
        /// Useful for tests that need a user with portfolios already set up.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="portfolioCount"></param>
        /// <returns></returns>
        public static async Task<User> CreateUserWithPortfolios(ApplicationDbContext context, int portfolioCount = 2)
        {
            var user = await CreateUser(context);
            await CreatePortfolios(context, user.Id, portfolioCount, makeFirstDefault: true);

            return user;
        }

        #endregion

        #region Cleanup Helpers

        /// <summary>
        /// Clears all data from test database.
        /// Useful for tests that need clean slate.
        /// </summary>
        /// <remarks>
        /// WARNING: This deletes EVERYTHING in database!
        /// Only use in tests with isolated databases.
        /// 
        /// Order matters due to foreign keys:
        /// 1. Delete child records first (portfolios)
        /// 2. Delete parent records last (users)
        /// 
        /// Why we need this: (particularly for TestContainer based tests - postgres)
        /// - Some tests need completely empty database
        /// - Integration tests might leave data behind
        /// - Ensures test isolation
        /// 
        /// Note: With in-memory database + Guid names, this is usually not needed
        /// because each test gets fresh database. But useful for TestContainer tests.
        /// </remarks>
        public static async Task ClearDatabase(ApplicationDbContext context)
        {
            // Remove in order (child to parent due to foreign keys)
            context.Portfolios.RemoveRange(context.Portfolios);
            context.Users.RemoveRange(context.Users);
            context.Securities.RemoveRange(context.Securities);

            await context.SaveChangesAsync();
        }

        #endregion
    }
}

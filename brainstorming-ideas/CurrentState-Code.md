# PortfolioTracker Solution Structure

## PortfolioTracker.API

### Program.cs

Configures DI, JWT, EF Core, logging, error handling, and environment-specific settings.

### Controllers

#### HoldingsController : ControllerBase

**Namespace:** PortfolioTracker.API.Controllers

**Methods:**

- `Task<ActionResult<IEnumerable<HoldingDto>>> GetPortfolioHoldings(Guid portfolioId)`
- `Task<ActionResult<HoldingDto>> GetHolding(Guid portfolioId, Guid holdingId)`
- `Task<ActionResult<HoldingDto>> CreateHolding(Guid portfolioId, CreateHoldingDto createHoldingDto)`
- `Task<ActionResult<HoldingDto>> UpdateHolding(Guid portfolioId, Guid holdingId, UpdateHoldingDto updateHoldingDto)`
- `Task<IActionResult> DeleteHolding(Guid portfolioId, Guid holdingId)`

#### TransactionsController : ControllerBase

**Namespace:** PortfolioTracker.API.Controllers

**Methods:**

- `Task<ActionResult<IEnumerable<TransactionDto>>> GetHoldingTransactions(Guid holdingId)`
- `Task<ActionResult<IEnumerable<TransactionDto>>> GetPortfolioTransactions(Guid portfolioId)`
- `Task<ActionResult<TransactionDto>> GetTransaction(Guid transactionId)`
- `Task<ActionResult<TransactionDto>> CreateTransaction(Guid portfolioId, CreateTransactionDto createTransactionDto)`
- `Task<ActionResult<TransactionDto>> UpdateTransaction(Guid transactionId, UpdateTransactionDto updateTransactionDto)`
- `Task<IActionResult> DeleteTransaction(Guid transactionId)`

#### SecuritiesController : ControllerBase

**Namespace:** PortfolioTracker.API.Controllers

**Methods:**

- `Task<ActionResult<List<SecurityDto>>> SearchSecurities(string query, int limit = 10)`
- `Task<ActionResult<SecurityDto>> GetSecurity(Guid id)`
- `Task<ActionResult<SecurityDto>> GetSecurityBySymbol(string symbol)`
- `Task<ActionResult<SecurityDto>> GetOrCreateSecurity(GetOrCreateSecurityRequest request)`
- `Task<ActionResult<SecurityDto>> CreateSecurity(CreateSecurityDto createSecurityDto)`
- `Task<ActionResult<SecurityDto>> UpdateSecurity(Guid id, UpdateSecurityDto updateSecurityDto)`
- `Task<IActionResult> DeleteSecurity(Guid id)`

#### TestController : ControllerBase

**Namespace:** PortfolioTracker.API.Controllers

**Methods:**

- `Task<IActionResult> TestStockData(string symbol)`

#### UsersController : ControllerBase

**Namespace:** PortfolioTracker.API.Controllers

**Methods:**

- `Task<ActionResult<IEnumerable<UserDto>>> GetUsers()`
- `Task<ActionResult<UserDto>> GetUser(Guid userId)`
- `Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)`
- `Task<ActionResult<UserDto>> UpdateUser(Guid userId, UpdateUserDto updateUserDto)`
- `Task<IActionResult> DeleteUser(Guid userId)`

#### PortfoliosController : ControllerBase

**Namespace:** PortfolioTracker.API.Controllers

**Methods:**

- `Task<ActionResult<IEnumerable<PortfolioDto>>> GetUserPortfolios(Guid userId)`
- `Task<ActionResult<PortfolioDto>> GetPortfolio(Guid userId, Guid portfolioId)`
- `Task<ActionResult<PortfolioDto>> GetDefaultPortfolio(Guid userId)`
- `Task<ActionResult<PortfolioDto>> CreatePortfolio(Guid userId, CreatePortfolioDto createPortfolioDto)`
- `Task<ActionResult<PortfolioDto>> UpdatePortfolio(Guid userId, Guid portfolioId, UpdatePortfolioDto updatePortfolioDto)`
- `Task<IActionResult> DeletePortfolio(Guid userId, Guid portfolioId)`
- `Task<IActionResult> SetAsDefault(Guid userId, Guid id)`

#### AuthController : ControllerBase

**Namespace:** PortfolioTracker.API.Controllers

**Methods:**

- `Task<ActionResult<AuthResponse>> Register(RegisterRequest request)`
- `Task<ActionResult<AuthResponse>> Login(LoginRequest request)`
- `ActionResult<UserInfo> GetCurrentUser()`

### Extensions

#### AuthExtensions (static)

**Namespace:** PortfolioTracker.API.Extensions

**Methods:**

- `Guid? GetAuthenticatedUserId(ClaimsPrincipal user)`
- `bool IsAuthorizedForUser(ClaimsPrincipal user, Guid requestedUserId)`

### Configuration

- `appsettings.json`
- `launchSettings.json`: Launch profiles for HTTP/HTTPS/IIS Express, default launch URL: Swagger

---

## PortfolioTracker.Core

### Entities

#### User

**Namespace:** PortfolioTracker.Core.Entities

- `Guid Id`
- `string Email`
- `string PasswordHash`
- `string? FullName`
- `DateTime CreatedAt`
- `DateTime UpdatedAt`
- `DateTime? LastLogin`
- `ICollection<Portfolio> Portfolios`

#### Portfolio

**Namespace:** PortfolioTracker.Core.Entities

- `Guid Id`
- `Guid UserId`
- `string Name`
- `string? Description`
- `string Currency`
- `bool IsDefault`
- `DateTime CreatedAt`
- `DateTime UpdatedAt`
- `User User`
- `ICollection<Holding> Holdings`
- `ICollection<PortfolioSnapshot> Snapshots`

#### Holding

**Namespace:** PortfolioTracker.Core.Entities

- `Guid Id`
- `Guid PortfolioId`
- `Guid SecurityId`
- `decimal TotalShares`
- `decimal? AverageCost`
- `DateTime CreatedAt`
- `DateTime UpdatedAt`
- `Portfolio Portfolio`
- `Security Security`
- `ICollection<Transaction> Transactions`
- `ICollection<Dividend> Dividends`

#### Security

**Namespace:** PortfolioTracker.Core.Entities

- `Guid Id`
- `string Symbol`
- `string Name`
- `string? Exchange`
- `string SecurityType`
- `string Currency`
- `string? Sector`
- `string? Industry`
- `DateTime CreatedAt`
- `DateTime UpdatedAt`
- `ICollection<Holding> Holdings`
- `ICollection<PriceHistory> PriceHistory`

#### Transaction

**Namespace:** PortfolioTracker.Core.Entities

- `Guid Id`
- `Guid HoldingId`
- `string TransactionType`
- `decimal Shares`
- `decimal PricePerShare`
- `decimal TotalAmount`
- `decimal Fees`
- `DateTime TransactionDate`
- `string? Notes`
- `DateTime CreatedAt`
- `Holding Holding`

#### PriceHistory

**Namespace:** PortfolioTracker.Core.Entities

- `Guid Id`
- `Guid SecurityId`
- `decimal Price`
- `decimal? OpenPrice`
- `decimal? HighPrice`
- `decimal? LowPrice`
- `decimal? ClosePrice`
- `long? Volume`
- `DateTime PriceDate`
- `DateTime CreatedAt`
- `Security Security`

#### Dividend

**Namespace:** PortfolioTracker.Core.Entities

- `Guid Id`
- `Guid HoldingId`
- `decimal AmountPerShare`
- `decimal TotalAmount`
- `DateTime PaymentDate`
- `DateTime? ExDividendDate`
- `DateTime CreatedAt`
- `Holding Holding`

#### PortfolioSnapshot

**Namespace:** PortfolioTracker.Core.Entities

- `Guid Id`
- `Guid PortfolioId`
- `decimal TotalValue`
- `decimal TotalCost`
- `decimal? TotalGainLoss`
- `DateTime SnapshotDate`
- `DateTime CreatedAt`
- `Portfolio Portfolio`

### DTOs

**Namespaces:**

- PortfolioTracker.Core.DTOs.User.UserDto
- PortfolioTracker.Core.DTOs.User.CreateUserDto
- PortfolioTracker.Core.DTOs.User.UpdateUserDto
- PortfolioTracker.Core.DTOs.Portfolio.PortfolioDto
- PortfolioTracker.Core.DTOs.Portfolio.CreatePortfolioDto
- PortfolioTracker.Core.DTOs.Portfolio.UpdatePortfolioDto
- PortfolioTracker.Core.DTOs.Security.SecurityDto
- PortfolioTracker.Core.DTOs.Security.CreateSecurityDto
- PortfolioTracker.Core.DTOs.Security.UpdateSecurityDto
- PortfolioTracker.Core.DTOs.Security.SecuritySearchDto
- PortfolioTracker.Core.DTOs.Holding.HoldingDto
- PortfolioTracker.Core.DTOs.Holding.CreateHoldingDto
- PortfolioTracker.Core.DTOs.Holding.UpdateHoldingDto
- PortfolioTracker.Core.DTOs.Holding.HoldingSummaryDto
- PortfolioTracker.Core.DTOs.Transaction.TransactionDto
- PortfolioTracker.Core.DTOs.Transaction.CreateTransactionDto
- PortfolioTracker.Core.DTOs.Transaction.UpdateTransactionDto
- PortfolioTracker.Core.DTOs.ExternalData.ExternalSecuritySearchDto
- PortfolioTracker.Core.DTOs.ExternalData.CompanyInfoDto
- PortfolioTracker.Core.DTOs.ExternalData.StockQuoteDto
- PortfolioTracker.Core.DTOs.ExternalData.HistoricalPriceDto
- PortfolioTracker.Core.DTOs.Auth.AuthResponse
- PortfolioTracker.Core.DTOs.Auth.RegisterRequest
- PortfolioTracker.Core.DTOs.Auth.LoginRequest
- PortfolioTracker.Core.DTOs.Auth.UserInfo

### Interfaces

#### IUserService

**Namespace:** PortfolioTracker.Core.Interfaces.Services

**Methods:**

- `Task<IEnumerable<UserDto>> GetAllUsersAsync()`
- `Task<UserDto?> GetUserByIdAsync(Guid id)`
- `Task<UserDto?> GetUserByEmailAsync(string email)`
- `Task<UserDto?> CreateUserAsync(CreateUserDto createUserDto)`
- `Task<UserDto?> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto)`
- `Task<bool> DeleteUserAsync(Guid id)`
- `Task<bool> UserExistsAsync(string email)`

#### IPortfolioService

**Namespace:** PortfolioTracker.Core.Interfaces.Services

**Methods:**

- `Task<IEnumerable<PortfolioDto>> GetUserPortfoliosAsync(Guid userId)`
- `Task<PortfolioDto?> GetPortfolioByIdAsync(Guid portfolioId, Guid userId)`
- `Task<PortfolioDto?> GetDefaultPortfolioAsync(Guid userId)`
- `Task<PortfolioDto> CreatePortfolioAsync(Guid userId, CreatePortfolioDto createPortfolioDto)`
- `Task<PortfolioDto?> UpdatePortfolioAsync(Guid portfolioId, Guid userId, UpdatePortfolioDto updatePortfolioDto)`
- `Task<bool> DeletePortfolioAsync(Guid portfolioId, Guid userId)`
- `Task<bool> SetAsDefaultAsync(Guid portfolioId, Guid userId)`

#### IJwtTokenService

**Namespace:** PortfolioTracker.Core.Interfaces.Services

**Methods:**

- `string GenerateToken(User user)`
- `ClaimsPrincipal? ValidateToken(string token)`

#### IAuthService

**Namespace:** PortfolioTracker.Core.Interfaces.Services

**Methods:**

- `Task<AuthResponse> RegisterAsync(RegisterRequest request)`
- `Task<AuthResponse> LoginAsync(LoginRequest request)`

#### IUserRepository : IRepository<User>

**Namespace:** PortfolioTracker.Core.Interfaces.Repositories

**Methods:**

- `Task<User?> GetByEmailAsync(string email)`
- `Task<bool> IsEmailTakenAsync(string email, Guid? excludeUserId = null)`

#### IPortfolioRepository : IRepository<Portfolio>

**Namespace:** PortfolioTracker.Core.Interfaces.Repositories

**Methods:**

- `Task<IEnumerable<Portfolio>> GetByUserIdAsync(Guid userId)`
- `Task<Portfolio?> GetByIdAndUserIdAsync(Guid portfolioId, Guid userId)`
- `Task<Portfolio?> GetDefaultPortfolioAsync(Guid userId)`
- `Task<bool> UserHasPortfolioWithNameAsync(Guid userId, string name, Guid? excludePortfolioId = null)`
- `Task SetAsDefaultAsync(Guid portfolioId, Guid userId)`
- `Task<IEnumerable<Portfolio>> GetWithHoldingsCountAsync(Guid userId)`

#### ISecurityRepository : IRepository<Security>

**Namespace:** PortfolioTracker.Core.Interfaces.Repositories

**Methods:**

- `Task<Security?> GetBySymbolAsync(string symbol)`
- `Task<List<Security>> SearchAsync(string query, int limit = 10)`
- `Task<bool> ExistsBySymbolAsync(string symbol)`

#### IRepository<T>

**Namespace:** PortfolioTracker.Core.Interfaces.Repositories

**Methods:**

- `Task<T?> GetByIdAsync(Guid id)`
- `Task<IEnumerable<T>> GetAllAsync()`
- `Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)`
- `Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)`
- `Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)`
- `Task<T> AddAsync(T entity)`
- `Task UpdateAsync(T entity)`
- `Task DeleteAsync(T entity)`
- `Task<int> SaveChangesAsync()`

---

### Services

#### UserService : IUserService

**Namespace:** PortfolioTracker.Core.Services

- Implements all `IUserService` methods
- Private fields: `_userRepository`, `_logger`

#### PortfolioService : IPortfolioService

**Namespace:** PortfolioTracker.Core.Services

- Implements all `IPortfolioService` methods
- Private fields: `_portfolioRepository`, `_userRepository`, `_logger`

#### JwtTokenService : IJwtTokenService

**Namespace:** PortfolioTracker.Core.Services

- Implements: `GenerateToken(User)`, `ValidateToken(string)`
- Private: `_jwtSettings`

#### AuthService : IAuthService

**Namespace:** PortfolioTracker.Core.Services

- Implements: `RegisterAsync(RegisterRequest)`, `LoginAsync(LoginRequest)`

#### HoldingService : IHoldingService

**Namespace:** PortfolioTracker.Core.Services

- Implements all `IHoldingService` methods
- Private fields: `_holdingRepository`, `_portfolioRepository`, `_securityRepository`, `_stockDataService`, `_logger`

#### TransactionService : ITransactionService

**Namespace:** PortfolioTracker.Core.Services

- Implements all `ITransactionService` methods
- Private fields: `_transactionRepository`, `_holdingRepository`, `_portfolioRepository`, `_logger`

#### SecurityService : ISecurityService

**Namespace:** PortfolioTracker.Core.Services

- Implements all `ISecurityService` methods
- Private fields: `_securityRepository`, `_stockDataService`, `_logger`

---

## PortfolioTracker.Infrastructure

### ApplicationDbContext : DbContext

**Namespace:** PortfolioTracker.Infrastructure.Data

**Properties:**

- `DbSet<User> Users`
- `DbSet<Portfolio> Portfolios`
- `DbSet<Holding> Holdings`
- `DbSet<Security> Securities`
- `DbSet<Transaction> Transactions`
- `DbSet<PriceHistory> PriceHistory`
- `DbSet<Dividend> Dividends`
- `DbSet<PortfolioSnapshot> PortfolioSnapshots`

**Methods:**

- `OnModelCreating(ModelBuilder)`
- `SaveChangesAsync(CancellationToken)`
- Private: `ConfigureIndexes`, `ConfigureUniqueConstraints`, `ConfigureRelationships`, `ConfigureDefaults`

### Repositories

#### Repository<T> : IRepository<T>

**Namespace:** PortfolioTracker.Infrastructure.Repositories

- Implements all `IRepository<T>` methods
- Protected: `Context`, `DbSet`

#### UserRepository : Repository<User>, IUserRepository

**Namespace:** PortfolioTracker.Infrastructure.Repositories

- Implements `IUserRepository` methods

#### PortfolioRepository : Repository<Portfolio>, IPortfolioRepository

**Namespace:** PortfolioTracker.Infrastructure.Repositories

- Implements `IPortfolioRepository` methods

#### SecurityRepository : Repository<Security>, ISecurityRepository

**Namespace:** PortfolioTracker.Infrastructure.Repositories

- Implements `ISecurityRepository` methods

#### HoldingRepository : Repository<Holding>, IHoldingRepository

**Namespace:** PortfolioTracker.Infrastructure.Repositories

- Implements `IHoldingRepository` methods

#### TransactionRepository : Repository<Transaction>, ITransactionRepository

**Namespace:** PortfolioTracker.Infrastructure.Repositories

- Implements `ITransactionRepository` methods

---

## PortfolioTracker.UnitTests

### TestBase (abstract)

**Namespace:** PortfolioTracker.UnitTests

**Methods:**

- `CreateMockLogger<T>()`
- `CreateMockLoggerWithVerify<T>()`

### Services

#### UserServiceTests : TestBase

**Namespace:** PortfolioTracker.UnitTests.Services

- Fields: `_mockUserRepository`, `_userService`
- Methods: All test methods for user service (e.g., `GetAllUsersAsync_WhenUsersExist_ShouldReturnAllUsers()`, etc.)

#### PortfolioServiceTests : TestBase

**Namespace:** PortfolioTracker.UnitTests.Services

- Fields: `_mockPortfolioRepository`, `_mockUserRepository`, `_portfolioService`
- Methods: All test methods for portfolio service

### Extensions

#### AuthExtensionsTests

**Namespace:** PortfolioTracker.UnitTests.Extensions

- Methods: All test methods for `AuthExtensions`

---

## PortfolioTracker.IntegrationTests

### IntegrationTestBase (abstract)

**Namespace:** PortfolioTracker.IntegrationTests

**Fields:**

- `Client`, `Factory`, `Context`, `_scope`

**Methods:**

- `CleanDatabase()`, `Dispose()`, `ReloadFromDb<T>(T)`, `AuthenticateUserAsync(string, string)`
- `RegisterAndAuthenticateAsync(...)`, `ClearAuthentication()`

### Helpers

### DTOs

**Namespaces:**
PortfolioTracker.Core.DTOs.Holding.HoldingDto
PortfolioTracker.Core.DTOs.Holding.CreateHoldingDto
PortfolioTracker.Core.DTOs.Holding.UpdateHoldingDto
PortfolioTracker.Core.DTOs.Holding.HoldingSummaryDto
PortfolioTracker.Core.DTOs.Transaction.TransactionDto
PortfolioTracker.Core.DTOs.Transaction.CreateTransactionDto
PortfolioTracker.Core.DTOs.Transaction.UpdateTransactionDto
PortfolioTracker.Core.DTOs.ExternalData.ExternalSecuritySearchDto
PortfolioTracker.Core.DTOs.ExternalData.CompanyInfoDto
PortfolioTracker.Core.DTOs.ExternalData.StockQuoteDto
PortfolioTracker.Core.DTOs.ExternalData.HistoricalPriceDto

#### TestDataBuilder (static)

### Enums

#### TransactionType

**Namespace:** PortfolioTracker.Core.Enums

### Helpers

#### HttpClientExtensions (static)

**Namespace:** PortfolioTracker.Core.Helpers
**Methods:**

- `PostAsJsonAsync<T>(...)`
- `PutAsJsonAsync<T>(...)`
- `ReadAsJsonAsync<T>(...)`
- `ReadAsSuccessfulJsonAsync<T>(...)`
- `ReadAsStringAsync(...)`
- `BuildUrl(...)`

### Interfaces

**Namespace:** PortfolioTracker.IntegrationTests.Helpers

#### IHoldingService

**Namespace:** PortfolioTracker.Core.Interfaces.Services
**Methods:**

- `Task<IEnumerable<HoldingDto>> GetPortfolioHoldingsAsync(Guid portfolioId, Guid userId)`
- `Task<HoldingDto?> GetHoldingByIdAsync(Guid holdingId, Guid portfolioId, Guid userId)`
- `Task<HoldingDto?> CreateHoldingAsync(Guid portfolioId, Guid userId, CreateHoldingDto createHoldingDto)`
- `Task<HoldingDto?> UpdateHoldingAsync(Guid holdingId, Guid portfolioId, Guid userId, UpdateHoldingDto updateHoldingDto)`
- `Task<bool> DeleteHoldingAsync(Guid holdingId, Guid portfolioId, Guid userId)`

#### ISecurityService

**Namespace:** PortfolioTracker.Core.Interfaces.Services
**Methods:**

- `Task<List<SecurityDto>> SearchSecuritiesAsync(string query, int limit = 10)`
- `Task<SecurityDto?> GetSecurityByIdAsync(Guid id)`
- `Task<SecurityDto?> GetSecurityBySymbolAsync(string symbol)`
- `Task<SecurityDto> GetOrCreateSecurityAsync(string symbol)`

#### IStockDataService

**Namespace:** PortfolioTracker.Core.Interfaces.Services
**Methods:**

- `Task<StockQuoteDto?> GetQuoteAsync(string symbol)`
- `Task<CompanyInfoDto?> GetCompanyInfoAsync(string symbol)`
- `Task<List<ExternalSecuritySearchDto>> SearchSecuritiesAsync(string query, int limit = 10)`
- `Task<List<HistoricalPriceDto>?> GetHistoricalPricesAsync(string symbol, DateTime startDate, DateTime endDate)`

#### ITransactionService

**Namespace:** PortfolioTracker.Core.Interfaces.Services
**Methods:**

- `Task<IEnumerable<TransactionDto>> GetHoldingTransactionsAsync(Guid holdingId, Guid userId)`
- `Task<IEnumerable<TransactionDto>> GetPortfolioTransactionsAsync(Guid portfolioId, Guid userId)`
- `Task<TransactionDto?> GetTransactionByIdAsync(Guid transactionId, Guid userId)`
- `Task<TransactionDto> CreateTransactionAsync(Guid userId, CreateTransactionDto createTransactionDto)`
- `Task<TransactionDto?> UpdateTransactionAsync(Guid transactionId, Guid userId, UpdateTransactionDto updateTransactionDto)`
- `Task<bool> DeleteTransactionAsync(Guid transactionId, Guid userId)`

#### IHoldingRepository : IRepository<Holding>

**Namespace:** PortfolioTracker.Core.Interfaces.Repositories
**Methods:**

- `Task<IEnumerable<Holding>> GetByPortfolioIdAsync(Guid portfolioId)`
- `Task<Holding?> GetByIdWithDetailsAsync(Guid holdingId)`
- `Task<Holding?> GetByPortfolioAndSecurityAsync(Guid portfolioId, Guid securityId)`
- `Task<bool> ExistsInPortfolioAsync(Guid holdingId, Guid portfolioId)`
- `Task<IEnumerable<Holding>> GetWithTransactionsAsync(Guid portfolioId)`

#### ITransactionRepository : IRepository<Transaction>

**Namespace:** PortfolioTracker.Core.Interfaces.Repositories
**Methods:**

- `Task<IEnumerable<Transaction>> GetByHoldingIdAsync(Guid holdingId)`
- `Task<Transaction?> GetByIdWithDetailsAsync(Guid transactionId)`
- `Task<IEnumerable<Transaction>> GetByPortfolioIdAsync(Guid portfolioId)`
- `Task<bool> ExistsInHoldingAsync(Guid transactionId, Guid holdingId)`

**Methods:**

### HoldingService : IHoldingService

**Namespace:** PortfolioTracker.Core.Services
Implements all `IHoldingService` methods
Private fields: `_holdingRepository`, `_portfolioRepository`, `_securityRepository`, `_stockDataService`, `_logger`

### TransactionService : ITransactionService

**Namespace:** PortfolioTracker.Core.Services
Implements all `ITransactionService` methods
Private fields: `_transactionRepository`, `_holdingRepository`, `_portfolioRepository`, `_logger`

### SecurityService : ISecurityService

**Namespace:** PortfolioTracker.Core.Services
Implements all `ISecurityService` methods
Private fields: `_securityRepository`, `_stockDataService`, `_logger`

- `CreateUser(...)`, `CreateUsers(...)`, `CreatePortfolio(...)`

### ApplicationDbContext : DbContext

#### HoldingRepository : Repository<Holding>, IHoldingRepository

**Namespace:** PortfolioTracker.Infrastructure.Repositories
Implements `IHoldingRepository` methods

#### TransactionRepository : Repository<Transaction>, ITransactionRepository

**Namespace:** PortfolioTracker.Infrastructure.Repositories
Implements `ITransactionRepository` methods

### Services

#### StockDataCachingService : IStockDataService

**Namespace:** PortfolioTracker.Infrastructure.Services

- Implements all `IStockDataService` methods, wraps another IStockDataService for caching
- Private fields: `_innerService`, `_cache`, `_logger`, `_cacheSettings`

#### AlphaVantageService : IStockDataService

**Namespace:** PortfolioTracker.Infrastructure.Services

- Implements all `IStockDataService` methods for Alpha Vantage API
- Private fields: `_httpClient`, `_logger`, `_apiKey`, `_baseUrl`

### Configuration

#### StockDataCacheSettings

**Namespace:** PortfolioTracker.Infrastructure.Configuration

- Properties: `QuoteCacheDurationMinutes`, `CompanyInfoCacheDurationDays`, `HistoricalDataCacheDurationDays`

#### RedisSettings

**Namespace:** PortfolioTracker.Infrastructure.Configuration

- Properties: `ConnectionString`, `InstanceName`

#### AlphaVantageSettings

**Namespace:** PortfolioTracker.Infrastructure.Configuration

- Properties: `ApiKey`, `BaseUrl`, `TimeoutInSeconds`, `EnableCaching`, `SecurityInfoCacheMinutes`, `PriceCacheMinutes`

### HoldingsController : ControllerBase

**Namespace:** PortfolioTracker.API.Controllers
**Methods:**

- `Task<ActionResult<IEnumerable<HoldingDto>>> GetPortfolioHoldings(Guid portfolioId)`
- `Task<ActionResult<HoldingDto>> GetHolding(Guid portfolioId, Guid holdingId)`
- `Task<ActionResult<HoldingDto>> CreateHolding(Guid portfolioId, CreateHoldingDto createHoldingDto)`
- `Task<ActionResult<HoldingDto>> UpdateHolding(Guid portfolioId, Guid holdingId, UpdateHoldingDto updateHoldingDto)`
- `Task<IActionResult> DeleteHolding(Guid portfolioId, Guid holdingId)`

### TransactionsController : ControllerBase

**Namespace:** PortfolioTracker.API.Controllers
**Methods:**

- `Task<ActionResult<IEnumerable<TransactionDto>>> GetHoldingTransactions(Guid holdingId)`
- `Task<ActionResult<IEnumerable<TransactionDto>>> GetPortfolioTransactions(Guid portfolioId)`
- `Task<ActionResult<TransactionDto>> GetTransaction(Guid transactionId)`
- `Task<ActionResult<TransactionDto>> CreateTransaction(Guid portfolioId, CreateTransactionDto createTransactionDto)`
- `Task<ActionResult<TransactionDto>> UpdateTransaction(Guid transactionId, UpdateTransactionDto updateTransactionDto)`
- `Task<IActionResult> DeleteTransaction(Guid transactionId)`

### TestController : ControllerBase

**Namespace:** PortfolioTracker.API.Controllers
**Methods:**

- `Task<IActionResult> TestStockData(string symbol)`

### UsersControllerTests, PortfoliosControllerTests, AuthControllerTests, HoldingsControllerTests, TransactionsControllerTests

**Methods:**

- `PostAsJsonAsync<T>(...)`, `PutAsJsonAsync<T>(...)`, `ReadAsJsonAsync<T>(...)`
- `ReadAsSuccessfulJsonAsync<T>(...)`, `ReadAsStringAsync(...)`, `BuildUrl(...)`

#### HttpResponseExtensions (static)

**Namespace:** PortfolioTracker.IntegrationTests.Helpers

**Methods:**

- `IsSuccessful(...)`, `GetStatusCodeAsInt(...)`

### Fixtures

#### PostgresIntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime

**Namespace:** PortfolioTracker.IntegrationTests.Fixtures

**Fields:**

- `_postgresContainer`, `_connectionString`

**Methods:**

- `InitializeAsync()`, `ConfigureWebHost(IWebHostBuilder)`, `DisposeAsync()`

#### IntegrationTestWebAppFactory : WebApplicationFactory<Program>

**Namespace:** PortfolioTracker.IntegrationTests.Fixtures

**Fields:**

- `_dbName`

**Methods:**

- `ConfigureWebHost(IWebHostBuilder)`

---

## API

### UsersControllerTests, PortfoliosControllerTests, AuthControllerTests

**Namespace:** PortfolioTracker.IntegrationTests.API

- Methods: All test methods for API endpoints

---

## Configuration

- `appsettings.Testing.json`

---

## Docker Compose Configuration

### `docker-compose.yml`

**Services:**

- **postgres**: PostgreSQL 16 with persistent storage, healthcheck, and exposed on port 5432.

  - **Environment**: `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`
  - **Volume**: `postgres_data`
  - **Network**: `portfolio-network`

- **pgadmin**: pgAdmin 4 for DB management, exposed on port 5050.

  - **Environment**: `PGADMIN_DEFAULT_EMAIL`, `PGADMIN_DEFAULT_PASSWORD`
  - **Volume**: `pgadmin_data`
  - **Depends on**: `postgres`
  - **Network**: `portfolio-network`

- **redis**: Redis 7 for caching, exposed on port 6379.
  - **Volume**: `redis_data`
  - **Network**: `portfolio-network`
  - Healthcheck and append-only persistence enabled.

**Volumes:**

- `postgres_data`, `pgadmin_data`, `redis_data` (all local)

**Network:**

- `portfolio-network` (bridge)

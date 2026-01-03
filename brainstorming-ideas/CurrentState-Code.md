# PortfolioTracker Solution Structure

## PortfolioTracker.API

### Program.cs

Configures DI, JWT, EF Core, logging, error handling, and environment-specific settings.

### Controllers

#### UsersController : ControllerBase

**Methods:**

- `Task<ActionResult<IEnumerable<UserDto>>> GetUsers()`
- `Task<ActionResult<UserDto>> GetUser(Guid userId)`
- `Task<ActionResult<UserDto>> CreateUser(CreateUserDto createUserDto)`
- `Task<ActionResult<UserDto>> UpdateUser(Guid userId, UpdateUserDto updateUserDto)`
- `Task<IActionResult> DeleteUser(Guid userId)`

#### PortfoliosController : ControllerBase

**Methods:**

- `Task<ActionResult<IEnumerable<PortfolioDto>>> GetUserPortfolios(Guid userId)`
- `Task<ActionResult<PortfolioDto>> GetPortfolio(Guid userId, Guid portfolioId)`
- `Task<ActionResult<PortfolioDto>> GetDefaultPortfolio(Guid userId)`
- `Task<ActionResult<PortfolioDto>> CreatePortfolio(Guid userId, CreatePortfolioDto createPortfolioDto)`
- `Task<ActionResult<PortfolioDto>> UpdatePortfolio(Guid userId, Guid portfolioId, UpdatePortfolioDto updatePortfolioDto)`
- `Task<IActionResult> DeletePortfolio(Guid userId, Guid portfolioId)`
- `Task<IActionResult> SetAsDefault(Guid userId, Guid id)`

#### AuthController : ControllerBase

**Methods:**

- `Task<ActionResult<AuthResponse>> Register(RegisterRequest request)`
- `Task<ActionResult<AuthResponse>> Login(LoginRequest request)`
- `ActionResult<UserInfo> GetCurrentUser()`

### Extensions

#### AuthExtensions (static)

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

- `Guid Id`
- `string Email`
- `string PasswordHash`
- `string? FullName`
- `DateTime CreatedAt`
- `DateTime UpdatedAt`
- `DateTime? LastLogin`
- `ICollection<Portfolio> Portfolios`

#### Portfolio

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

- `Guid Id`
- `Guid HoldingId`
- `decimal AmountPerShare`
- `decimal TotalAmount`
- `DateTime PaymentDate`
- `DateTime? ExDividendDate`
- `DateTime CreatedAt`
- `Holding Holding`

#### PortfolioSnapshot

- `Guid Id`
- `Guid PortfolioId`
- `decimal TotalValue`
- `decimal TotalCost`
- `decimal? TotalGainLoss`
- `DateTime SnapshotDate`
- `DateTime CreatedAt`
- `Portfolio Portfolio`

### DTOs

- `UserDto`, `CreateUserDto`, `UpdateUserDto`, `PortfolioDto`, `CreatePortfolioDto`, `UpdatePortfolioDto`
- `SecurityDto`, `CreateSecurityDto`, `UpdateSecurityDto`, `SecuritySearchDto`
- `AuthResponse`, `RegisterRequest`, `LoginRequest`, `UserInfo`

### Interfaces

#### IUserService

**Methods:**

- `Task<IEnumerable<UserDto>> GetAllUsersAsync()`
- `Task<UserDto?> GetUserByIdAsync(Guid id)`
- `Task<UserDto?> GetUserByEmailAsync(string email)`
- `Task<UserDto?> CreateUserAsync(CreateUserDto createUserDto)`
- `Task<UserDto?> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto)`
- `Task<bool> DeleteUserAsync(Guid id)`
- `Task<bool> UserExistsAsync(string email)`

#### IPortfolioService

**Methods:**

- `Task<IEnumerable<PortfolioDto>> GetUserPortfoliosAsync(Guid userId)`
- `Task<PortfolioDto?> GetPortfolioByIdAsync(Guid portfolioId, Guid userId)`
- `Task<PortfolioDto?> GetDefaultPortfolioAsync(Guid userId)`
- `Task<PortfolioDto> CreatePortfolioAsync(Guid userId, CreatePortfolioDto createPortfolioDto)`
- `Task<PortfolioDto?> UpdatePortfolioAsync(Guid portfolioId, Guid userId, UpdatePortfolioDto updatePortfolioDto)`
- `Task<bool> DeletePortfolioAsync(Guid portfolioId, Guid userId)`
- `Task<bool> SetAsDefaultAsync(Guid portfolioId, Guid userId)`

#### IJwtTokenService

**Methods:**

- `string GenerateToken(User user)`
- `ClaimsPrincipal? ValidateToken(string token)`

#### IAuthService

**Methods:**

- `Task<AuthResponse> RegisterAsync(RegisterRequest request)`
- `Task<AuthResponse> LoginAsync(LoginRequest request)`

#### IUserRepository : IRepository<User>

**Methods:**

- `Task<User?> GetByEmailAsync(string email)`
- `Task<bool> IsEmailTakenAsync(string email, Guid? excludeUserId = null)`

#### IPortfolioRepository : IRepository<Portfolio>

**Methods:**

- `Task<IEnumerable<Portfolio>> GetByUserIdAsync(Guid userId)`
- `Task<Portfolio?> GetByIdAndUserIdAsync(Guid portfolioId, Guid userId)`
- `Task<Portfolio?> GetDefaultPortfolioAsync(Guid userId)`
- `Task<bool> UserHasPortfolioWithNameAsync(Guid userId, string name, Guid? excludePortfolioId = null)`
- `Task SetAsDefaultAsync(Guid portfolioId, Guid userId)`
- `Task<IEnumerable<Portfolio>> GetWithHoldingsCountAsync(Guid userId)`

#### ISecurityRepository : IRepository<Security>

**Methods:**

- `Task<Security?> GetBySymbolAsync(string symbol)`
- `Task<List<Security>> SearchAsync(string query, int limit = 10)`
- `Task<bool> ExistsBySymbolAsync(string symbol)`

#### IRepository<T>

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

## Services

### UserService : IUserService

- Implements all `IUserService` methods
- Private fields: `_userRepository`, `_logger`

### PortfolioService : IPortfolioService

- Implements all `IPortfolioService` methods
- Private fields: `_portfolioRepository`, `_userRepository`, `_logger`

### JwtTokenService : IJwtTokenService

- Implements: `GenerateToken(User)`, `ValidateToken(string)`
- Private: `_jwtSettings`

### AuthService : IAuthService

- Implements: `RegisterAsync(RegisterRequest)`, `LoginAsync(LoginRequest)`

---

## PortfolioTracker.Infrastructure

### ApplicationDbContext : DbContext

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

- Implements all `IRepository<T>` methods
- Protected: `Context`, `DbSet`

#### UserRepository : Repository<User>, IUserRepository

- Implements `IUserRepository` methods

#### PortfolioRepository : Repository<Portfolio>, IPortfolioRepository

- Implements `IPortfolioRepository` methods

#### SecurityRepository : Repository<Security>, ISecurityRepository

- Implements `ISecurityRepository` methods

---

## PortfolioTracker.UnitTests

### TestBase (abstract)

**Methods:**

- `CreateMockLogger<T>()`
- `CreateMockLoggerWithVerify<T>()`

### Services

#### UserServiceTests : TestBase

- Fields: `_mockUserRepository`, `_userService`
- Methods: All test methods for user service (e.g., `GetAllUsersAsync_WhenUsersExist_ShouldReturnAllUsers()`, etc.)

#### PortfolioServiceTests : TestBase

- Fields: `_mockPortfolioRepository`, `_mockUserRepository`, `_portfolioService`
- Methods: All test methods for portfolio service

### Extensions

#### AuthExtensionsTests

- Methods: All test methods for `AuthExtensions`

---

## PortfolioTracker.IntegrationTests

### IntegrationTestBase (abstract)

**Fields:**

- `Client`, `Factory`, `Context`, `_scope`

**Methods:**

- `CleanDatabase()`, `Dispose()`, `ReloadFromDb<T>(T)`, `AuthenticateUserAsync(string, string)`
- `RegisterAndAuthenticateAsync(...)`, `ClearAuthentication()`

### Helpers

#### TestDataBuilder (static)

**Methods:**

- `CreateUser(...)`, `CreateUsers(...)`, `CreatePortfolio(...)`
- `CreatePortfolios(...)`, `CreateSecurity(...)`, `CreateUserWithPortfolios(...)`, `ClearDatabase(...)`

#### TestEntityExtensions (static)

- Fluent builder methods for test entities

#### HttpClientExtensions (static)

**Methods:**

- `PostAsJsonAsync<T>(...)`, `PutAsJsonAsync<T>(...)`, `ReadAsJsonAsync<T>(...)`
- `ReadAsSuccessfulJsonAsync<T>(...)`, `ReadAsStringAsync(...)`, `BuildUrl(...)`

#### HttpResponseExtensions (static)

**Methods:**

- `IsSuccessful(...)`, `GetStatusCodeAsInt(...)`

### Fixtures

#### PostgresIntegrationTestFactory : WebApplicationFactory<Program>, IAsyncLifetime

**Fields:**

- `_postgresContainer`, `_connectionString`

**Methods:**

- `InitializeAsync()`, `ConfigureWebHost(IWebHostBuilder)`, `DisposeAsync()`

#### IntegrationTestWebAppFactory : WebApplicationFactory<Program>

**Fields:**

- `_dbName`

**Methods:**

- `ConfigureWebHost(IWebHostBuilder)`

---

## API

### UsersControllerTests, PortfoliosControllerTests, AuthControllerTests

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

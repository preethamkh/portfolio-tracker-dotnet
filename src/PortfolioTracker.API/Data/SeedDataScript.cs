using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Core.Entities;
using PortfolioTracker.Core.Enums;
using PortfolioTracker.Infrastructure.Data;

namespace PortfolioTracker.API.Data;

/// <summary>
/// Seed data script for development/testing.
/// Creates a test user with portfolio, securities, holdings, and transactions.
/// </summary>
public static class SeedDataScript
{
    public static async Task SeedTestDataAsync(ApplicationDbContext context)
    {
        // Check if we already have data
        if (await context.Users.AnyAsync())
        {
            Console.WriteLine("Database already has data. Skipping seed.");
            return;
        }

        // ========================================================================
        // 1. CREATE TEST USER
        // ========================================================================

        // Seed initial data
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            FullName = "Test User",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            CreatedAt = DateTime.UtcNow,
            LastLogin = null
        };

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Created user: {user.Email}");

        // ========================================================================
        // 2. CREATE DEFAULT PORTFOLIO
        // ========================================================================

        var portfolio = new Portfolio
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Name = "My Investment Portfolio",
            Description = "Primary investment portfolio for long-term growth",
            Currency = "AUD",
            IsDefault = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await context.Portfolios.AddAsync(portfolio);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Created portfolio: {portfolio.Name}");

        // ========================================================================
        // 3. CREATE SECURITIES (ASX ETFs)
        // ========================================================================

        var securities = new List<Security>
        {
            new Security
            {
                Id = Guid.NewGuid(),
                Symbol = "VGS.AX",
                Name = "Vanguard MSCI Index ETF",
                Exchange = "ASX",
                SecurityType = "ETF",
                Currency = "AUD",
                Sector = "Diversified",
                Industry = "Index Fund",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Security
            {
                Id = Guid.NewGuid(),
                Symbol = "IVV.AX",
                Name = "iShares S&P 500 ETF",
                Exchange = "ASX",
                SecurityType = "ETF",
                Currency = "AUD",
                Sector = "Diversified",
                Industry = "Index Fund",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Security
            {
                Id = Guid.NewGuid(),
                Symbol = "VAS.AX",
                Name = "Vanguard Australian Shares Index ETF",
                Exchange = "ASX",
                SecurityType = "ETF",
                Currency = "AUD",
                Sector = "Diversified",
                Industry = "Index Fund",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Security
            {
                Id = Guid.NewGuid(),
                Symbol = "NDQ.AX",
                Name = "BetaShares NASDAQ 100 ETF",
                Exchange = "ASX",
                SecurityType = "ETF",
                Currency = "AUD",
                Sector = "Technology",
                Industry = "Index Fund",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Security
            {
                Id = Guid.NewGuid(),
                Symbol = "A200.AX",
                Name = "BetaShares Australia 200 ETF",
                Exchange = "ASX",
                SecurityType = "ETF",
                Currency = "AUD",
                Sector = "Diversified",
                Industry = "Index Fund",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        await context.Securities.AddRangeAsync(securities);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Created {securities.Count} securities");

        // ========================================================================
        // 4. CREATE HOLDINGS WITH TRANSACTIONS
        // ========================================================================

        // HOLDING 1: VGS.AX (100 shares)
        var vgsHolding = new Holding
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolio.Id,
            SecurityId = securities[0].Id, // VGS.AX
            TotalShares = 100,
            AverageCost = 147.74m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var vgsTransactions = new List<Transaction>
        {
            new()
            {
                Id = Guid.NewGuid(),
                HoldingId = vgsHolding.Id,
                TransactionType = TransactionType.Buy,
                Shares = 50,
                PricePerShare = 145.00m,
                TotalAmount = 7250.00m,
                Fees = 9.95m,
                TransactionDate = DateTime.UtcNow.AddMonths(-6),
                Notes = "Initial purchase",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                HoldingId = vgsHolding.Id,
                TransactionType = TransactionType.Buy,
                Shares = 50,
                PricePerShare = 150.48m,
                TotalAmount = 7524.00m,
                Fees = 9.95m,
                TransactionDate = DateTime.UtcNow.AddMonths(-3),
                Notes = "Adding to position",
                CreatedAt = DateTime.UtcNow
            }
        };

        // HOLDING 2: IVV.AX (228 shares)
        var ivvHolding = new Holding
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolio.Id,
            SecurityId = securities[1].Id, // IVV.AX
            TotalShares = 228,
            AverageCost = 66.00m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var ivvTransactions = new List<Transaction>
        {
            new()
            {
                Id = Guid.NewGuid(),
                HoldingId = ivvHolding.Id,
                TransactionType = TransactionType.Buy,
                Shares = 100,
                PricePerShare = 65.00m,
                TotalAmount = 6500.00m,
                Fees = 9.95m,
                TransactionDate = DateTime.UtcNow.AddMonths(-8),
                Notes = "Initial purchase",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                HoldingId = ivvHolding.Id,
                TransactionType = TransactionType.Buy,
                Shares = 100,
                PricePerShare = 67.00m,
                TotalAmount = 6700.00m,
                Fees = 9.95m,
                TransactionDate = DateTime.UtcNow.AddMonths(-4),
                Notes = "Second purchase",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                HoldingId = ivvHolding.Id,
                TransactionType = TransactionType.Buy,
                Shares = 28,
                PricePerShare = 66.00m,
                TotalAmount = 1848.00m,
                Fees = 9.95m,
                TransactionDate = DateTime.UtcNow.AddMonths(-1),
                Notes = "Topping up",
                CreatedAt = DateTime.UtcNow
            }
        };

        // HOLDING 3: VAS.AX (150 shares)
        var vasHolding = new Holding
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolio.Id,
            SecurityId = securities[2].Id, // VAS.AX
            TotalShares = 150,
            AverageCost = 85.50m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var vasTransactions = new List<Transaction>
        {
            new()
            {
                Id = Guid.NewGuid(),
                HoldingId = vasHolding.Id,
                TransactionType = TransactionType.Buy,
                Shares = 150,
                PricePerShare = 85.50m,
                TotalAmount = 12825.00m,
                Fees = 9.95m,
                TransactionDate = DateTime.UtcNow.AddMonths(-5),
                Notes = "Diversifying into Australian market",
                CreatedAt = DateTime.UtcNow
            }
        };

        // HOLDING 4: NDQ.AX (75 shares)
        var ndqHolding = new Holding
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolio.Id,
            SecurityId = securities[3].Id, // NDQ.AX
            TotalShares = 75,
            AverageCost = 32.20m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var ndqTransactions = new List<Transaction>
        {
            new()
            {
                Id = Guid.NewGuid(),
                HoldingId = ndqHolding.Id,
                TransactionType = TransactionType.Buy,
                Shares = 75,
                PricePerShare = 32.20m,
                TotalAmount = 2415.00m,
                Fees = 9.95m,
                TransactionDate = DateTime.UtcNow.AddMonths(-2),
                Notes = "Tech exposure",
                CreatedAt = DateTime.UtcNow
            }
        };

        // Add all holdings and transactions
        await context.Holdings.AddRangeAsync(vgsHolding, ivvHolding, vasHolding, ndqHolding);
        await context.Transactions.AddRangeAsync(vgsTransactions);
        await context.Transactions.AddRangeAsync(ivvTransactions);
        await context.Transactions.AddRangeAsync(vasTransactions);
        await context.Transactions.AddRangeAsync(ndqTransactions);

        await context.SaveChangesAsync();
        Console.WriteLine("✓ Created 4 holdings with transactions");

        // ========================================================================
        // 5. CREATE PRICE HISTORY (Optional - for charts later)
        // ========================================================================

        // Add some historical prices for VGS.AX (last 30 days)
        var priceHistory = new List<PriceHistory>();
        var baseDate = DateTime.UtcNow.AddDays(-30);
        var basePrice = 145.00m;

        for (int i = 0; i < 30; i++)
        {
            var date = baseDate.AddDays(i);
            // Skip weekends
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
                continue;

            // Simulate price movement
            var randomChange = (decimal)(new Random().NextDouble() * 4 - 2); // -2 to +2
            basePrice += randomChange;

            priceHistory.Add(new PriceHistory
            {
                Id = Guid.NewGuid(),
                SecurityId = securities[0].Id, // VGS.AX
                Price = basePrice,
                OpenPrice = basePrice - 0.5m,
                HighPrice = basePrice + 1.0m,
                LowPrice = basePrice - 1.0m,
                ClosePrice = basePrice,
                Volume = 1000000 + new Random().Next(500000),
                PriceDate = date,
                CreatedAt = DateTime.UtcNow
            });
        }

        await context.PriceHistory.AddRangeAsync(priceHistory);
        await context.SaveChangesAsync();
        Console.WriteLine($"✓ Created {priceHistory.Count} price history records");

        // ========================================================================
        // SUMMARY
        // ========================================================================

        Console.WriteLine("\n=== SEED DATA SUMMARY ===");
        Console.WriteLine($"User: {user.Email} / password123");
        Console.WriteLine($"Portfolio: {portfolio.Name}");
        Console.WriteLine($"Securities: {securities.Count}");
        Console.WriteLine($"Holdings: 4");
        Console.WriteLine($"Transactions: {vgsTransactions.Count + ivvTransactions.Count + vasTransactions.Count + ndqTransactions.Count}");
        Console.WriteLine($"\nTotal Portfolio Cost: ~A${(100 * 147.74m + 228 * 66.00m + 150 * 85.50m + 75 * 32.20m):N2}");
        Console.WriteLine("\n✓ Seed data complete!");
        Console.WriteLine("\nNOTE: This seed data does NOT include current prices.");
        Console.WriteLine("You'll need to fetch live prices from Alpha Vantage or manually set them.");
    }
}

using Microsoft.Extensions.Logging;
using PortfolioTracker.Core.DTOs.Portfolio;
using PortfolioTracker.Core.Entities;
using PortfolioTracker.Core.Interfaces.Repositories;
using PortfolioTracker.Core.Interfaces.Services;

namespace PortfolioTracker.Core.Services
{
    /// <summary>
    /// Service implementation for portfolio management.
    /// Contains business logic for portfolio operations with authorization.
    /// </summary>
    /// <remarks>
    /// Key Business Rules Implemented:
    /// 1. Users can only access their own portfolios (authorization)
    /// 2. Portfolio names must be unique per user
    /// 3. Only one default portfolio per user
    /// 4. If user sets IsDefault=true on creation, unset other defaults
    /// 5. Cascade delete handled by database (portfolios → holdings → transactions)
    /// </remarks>
    public class PortfolioService : IPortfolioService
    {
        private readonly IPortfolioRepository _portfolioRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<PortfolioService> _logger;

        public PortfolioService(IPortfolioRepository portfolioRepository, IUserRepository userRepository, ILogger<PortfolioService> logger)
        {
            _portfolioRepository = portfolioRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all portfolios for a specific user.
        /// </summary>
        public async Task<IEnumerable<PortfolioDto>> GetUserPortfoliosAsync(Guid userId)
        {
            _logger.LogInformation("Retrieving portfolios for user: {UserId}", userId);

            var portfolios = await _portfolioRepository.GetByUserIdAsync(userId);

            // todo: null check for existence?
            _logger.LogInformation("Retrieved {Count} portfolios for user {UserId}",
                portfolios.Count(), userId);

            return portfolios.Select(MapPortfolioToDto);
        }

        /// <summary>
        /// Get a specific portfolio by ID.
        /// Authorization: Ensures portfolio belongs to the user.
        /// </summary>
        public async Task<PortfolioDto?> GetPortfolioByIdAsync(Guid portfolioId, Guid userId)
        {
            _logger.LogInformation("Retrieving portfolio {PortfolioId} for user {UserId}", portfolioId, userId);

            // This method ensures authorization at data layer
            var portfolio = await _portfolioRepository.GetByIdAndUserIdAsync(portfolioId, userId);

            if (portfolio == null)
            {
                _logger.LogWarning("Portfolio {PortfolioId} not found or unauthorized for user {UserId}",
                    portfolioId, userId);
                return null;
            }

            return MapPortfolioToDto(portfolio);
        }

        /// <summary>
        /// Get the user's default portfolio.
        /// </summary>
        public async Task<PortfolioDto?> GetDefaultPortfolioAsync(Guid userId)
        {
            _logger.LogInformation("Retrieving default portfolio for user: {UserId}", userId);

            var portfolio = await _portfolioRepository.GetDefaultPortfolioAsync(userId);

            if (portfolio == null)
            {
                _logger.LogInformation("No default portfolio found for user {UserId}", userId);
                return null;
            }

            return MapPortfolioToDto(portfolio);
        }

        /// <summary>
        /// Create a new portfolio for a user.
        /// </summary>
        public async Task<PortfolioDto> CreatePortfolioAsync(Guid userId, CreatePortfolioDto createPortfolioDto)
        {
            _logger.LogInformation("Creating portfolio '{Name}' for user {UserId}",
                createPortfolioDto.Name, userId);

            // Business Rule 1: Verify user exists
            var userExists = await _userRepository.GetByIdAsync(userId);
            if (userExists == null)
            {
                _logger.LogWarning("User {UserId} not found", userId);
                throw new InvalidOperationException($"User with ID {userId} not found");
            }

            // Business Rule 2: Check for duplicate portfolio name (per user)
            var nameExists = await _portfolioRepository.UserHasPortfolioWithNameAsync(
                userId, createPortfolioDto.Name);

            if (nameExists)
            {
                _logger.LogWarning("User {UserId} already has portfolio named '{Name}'",
                    userId, createPortfolioDto.Name);
                throw new InvalidOperationException(
                    $"You already have a portfolio named '{createPortfolioDto.Name}'");
            }

            // Business Rule 3: If this is being set as default, unset other defaults
            if (createPortfolioDto.IsDefault)
            {
                _logger.LogInformation("Setting new portfolio as default for user {UserId}", userId);
                // This will be handled after creation
            }

            // Create portfolio entity
            var portfolio = new Portfolio
            {
                UserId = userId,
                Name = createPortfolioDto.Name,
                Description = createPortfolioDto.Description,
                Currency = createPortfolioDto.Currency,
                IsDefault = createPortfolioDto.IsDefault,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Save to database
            await _portfolioRepository.AddAsync(portfolio);
            await _portfolioRepository.SaveChangesAsync();

            // If this is default, unset other defaults
            if (createPortfolioDto.IsDefault)
            {
                await _portfolioRepository.SetAsDefaultAsync(portfolio.Id, userId);
            }

            _logger.LogInformation("Created portfolio {PortfolioId} for user {UserId}",
                portfolio.Id, userId);

            return MapPortfolioToDto(portfolio);
        }

        /// <summary>
        /// Update portfolio details.
        /// Authorization: Ensures portfolio belongs to the user.
        /// </summary>
        public async Task<PortfolioDto?> UpdatePortfolioAsync(Guid portfolioId, Guid userId, UpdatePortfolioDto updatePortfolioDto)
        {
            _logger.LogInformation("Updating portfolio {PortfolioId} for user {UserId}", portfolioId, userId);

            // Authorization check
            var portfolio = await _portfolioRepository.GetByIdAndUserIdAsync(portfolioId, userId);

            if (portfolio == null)
            {
                _logger.LogWarning("Portfolio {PortfolioId} not found or unauthorized for user {UserId}",
                    portfolioId, userId);
                return null;
            }

            // Update name if provided
            if (!string.IsNullOrWhiteSpace(updatePortfolioDto.Name) &&
                updatePortfolioDto.Name != portfolio.Name)
            {
                // Check for duplicate name
                var nameExists = await _portfolioRepository.UserHasPortfolioWithNameAsync(
                    userId, updatePortfolioDto.Name, portfolioId);

                if (nameExists)
                {
                    _logger.LogWarning("User {UserId} already has another portfolio named '{Name}'",
                        userId, updatePortfolioDto.Name);
                    throw new InvalidOperationException(
                        $"You already have another portfolio named '{updatePortfolioDto.Name}'");
                }

                portfolio.Name = updatePortfolioDto.Name;
            }

            // Update description if provided
            if (updatePortfolioDto.Description != null)
            {
                portfolio.Description = updatePortfolioDto.Description;
            }

            // Update currency if provided
            if (!string.IsNullOrWhiteSpace(updatePortfolioDto.Currency))
            {
                portfolio.Currency = updatePortfolioDto.Currency;
            }

            // Update default status if provided
            if (updatePortfolioDto.IsDefault.HasValue)
            {
                if (updatePortfolioDto.IsDefault.Value && !portfolio.IsDefault)
                {
                    // Set as new default (will unset others)
                    await _portfolioRepository.SetAsDefaultAsync(portfolioId, userId);
                }
                else if (!updatePortfolioDto.IsDefault.Value && portfolio.IsDefault)
                {
                    // Unset as default
                    portfolio.IsDefault = false;
                }
            }

            // Save changes
            await _portfolioRepository.UpdateAsync(portfolio);
            await _portfolioRepository.SaveChangesAsync();

            _logger.LogInformation("Updated portfolio {PortfolioId}", portfolioId);

            return MapPortfolioToDto(portfolio);
        }

        /// <summary>
        /// Delete a portfolio.
        /// Authorization: Ensures portfolio belongs to the user.
        /// </summary>
        public async Task<bool> DeletePortfolioAsync(Guid portfolioId, Guid userId)
        {
            _logger.LogInformation("Deleting portfolio {PortfolioId} for user {UserId}", portfolioId, userId);

            // Authorization check
            var portfolio = await _portfolioRepository.GetByIdAndUserIdAsync(portfolioId, userId);

            if (portfolio == null)
            {
                _logger.LogWarning("Portfolio {PortfolioId} not found or unauthorized for user {UserId}",
                    portfolioId, userId);
                return false;
            }

            // Delete portfolio (cascade will delete holdings, transactions, etc.)
            await _portfolioRepository.DeleteAsync(portfolio);
            await _portfolioRepository.SaveChangesAsync();

            _logger.LogInformation("Deleted portfolio {PortfolioId}", portfolioId);

            return true;
        }

        /// <summary>
        /// Set a portfolio as the user's default.
        /// Authorization: Ensures portfolio belongs to the user.
        /// </summary>
        public async Task<bool> SetAsDefaultAsync(Guid portfolioId, Guid userId)
        {
            _logger.LogInformation("Setting portfolio {PortfolioId} as default for user {UserId}",
                portfolioId, userId);

            // Authorization check
            var portfolio = await _portfolioRepository.GetByIdAndUserIdAsync(portfolioId, userId);

            if (portfolio == null)
            {
                _logger.LogWarning("Portfolio {PortfolioId} not found or unauthorized for user {UserId}",
                    portfolioId, userId);
                return false;
            }

            // Set as default (will unset others)
            await _portfolioRepository.SetAsDefaultAsync(portfolioId, userId);

            _logger.LogInformation("Set portfolio {PortfolioId} as default", portfolioId);

            return true;
        }

        /// <summary>
        /// Maps a Portfolio entity to PortfolioDto.
        /// </summary>
        private static PortfolioDto MapPortfolioToDto(Portfolio portfolio)
        {
            return new PortfolioDto
            {
                Id = portfolio.Id,
                UserId = portfolio.UserId,
                Name = portfolio.Name,
                Description = portfolio.Description,
                Currency = portfolio.Currency,
                IsDefault = portfolio.IsDefault,
                CreatedAt = portfolio.CreatedAt,
                UpdatedAt = portfolio.UpdatedAt,
                HoldingsCount = 0 // Will be populated later when we have Holdings
            };
        }
    }
}

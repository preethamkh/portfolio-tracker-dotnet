using Microsoft.Extensions.Logging;
using PortfolioTracker.Core.DTOs.Auth;
using PortfolioTracker.Core.Entities;
using PortfolioTracker.Core.Interfaces.Repositories;
using PortfolioTracker.Core.Interfaces.Services;

namespace PortfolioTracker.Core.Services;

/// <summary>
/// Service for handling user authentication (registration and login).
/// </summary>
public class AuthService(IUserRepository userRepository, IJwtTokenService jwtTokenService, ILogger<AuthService> logger)
    : IAuthService
{
    /// <summary>
    /// Registers a new user.
    /// </summary>
    /// <remarks>
    /// Registration Process:
    /// 1. Check if email already exists
    /// 2. Hash password with BCrypt
    /// 3. Create user entity
    /// 4. Save to database
    /// 5. Generate JWT token
    /// 6. Return token + user info
    /// </remarks>
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        logger.LogInformation("Attempting to register user with email: {Email}", request.Email);

        // Step 1: Check if email already exists
        var emailTaken = await userRepository.IsEmailTakenAsync(request.Email);
        if (emailTaken)
        {
            logger.LogWarning("Registration failed: Email {Email} already exists", request.Email);
            throw new InvalidOperationException($"User with email '{request.Email}' already exists");
        }

        // Step 2: Hash password with BCrypt
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Step 3: Create user entity
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = passwordHash,
            FullName = request.FullName,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Step 4: Save to database
        await userRepository.AddAsync(user);
        await userRepository.SaveChangesAsync();

        logger.LogInformation("User registered successfully: {UserId} ({Email})", user.Id, user.Email);

        // Step 5: Generate JWT token
        var token = jwtTokenService.GenerateToken(user);

        return new AuthResponse
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName
            }
        };
    }

    /// <summary>
    /// Authenticates a user and returns JWT token.
    /// </summary>
    /// <remarks>
    /// Login Process:
    /// 1. Find user by email
    /// 2. Verify password with BCrypt
    /// 3. Update last login timestamp
    /// 4. Generate JWT token
    /// 5. Return token + user info
    /// </remarks>
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        logger.LogInformation("Login attempt for email: {Email}", request.Email);

        // Step 1: Find user by email
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            logger.LogWarning("Login failed: User not found for email {Email}", request.Email);

            // Security: Don't reveal whether email exists or password is wrong
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Step 2: Verify password with BCrypt
        var passwordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
        if (!passwordValid)
        {
            logger.LogWarning("Login failed: Invalid password for email {Email}", request.Email);

            // Security: Same error message as above (don't reveal which was wrong)
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        // Step 3: Update last login timestamp
        user.LastLogin = DateTime.UtcNow;
        await userRepository.UpdateAsync(user);
        await userRepository.SaveChangesAsync();

        // Step 4: Generate JWT token
        var token = jwtTokenService.GenerateToken(user);

        // Step 5: Return response
        return new AuthResponse
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            User = new UserInfo
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName
            }
        };
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PortfolioTracker.Core.Entities;
using PortfolioTracker.Infrastructure.Data;

namespace PortfolioTracker.API.Controllers
{
    /// <summary>
    /// Controller for managing user accounts.
    /// This controller is now THIN - it only handles HTTP concerns.
    /// All business logic is in UserService.
    /// </summary>
    /// <remarks>
    /// Controller Responsibilities (Keep it THIN!):
    /// 1. HTTP request/response handling
    /// 2. Route mapping
    /// 3. Status code decisions
    /// 4. Input validation (basic)
    /// 5. Calling services
    /// 
    /// What Controllers Should NOT Do:
    /// DON'T DO : Business logic (that's in services)
    /// DON'T DO : Database access (that's in repositories/services)
    /// DON'T DO : Complex validation (that's in services/validators)
    /// DON'T DO : Data mapping (that's in services)
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        // todo: inject IUserService instead of using DbContext directly
        // todo: keep controller thin
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;

        /// <summary>
        /// Constructor with Dependency Injection
        /// </summary>
        /// <param name="context">Database context</param>
        /// <param name="logger">Logger for this controller</param>
        public UsersController(ApplicationDbContext context, ILogger<UsersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all users (for testing purposes - not for production)
        /// </summary>
        /// <returns>List of all users</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            _logger.LogInformation("Fetching all users from the database");

            var users = await _context.Users
                // read-only query for better performance
                .AsNoTracking()
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} users", users.Count);

            return Ok(users);
        }

        /// <summary>
        /// Get a specific user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details</returns>
        // Adding constraint (guid) to the route parameter
        [HttpGet("{id:guid}")]
        [ProducesResponseType((StatusCodes.Status200OK))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<User>> GetUser(Guid id)
        {            _logger.LogInformation("Getting user with ID: {UserId}", id);

            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                _logger.LogWarning("User with ID {UserId} not found", id);
                return NotFound(new { message = $"User with ID {id} not found" });
            }

            _logger.LogInformation("Retrieved user: {Email}", user.Email);
            return Ok(user);
        }

        //[HttpPost]
        //public async Task<ActionResult<User>> CreateUser([FromBody] CreateUser)
    }
}

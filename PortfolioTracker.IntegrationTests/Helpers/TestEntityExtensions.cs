namespace PortfolioTracker.IntegrationTests.Helpers
{
    /// <summary>
    /// Extension class for entity-specific helper methods for tests.
    /// Provies Fluent APIs for customizing test entities.
    /// </summary>
    /// <remarks>
    /// Allows for more readable and maintainable test code.
    /// 
    /// var user = TestDataBuilder.CreateUser(context)
    ///     .WithEmail("custom@test.com")
    ///     .WithName("Custom Name");
    ///
    /// Currently not implemented but shows extension pattern for future.
    /// </remarks>
    public static class TestEntityExtensions
    {
        // Future: Add fluent builder methods
        // Example:
        // public static User WithEmail(this User user, string email)
        // {
        //     user.Email = email;
        //     return user;
        // }

        // Example usage: extension method
        //var user = new User();
        //user.WithEmail("custom@test.com");
    }
}

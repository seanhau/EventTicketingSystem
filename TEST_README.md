# Event Ticketing System - Test Suite

This document provides an overview of the comprehensive test suite for the Event Ticketing System.

## Test Projects

### 1. Application.Tests
Unit tests for the Application layer, including domain entities, commands, queries, and validators.

**Location:** `Application.Tests/`

**Test Coverage:**
- **Domain Tests** (`Domain/`)
  - `EventTests.cs` - Tests for Event entity
  - `PricingTierTests.cs` - Tests for PricingTier entity
  - `ActivityTests.cs` - Tests for Activity entity
  - `TicketPurchaseTests.cs` - Tests for TicketPurchase entity

- **Application Layer Tests** (`Application/`)
  - `Commands/CreateEventCommandTests.cs` - Tests for CreateEvent command handler
  - `Queries/GetEventDetailsQueryTests.cs` - Tests for GetEventDetails query handler
  - `Validators/CreateEventValidatorTests.cs` - Tests for CreateEvent validator

### 2. API.IntegrationTests
Integration tests for API endpoints using WebApplicationFactory.

**Location:** `API.IntegrationTests/`

**Test Coverage:**
- `EventsControllerIntegrationTests.cs` - Full CRUD operations for Events API
- `ActivitiesControllerIntegrationTests.cs` - Full CRUD operations for Activities API
- `CustomWebApplicationFactory.cs` - Test server configuration with in-memory database

## Running the Tests

### Run All Tests
```bash
dotnet test
```

### Run Specific Test Project
```bash
# Unit tests only
dotnet test Application.Tests/Application.Tests.csproj

# Integration tests only
dotnet test API.IntegrationTests/API.IntegrationTests.csproj
```

### Run Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Run Tests in Watch Mode
```bash
dotnet watch test --project Application.Tests/Application.Tests.csproj
```

## Test Frameworks and Libraries

- **xUnit** - Testing framework
- **FluentAssertions** - Assertion library for more readable tests
- **Moq** - Mocking framework (for unit tests)
- **Microsoft.EntityFrameworkCore.InMemory** - In-memory database for testing
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing support

## Test Structure

### Unit Tests
Unit tests follow the **Arrange-Act-Assert (AAA)** pattern:

```csharp
[Fact]
public void Method_Should_ExpectedBehavior_When_Condition()
{
    // Arrange - Set up test data and dependencies
    var entity = new Event { ... };

    // Act - Execute the method being tested
    var result = entity.AvailableTickets;

    // Assert - Verify the expected outcome
    result.Should().Be(100);
}
```

### Integration Tests
Integration tests use `WebApplicationFactory` to create a test server:

```csharp
public class EventsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EventsControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateEvent_ShouldReturnCreated_WhenValidData()
    {
        // Test implementation
    }
}
```

## Key Test Scenarios

### Domain Entity Tests
- Default value initialization
- Property setters and getters
- Computed properties (e.g., AvailableTickets)
- Business logic validation

### Command Handler Tests
- Successful command execution
- Validation failures
- Business rule violations
- Database persistence

### Query Handler Tests
- Successful data retrieval
- Not found scenarios
- Data mapping and transformation
- Include related entities

### Validator Tests
- Required field validation
- Length constraints
- Range validation
- Custom business rules
- Multiple validation errors

### Integration Tests
- HTTP status codes
- Request/response serialization
- End-to-end workflows
- CRUD operations
- Error handling

## Test Data Management

### Unit Tests
- Use in-memory database with unique database names per test
- Create test data within each test method
- No shared state between tests

### Integration Tests
- Use `CustomWebApplicationFactory` to configure test server
- In-memory database is recreated for each test class
- Tests are isolated and can run in parallel

## Best Practices

1. **Test Naming Convention**
   - Format: `Method_Should_ExpectedBehavior_When_Condition`
   - Example: `CreateEvent_ShouldReturnCreated_WhenValidData`

2. **One Assert Per Test**
   - Focus on testing one specific behavior
   - Use multiple tests for different scenarios

3. **Test Independence**
   - Each test should be able to run independently
   - No shared state between tests
   - Use unique database names for isolation

4. **Readable Assertions**
   - Use FluentAssertions for clear, readable assertions
   - Example: `result.Should().Be(expected)`

5. **Arrange-Act-Assert Pattern**
   - Clearly separate test phases
   - Makes tests easier to read and maintain

## Continuous Integration

These tests are designed to run in CI/CD pipelines:

```yaml
# Example GitHub Actions workflow
- name: Run Tests
  run: dotnet test --no-build --verbosity normal
```

## Test Coverage Goals

- **Domain Entities:** 100% coverage
- **Command Handlers:** 90%+ coverage
- **Query Handlers:** 90%+ coverage
- **Validators:** 100% coverage
- **API Controllers:** 80%+ coverage (via integration tests)

## Adding New Tests

When adding new features:

1. **Write unit tests first** for domain entities and business logic
2. **Add command/query handler tests** for application layer
3. **Create validator tests** for input validation
4. **Write integration tests** for API endpoints
5. **Run all tests** to ensure no regressions

## Troubleshooting

### Common Issues

**Issue:** Tests fail with database conflicts
- **Solution:** Ensure each test uses a unique database name

**Issue:** Integration tests fail to start
- **Solution:** Verify `Program` class is marked as `public partial class Program { }`

**Issue:** Flaky tests
- **Solution:** Check for shared state, use proper async/await, ensure test isolation

## Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [ASP.NET Core Integration Tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)

---

**Made with Bob**
# .NET tests

## Stack

- Write tests with xUnit v3 (`[Fact]`, `[Theory]`), assertions with FluentAssertions, and test doubles with NSubstitute. Do not add another test framework, assertion library, or mocking library.
- Put isolated handler and domain tests in `tests/DMAuth.Tests.Unit`.
- Put persistence and HTTP-pipeline tests in `tests/DMAuth.Tests.Integration`: exercise the persistence layer through the EF Core in-memory provider, and the full request pipeline through `IntegrationTestFactory` (`WebApplicationFactory<Program>`).

## Naming

- Name every test method `MethodName_Scenario_ExpectedResult`, e.g. `Handle_WithValidCommand_ReturnsSuccessWithUserId`, `Handle_WithDuplicateEmail_ReturnsConflictError`, `Validate_WithShortPassword_ReturnsViolation`.
- Name the test class `<SourceType>Tests`, e.g. `RegisterUserCommandHandlerTests`.

## Organization

- Mirror the source structure in unit tests: one test file per source file, grouped in folders by layer.
- Group integration test classes in folders by feature; each class receives the shared `IntegrationTestFactory` test server fixture.

## Structure

- Lay out every test body as Arrange, Act, Assert, each section introduced by a `// Arrange`, `// Act`, or `// Assert` comment line, in that order.
- Arrange sets up inputs, substitutes, and expected values; Act invokes the unit under test; Assert verifies the result.
- Pass `CancellationToken.None` when the unit under test takes a cancellation token.

## Test class setup and isolation

- Create every substitute and the system under test in the test class constructor, assigned to `private readonly` fields; never in a static field.
- Share no state between test classes. Each integration test class gets its own database instance.
- Tests within one class run sequentially; share a fixture between tests only within one class.
- Each test seeds the data it needs. Never rely on data left by another test.

## Assertions

- Use FluentAssertions for every assertion. Do not use xUnit `Assert.*`.
- Chain `.Should()` directly off the subject under assertion: `result.IsSuccess.Should().BeTrue()`, `result.Value.Should().Be(expectedId)`.
- Assert a `Result` failure on its flag and its kind: `result.IsSuccess.Should().BeFalse()` and `result.ErrorType.Should().Be(ResultError.Conflict)`; assert error text with `result.Error.Should().Contain(...)`.
- Assert collections with FluentAssertions collection assertions: `users.Should().HaveCount(3)`, `users.Should().ContainSingle(user => user.Email == "test@example.com")`.

## Substitutes (NSubstitute)

- Create each dependency's substitute with `Substitute.For<TInterface>()`.
- Match arguments the test does not care about with `Arg.Any<T>()`, including `Arg.Any<CancellationToken>()`.
- Configure a return with `.Returns(value)`; configure a null return with an explicit nullable cast, `.Returns((User?)null)`.
- Verify an expected call with `.Received(n)` (with `await` when the member is async); verify an absent call with `.DidNotReceive()`.

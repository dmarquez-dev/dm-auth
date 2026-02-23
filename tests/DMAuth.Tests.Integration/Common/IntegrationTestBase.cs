namespace DMAuth.Tests.Integration.Common;

/// <summary>
///		Base class for all integration tests providing shared test infrastructure.
/// </summary>
public abstract class IntegrationTestBase
{
	/// <summary>
	///		The cancellation token for the currently executing test, sourced from
	///		<see cref="TestContext.Current"/>. Pass this to all async calls in test methods
	///		to allow the test framework to cancel the operation if needed.
	/// </summary>
	protected CancellationToken TestCancellationToken =>
		TestContext.Current.CancellationToken;
}

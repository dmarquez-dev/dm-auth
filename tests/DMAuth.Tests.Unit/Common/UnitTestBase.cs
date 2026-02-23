namespace DMAuth.Tests.Unit.Common;

/// <summary>
///		Base class for all unit tests providing shared test infrastructure.
/// </summary>
public abstract class UnitTestBase
{
	/// <summary>
	///		The cancellation token for the currently executing test, sourced from
	///		<see cref="TestContext.Current"/>. Pass this to all async calls in test methods
	///		to allow the test framework to cancel the operation if needed.
	/// </summary>
	protected CancellationToken TestCancellationToken =>
		TestContext.Current.CancellationToken;
}

namespace DMAuth.Application.Common.Results.Extensions;

/// <summary>
///		Extension methods for inspecting <see cref="Result"/> outcomes.
/// </summary>
public static class ResultExtensions
{
	/// <summary>
	///		Returns true when the result represents a not-found failure.
	/// </summary>
	/// <param name="result">
	///		The result to inspect.
	/// </param>
	public static bool IsNotFound(this Result result) =>
		result.ErrorType is ResultError.NotFound;

	/// <summary>
	///		Returns true when the result represents a conflict failure.
	/// </summary>
	/// <param name="result">
	///		The result to inspect.
	/// </param>
	public static bool IsConflict(this Result result) =>
		result.ErrorType is ResultError.Conflict;

	/// <summary>
	///		Returns true when the result represents an unauthorized failure.
	/// </summary>
	/// <param name="result">
	///		The result to inspect.
	/// </param>
	public static bool IsUnauthorized(this Result result) =>
		result.ErrorType is ResultError.Unauthorized;

	/// <summary>
	///		Returns true when the result represents a forbidden failure.
	/// </summary>
	/// <param name="result">
	///		The result to inspect.
	/// </param>
	public static bool IsForbidden(this Result result) =>
		result.ErrorType is ResultError.Forbidden;

	/// <summary>
	///		Returns true when the result represents a general business rule violation.
	/// </summary>
	/// <param name="result">
	///		The result to inspect.
	/// </param>
	public static bool IsInvalid(this Result result) =>
		result.ErrorType is ResultError.Invalid;
}

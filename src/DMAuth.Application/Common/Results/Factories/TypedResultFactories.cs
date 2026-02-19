namespace DMAuth.Application.Common.Results;

public sealed partial class TypedResult<T>
{
	/// <summary>
	///		Returns a successful result carrying the given value.
	/// </summary>
	/// <param name="value">
	///		The value produced by the operation.
	/// </param>
	public static TypedResult<T> Success(
		T value) =>
			new(
				value);

	/// <summary>
	///		Returns a failure result with the specified error category.
	/// </summary>
	/// <param name="error">
	///		Description of the failure.
	/// </param>
	/// <param name="errorType">
	///		The category of failure.
	/// </param>
	public static new TypedResult<T> Failure(
		string error,
		ResultError errorType) =>
			new(
				error,
				errorType);

	/// <summary>
	///		Returns a failure result indicating the requested resource does not exist.
	/// </summary>
	/// <param name="error">
	///		Description of what was not found.
	/// </param>
	public static new TypedResult<T> NotFound(
		string error) =>
			Failure(
				error,
				ResultError.NotFound);

	/// <summary>
	///		Returns a failure result indicating a conflict with existing data.
	/// </summary>
	/// <param name="error">
	///		Description of the conflict.
	/// </param>
	public static new TypedResult<T> Conflict(
		string error) =>
			Failure(
				error,
				ResultError.Conflict);

	/// <summary>
	///		Returns a failure result indicating the caller is not authenticated.
	/// </summary>
	/// <param name="error">
	///		Description of the authentication requirement.
	/// </param>
	public static new TypedResult<T> Unauthorized(
		string error) =>
			Failure(
				error,
				ResultError.Unauthorized);

	/// <summary>
	///		Returns a failure result indicating the caller lacks the required permission.
	/// </summary>
	/// <param name="error">
	///		Description of the permission requirement.
	/// </param>
	public static new TypedResult<T> Forbidden(
		string error) =>
			Failure(
				error,
				ResultError.Forbidden);

	/// <summary>
	///		Returns a failure result indicating a general business rule violation.
	/// </summary>
	/// <param name="error">
	///		Description of the violated rule.
	/// </param>
	public static new TypedResult<T> Invalid(
		string error) =>
			Failure(
				error,
				ResultError.Invalid);
}

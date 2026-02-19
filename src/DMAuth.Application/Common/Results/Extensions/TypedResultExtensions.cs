namespace DMAuth.Application.Common.Results.Extensions;

/// <summary>
///		Extension methods for inspecting and unwrapping <see cref="TypedResult{T}"/> outcomes.
/// </summary>
public static class TypedResultExtensions
{
	/// <summary>
	///		Returns the result value on success, or <paramref name="defaultValue"/> on failure.
	/// </summary>
	/// <typeparam name="T">
	///		The type of the result value.
	/// </typeparam>
	/// <param name="result">
	///		The result to unwrap.
	/// </param>
	/// <param name="defaultValue">
	///		The value to return when the result is a failure.
	///		Defaults to the type default when not specified.
	/// </param>
	public static T? GetValueOrDefault<T>(
		this TypedResult<T> result,
		T? defaultValue = default) =>
			result.IsSuccess ? result.Value : defaultValue;
}

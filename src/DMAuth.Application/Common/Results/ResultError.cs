namespace DMAuth.Application.Common.Results;

/// <summary>
///		Categorizes the reason an operation failed.
/// </summary>
public enum ResultError
{
	/// <summary>
	///		No error; the operation succeeded.
	/// </summary>
	None,

	/// <summary>
	///		The requested resource does not exist.
	/// </summary>
	NotFound,

	/// <summary>
	///		The operation conflicts with existing data (e.g. a duplicate email address).
	/// </summary>
	Conflict,

	/// <summary>
	///		The caller is not authenticated.
	/// </summary>
	Unauthorized,

	/// <summary>
	///		The caller is authenticated but lacks the required permission.
	/// </summary>
	Forbidden,

	/// <summary>
	///		A general business rule violation that does not fit a more specific error category.
	/// </summary>
	Invalid
}

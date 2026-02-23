namespace DMAuth.Web.Common.CurrentUser;

/// <summary>
///		Provides access to the identity of the currently authenticated user
///		within the scope of an HTTP request.
/// </summary>
public interface ICurrentUserService
{
	/// <summary>
	///		Whether the current request is associated with an authenticated user.
	/// </summary>
	public bool IsAuthenticated { get; }

	/// <summary>
	///		The authenticated user's ID.
	/// </summary>
	/// <exception cref="InvalidOperationException">
	///		Thrown when the <c>NameIdentifier</c> claim is absent or invalid.
	///		This service must only be used within an authenticated request context.
	/// </exception>
	public Guid UserId { get; }
}

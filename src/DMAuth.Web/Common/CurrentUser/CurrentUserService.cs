using System.Security.Claims;

namespace DMAuth.Web.Common.CurrentUser;

/// <summary>
///		Resolves the current user's identity from the active HTTP request's
///		cookie authentication claims.
/// </summary>
public sealed class CurrentUserService(
	IHttpContextAccessor httpContextAccessor)
		: ICurrentUserService
{
	/// <inheritdoc />
	public bool IsAuthenticated =>
		httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;

	/// <inheritdoc />
	public Guid UserId
	{
		get
		{
			var value = httpContextAccessor.HttpContext?.User
				.FindFirstValue(ClaimTypes.NameIdentifier);

			if (!Guid.TryParse(value, out var id))
			{
				throw new InvalidOperationException(
					"NameIdentifier claim is missing or invalid. " +
					"ICurrentUserService must only be used within an authenticated request context.");
			}

			return id;
		}
	}
}

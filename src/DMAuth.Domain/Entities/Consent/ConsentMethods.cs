namespace DMAuth.Domain.Entities.Consent;

public partial class Consent
{
	/// <summary>
	///		Updates the granted scopes and records the time of the update.
	/// </summary>
	/// <param name="grantedScopes">
	///		The new space-delimited scopes to grant.
	/// </param>
	public void UpdateGrantedScopes(string grantedScopes)
	{
		GrantedScopes = grantedScopes;
		GrantedAt = DateTimeOffset.UtcNow;
	}
}

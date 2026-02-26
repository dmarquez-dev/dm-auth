using DMAuth.Domain.Exceptions;
using DMAuth.Domain.ValueObjects;

namespace DMAuth.Domain.Entities.Client;

public partial class Client
{
	/// <summary>
	///		Updates the client's name, redirect URIs, and allowed scopes.
	/// </summary>
	/// <param name="clientName">
	///		The new human-readable name for the client.
	/// </param>
	/// <param name="redirectUris">
	///		The new set of allowed redirect URIs.
	/// </param>
	/// <param name="allowedScopes">
	///		The new set of allowed OAuth 2.0 scopes.
	/// </param>
	public void UpdateRegistration(
		string clientName,
		List<string> redirectUris,
		List<string> allowedScopes)
	{
		ClientName = clientName;
		_redirectUris.Clear();
		_redirectUris.AddRange(redirectUris);
		_allowedScopes.Clear();
		_allowedScopes.AddRange(allowedScopes);
		UpdatedAt = DateTimeOffset.UtcNow;
	}

	/// <summary>
	///		Deactivates the client, preventing it from initiating OAuth 2.0 flows.
	/// </summary>
	/// <exception cref="DomainException">
	///		Thrown when the client is already inactive.
	/// </exception>
	public void Deactivate()
	{
		DomainException.ThrowIf(
			!IsActive,
			"Client is already inactive.");

		IsActive = false;
		UpdatedAt = DateTimeOffset.UtcNow;
	}
}

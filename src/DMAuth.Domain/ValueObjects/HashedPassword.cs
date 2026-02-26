using DMAuth.Domain.Exceptions;

namespace DMAuth.Domain.ValueObjects;

/// <summary>
///		Represents a hashed password value, ensuring type safety to prevent accidental plaintext storage.
/// </summary>
public record HashedPassword
{
	/// <summary>
	///		The hashed password string.
	/// </summary>
	public string Value { get; }

	/// <summary>
	///		Creates a new hashed password value object.
	/// </summary>
	/// <param name="value">
	///		The hashed password string.
	/// </param>
	/// <exception cref="DomainException">
	///		Thrown when the hashed password is empty.
	/// </exception>
	public HashedPassword(string value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			throw new DomainException("Hashed password cannot be empty.");
		}

		Value = value;
	}
}

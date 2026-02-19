namespace DMAuth.Domain.Exceptions;

/// <summary>
///		Base exception for domain rule violations.
/// </summary>
/// <param name="message">
///		Description of the domain rule that was violated.
/// </param>
public class DomainException(
	string message)
		: Exception(message);

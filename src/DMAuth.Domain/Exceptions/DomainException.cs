using System.Diagnostics.CodeAnalysis;

namespace DMAuth.Domain.Exceptions;

/// <summary>
///		Base exception for domain rule violations.
/// </summary>
/// <param name="message">
///		Description of the domain rule that was violated.
/// </param>
public class DomainException(
	string message)
		: Exception(message)
{
	/// <summary>
	///		Throws a <see cref="DomainException"/> if <paramref name="condition"/> is <see langword="true"/>.
	/// </summary>
	/// <param name="condition">
	///		The condition to evaluate. The exception is thrown when this is <see langword="true"/>.
	/// </param>
	/// <param name="exceptionMessage">
	///		Description of the domain rule that was violated.
	/// </param>
	/// <exception cref="DomainException">
	///		Thrown when <paramref name="condition"/> is <see langword="true"/>.
	/// </exception>
	public static void ThrowIf(
		[DoesNotReturnIf(true)] bool condition,
		string exceptionMessage)
	{
		if (condition)
		{
			throw new DomainException(exceptionMessage);
		}
	}
}

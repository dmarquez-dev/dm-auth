using System.Diagnostics.CodeAnalysis;

namespace DMAuth.Application.Common.Results;

/// <summary>
///		Represents the outcome of an operation that returns a value on success.
///		Use <see cref="Result"/> for operations that do not return a value.
/// </summary>
/// <typeparam name="T">
///		The type of the value returned on success.
/// </typeparam>
public sealed partial class TypedResult<T>
	: Result
{
	/// <summary>
	///		Initializes a successful result carrying the given value.
	/// </summary>
	/// <param name="value">
	///		The value produced by the operation.
	/// </param>
	private TypedResult(T value)
		: base(true)
	{
		Value = value;
	}

	/// <summary>
	///		Initializes a failed result with the given error details.
	/// </summary>
	/// <param name="error">
	///		Description of the failure.
	/// </param>
	/// <param name="errorType">
	///		The category of failure.
	/// </param>
	private TypedResult(
		string error,
		ResultError errorType)
			: base(
				false,
				error,
				errorType)
	{
	}

	/// <summary>
	///		Whether the operation succeeded.
	///		When <see langword="true"/>, <see cref="Value"/> is guaranteed non-null.
	/// </summary>
	[MemberNotNullWhen(true, nameof(Value))]
	public new bool IsSuccess =>
		base.IsSuccess;

	/// <summary>
	///		The value returned on success, or default on failure.
	/// </summary>
	public T? Value { get; }
}

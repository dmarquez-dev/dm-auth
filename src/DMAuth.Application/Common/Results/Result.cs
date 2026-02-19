namespace DMAuth.Application.Common.Results;

/// <summary>
///		Represents the outcome of an operation that does not return a value.
///		Use <see cref="TypedResult{T}"/> for operations that return data on success.
/// </summary>
public partial class Result
{
	/// <summary>
	///		Initializes the result with a success or failure state.
	/// </summary>
	/// <param name="isSuccess">
	///		Whether the operation succeeded.
	/// </param>
	/// <param name="error">
	///		Description of the failure, or empty string on success.
	/// </param>
	/// <param name="errorType">
	///		The category of failure, or <see cref="ResultError.None"/> on success.
	/// </param>
	protected Result(
		bool isSuccess,
		string error = "",
		ResultError errorType = ResultError.None)
	{
		IsSuccess = isSuccess;
		Error = error;
		ErrorType = errorType;
	}

	/// <summary>
	///		Whether the operation succeeded.
	/// </summary>
	public bool IsSuccess { get; }

	/// <summary>
	///		Description of the failure, or empty string on success.
	/// </summary>
	public string Error { get; }

	/// <summary>
	///		The category of failure, or <see cref="ResultError.None"/> on success.
	/// </summary>
	public ResultError ErrorType { get; }
}

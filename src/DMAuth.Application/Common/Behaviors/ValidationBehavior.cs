using FluentValidation;
using MediatR;

namespace DMAuth.Application.Common.Behaviors;

/// <summary>
///		MediatR pipeline behavior that runs FluentValidation validators before
///		the request handler is invoked.
///		Throws a <see cref="ValidationException"/> if any validators report failures,
///		which is handled by <c>ExceptionHandlingMiddleware</c> and mapped to HTTP 400.
/// </summary>
/// <typeparam name="TRequest">
///		The type of the incoming MediatR request.
/// </typeparam>
/// <typeparam name="TResponse">
///		The type of the response returned by the handler.
/// </typeparam>
/// <param name="validators">
///		All registered <see cref="IValidator{T}"/> instances for <typeparamref name="TRequest"/>.
///		Injected by the DI container; empty when no validators are registered for the request.
/// </param>
public sealed class ValidationBehavior<TRequest, TResponse>(
	IEnumerable<IValidator<TRequest>> validators)
		: IPipelineBehavior<TRequest, TResponse>
			where TRequest : notnull
{
	/// <inheritdoc />
	public async Task<TResponse> Handle(
		TRequest request,
		RequestHandlerDelegate<TResponse> next,
		CancellationToken cancellationToken)
	{
		if (!validators.Any())
		{
			return await next(
				cancellationToken);
		}

		var context = new ValidationContext<TRequest>(
			request);

		var failures = validators
			.Select(validator =>
				validator.Validate(
					context))
			.SelectMany(result =>
				result.Errors)
			.Where(failure =>
				failure is not null)
			.ToList();

		if (failures.Count > 0)
		{
			throw new ValidationException(
				failures);
		}

		return await next(
			cancellationToken);
	}
}

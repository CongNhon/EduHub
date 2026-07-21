using FluentValidation;
using MediatR;

namespace EduHub.Application.Common.Behaviors;

/// <summary>
/// Ghi chú: ValidationBehavior đại diện cho pipeline kiểm tra FluentValidation trước handler trong hệ thống EduHub.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Ghi chú: Handle xử lý pipeline kiểm tra FluentValidation trước handler, gọi database/service cần thiết và trả kết quả.
    /// </summary>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);
        var failures = await Task.WhenAll(validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));
        var errors = failures.SelectMany(result => result.Errors).Where(error => error is not null).ToList();

        if (errors.Count > 0)
        {
            throw new ValidationException(errors);
        }

        return await next(cancellationToken);
    }
}

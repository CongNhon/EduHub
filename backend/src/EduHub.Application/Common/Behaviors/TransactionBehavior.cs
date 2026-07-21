using EduHub.Application.Interfaces.Data;
using EduHub.Application.Common.CQRS;
using EduHub.Application.Common.Models;
using MediatR;

namespace EduHub.Application.Common.Behaviors;

/// <summary>
/// Ghi chú: TransactionBehavior đại diện cho pipeline bọc command trong database transaction trong hệ thống EduHub.
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <summary>
    /// Ghi chú: Handle xử lý pipeline bọc command trong database transaction, gọi database/service cần thiết và trả kết quả.
    /// </summary>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICommand<TResponse>)
        {
            return await next(cancellationToken);
        }

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var response = await next(cancellationToken);

            if (response is Result { IsFailure: true })
            {
                await transaction.RollbackAsync(CancellationToken.None);
                return response;
            }

            await transaction.CommitAsync(cancellationToken);
            return response;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }
}

using MediatR;

namespace EduHub.Application.Common.CQRS;

/// <summary>
/// Ghi chú: ICommand là command để xử lý i.
/// </summary>
public interface ICommand<out TResponse> : IRequest<TResponse>;

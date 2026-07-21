using MediatR;

namespace EduHub.Application.Common.CQRS;

/// <summary>
/// Ghi chú: IQuery là query để đọc i.
/// </summary>
public interface IQuery<out TResponse> : IRequest<TResponse>;

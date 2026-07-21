namespace EduHub.Application.Interfaces.Data;

/// <summary>
/// Ghi chú: IUnitOfWork là interface lưu thay đổi và mở transaction cho một use case.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Ghi chú: BeginTransactionAsync mở transaction cho command cần nhiều thao tác ghi cùng lúc.
    /// </summary>
    Task<IUnitOfWorkTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ghi chú: SaveChangesAsync lưu thay đổi entity của use case xuống database.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Ghi chú: IUnitOfWorkTransaction là transaction abstraction của Application, không phụ thuộc EF Core.
/// </summary>
public interface IUnitOfWorkTransaction : IAsyncDisposable
{
    /// <summary>
    /// Ghi chú: CommitAsync xác nhận transaction của command thành công.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Ghi chú: RollbackAsync hủy transaction khi command lỗi hoặc trả Result failure.
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}

using EduHub.Application.Common.Models;
using EduHub.WebApi.Dtos.Common;

namespace EduHub.WebApi.Mappings;

/// <summary>
/// Ghi chú: CommonMappings chứa mapping dùng chung cho response phân trang của API.
/// </summary>
public static class CommonMappings
{
    /// <summary>
    /// Ghi chú: ToPagedResponse chuyển PagedResult của Application thành PagedResponse DTO của API.
    /// </summary>
    public static PagedResponse<TApi> ToPagedResponse<TApplication, TApi>(
        this PagedResult<TApplication> result,
        Func<TApplication, TApi> mapItem) =>
        new(
            result.Items.Select(mapItem).ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount,
            result.TotalPages);
}

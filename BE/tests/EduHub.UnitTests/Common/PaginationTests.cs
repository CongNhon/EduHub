using EduHub.Application.Common.Models;

namespace EduHub.UnitTests.Common;

public sealed class PaginationTests
{
    [Fact]
    public void Create_NormalizesSearchAndAllowsConfiguredSortField()
    {
        IReadOnlySet<string> allowedSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "fullName" };

        var result = PageRequest.Create(search: "  Nguyen An  ", sortBy: "FULLNAME", allowedSortFields: allowedSortFields);

        Assert.True(result.IsSuccess);
        Assert.Equal(PageRequest.DefaultPage, result.Value.Page);
        Assert.Equal(PageRequest.DefaultPageSize, result.Value.PageSize);
        Assert.Equal("Nguyen An", result.Value.Search);
        Assert.Equal("FULLNAME", result.Value.SortBy);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_RejectsInvalidPage(int page)
    {
        var result = PageRequest.Create(page: page);

        Assert.True(result.IsFailure);
        Assert.Equal("pagination.page_invalid", result.Error?.Code);
    }

    [Fact]
    public void Create_RejectsPageSizeAboveMaximum()
    {
        var result = PageRequest.Create(pageSize: PageRequest.MaxPageSize + 1);

        Assert.True(result.IsFailure);
        Assert.Equal("pagination.page_size_invalid", result.Error?.Code);
    }

    [Fact]
    public void Create_RejectsUnknownSortField()
    {
        IReadOnlySet<string> allowedSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "fullName" };

        var result = PageRequest.Create(sortBy: "createdAt", allowedSortFields: allowedSortFields);

        Assert.True(result.IsFailure);
        Assert.Equal("pagination.sort_invalid", result.Error?.Code);
    }

    [Fact]
    public void Create_RejectsSearchAboveMaximumLength()
    {
        var result = PageRequest.Create(search: new string('a', PageRequest.MaxSearchLength + 1));

        Assert.True(result.IsFailure);
        Assert.Equal("pagination.search_too_long", result.Error?.Code);
    }

    [Fact]
    public void PagedResult_CalculatesTotalPages()
    {
        var result = new PagedResult<int>([1], page: 2, pageSize: 20, totalCount: 21);

        Assert.Equal(2, result.TotalPages);
    }
}

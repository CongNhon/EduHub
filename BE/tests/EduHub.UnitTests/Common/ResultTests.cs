using EduHub.Application.Common.Models;

namespace EduHub.UnitTests.Common;

public sealed class ResultTests
{
    [Fact]
    public void Success_ExposesValueWithoutError()
    {
        var result = Result.Success("student-01");

        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
        Assert.Equal("student-01", result.Value);
    }

    [Fact]
    public void Failure_ExposesStableErrorAndNoValue()
    {
        var error = new Error("student.code_conflict", "Student code already exists.", ErrorType.Conflict);
        var result = Result.Failure<string>(error);

        Assert.True(result.IsFailure);
        Assert.Same(error, result.Error);
        Assert.Throws<InvalidOperationException>(() => _ = result.Value);
    }
}

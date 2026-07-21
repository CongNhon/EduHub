using EduHub.Application;
using EduHub.Domain.Common;
using EduHub.WebApi.Modules.Diagnostics;
using NetArchTest.Rules;

namespace EduHub.ArchitectureTests;

public sealed class LayerDependencyTests
{
    [Fact]
    public void DomainDoesNotDependOnApplicationInfrastructureOrWebApi()
    {
        var result = Types.InAssembly(typeof(BaseEntity).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("EduHub.Application", "EduHub.Infrastructure", "EduHub.WebApi")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void ApplicationDoesNotDependOnInfrastructureOrWebApi()
    {
        var result = Types.InAssembly(typeof(DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny("EduHub.Infrastructure", "EduHub.WebApi")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void CarterModulesDoNotDependOnApplicationDbContext()
    {
        var result = Types.InAssembly(typeof(PipelineModule).Assembly)
            .That()
            .ResideInNamespace("EduHub.WebApi.Modules")
            .ShouldNot()
            .HaveDependencyOn("EduHub.Infrastructure.Persistence.ApplicationDbContext")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}

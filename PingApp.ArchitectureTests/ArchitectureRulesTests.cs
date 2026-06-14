using FluentAssertions;
using NetArchTest.Rules;

namespace PingApp.ArchitectureTests;

public class ArchitectureRulesTests
{
    private const string DomainNamespace = "PingApp.Domain";
    private const string ApplicationNamespace = "PingApp.Application";
    private const string InfrastructureNamespace = "PingApp.Infrastructure";
    private const string WinFormsNamespace = "PingApp.WinForms";

    [Fact]
    public void Domain_ShouldNotHaveDependencyOnOtherProjects()
    {
        var result = Types.InAssembly(typeof(Domain.Common.Result).Assembly)
            .ShouldNot()
            .HaveDependencyOnAll(ApplicationNamespace, InfrastructureNamespace, WinFormsNamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Домен не должен зависеть от внешних слоев!");
    }

    [Fact]
    public void Handlers_Should_Be_Sealed_And_EndWith_Handler()
    {
        var result = Types.InAssembly(typeof(Application.Features.Users.Login).Assembly)
            .That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .Should()
            .BeSealed()
            .And()
            .HaveNameEndingWith("Handler")
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Все хэндлеры должны быть запечатаны и оканчиваться на 'Handler'.");
    }
}
using NetArchTest.Rules;

namespace ModernPaySystem.ArchitectureTests;

public class ModuleBoundaryTests
{
    [Fact]
    public void TransactionsDomain_ShouldNotDependOn_AspNetCoreOrEfCore()
    {
        var assembly = typeof(ModernPaySystem.Modules.Transactions.Domain.Marker).Assembly;

        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.AspNetCore", "Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(Environment.NewLine, result.FailingTypeNames));
    }

    [Fact]
    public void DiwanDomain_ShouldNotDependOn_AspNetCoreOrEfCore()
    {
        var assembly = typeof(ModernPaySystem.Modules.Diwan.Domain.Marker).Assembly;

        var result = Types
            .InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny("Microsoft.AspNetCore", "Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(Environment.NewLine, result.FailingTypeNames));
    }

    [Fact]
    public void Contracts_ShouldNotDependOn_Infrastructure()
    {
        var txContracts = typeof(ModernPaySystem.Modules.Transactions.Contracts.Marker).Assembly;
        var diwanContracts = typeof(ModernPaySystem.Modules.Diwan.Contracts.Marker).Assembly;

        var txResult = Types.InAssembly(txContracts)
            .ShouldNot().HaveDependencyOn("ModernPaySystem.Modules.Transactions.Infrastructure")
            .GetResult();

        var diwanResult = Types.InAssembly(diwanContracts)
            .ShouldNot().HaveDependencyOn("ModernPaySystem.Modules.Diwan.Infrastructure")
            .GetResult();

        Assert.True(txResult.IsSuccessful, string.Join(Environment.NewLine, txResult.FailingTypeNames));
        Assert.True(diwanResult.IsSuccessful, string.Join(Environment.NewLine, diwanResult.FailingTypeNames));
    }

    [Fact]
    public void Application_ShouldNotDependOn_OtherModuleInfrastructure()
    {
        var txApp = typeof(ModernPaySystem.Modules.Transactions.Application.Marker).Assembly;
        var diwanApp = typeof(ModernPaySystem.Modules.Diwan.Application.Marker).Assembly;

        var txResult = Types.InAssembly(txApp)
            .ShouldNot().HaveDependencyOn("ModernPaySystem.Modules.Diwan.Infrastructure")
            .GetResult();

        var diwanResult = Types.InAssembly(diwanApp)
            .ShouldNot().HaveDependencyOn("ModernPaySystem.Modules.Transactions.Infrastructure")
            .GetResult();

        Assert.True(txResult.IsSuccessful, string.Join(Environment.NewLine, txResult.FailingTypeNames));
        Assert.True(diwanResult.IsSuccessful, string.Join(Environment.NewLine, diwanResult.FailingTypeNames));
    }
}

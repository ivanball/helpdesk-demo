using System.Reflection;
using Helpdesk.SharedKernel;
using Helpdesk.Tickets.Application;
using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Domain;
using Helpdesk.Tickets.Infrastructure;
using Helpdesk.Tickets.Presentation;
using NetArchTest.Rules;

namespace Helpdesk.ArchitectureTests;

/// <summary>One test per rule in docs/architecture-rules.md, in the same order.</summary>
public class ArchitectureRulesTests
{
    private static readonly Assembly SharedKernel = typeof(Result).Assembly;
    private static readonly Assembly Domain = typeof(Ticket).Assembly;
    private static readonly Assembly Application = typeof(TicketsApplication).Assembly;
    private static readonly Assembly Infrastructure = typeof(TicketsDbContext).Assembly;
    private static readonly Assembly Presentation = typeof(TicketEndpoints).Assembly;

    private const string DomainNamespace = "Helpdesk.Tickets.Domain";
    private const string InfrastructureNamespace = "Helpdesk.Tickets.Infrastructure";
    private const string PresentationNamespace = "Helpdesk.Tickets.Presentation";

    [Fact]
    public void Tickets_Domain_may_depend_only_on_SharedKernel_and_System()
    {
        var result = Types.InAssembly(Domain)
            .That().ResideInNamespace(DomainNamespace)
            .Should().OnlyHaveDependenciesOn(DomainNamespace, "Helpdesk.SharedKernel", "System")
            .GetResult();

        AssertNoOffenders(result);
    }

    [Fact]
    public void Tickets_Domain_must_not_reference_EntityFrameworkCore_anywhere()
    {
        var result = Types.InAssembly(Domain)
            .ShouldNot().HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        AssertNoOffenders(result);

        // "Anywhere" also covers an assembly reference that no type uses yet.
        var efReferences = Domain.GetReferencedAssemblies()
            .Where(a => a.Name?.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) == true)
            .Select(a => a.Name)
            .ToList();
        Assert.True(efReferences.Count == 0,
            $"Tickets.Domain references: {string.Join(", ", efReferences)}");
    }

    [Fact]
    public void Tickets_Application_must_not_depend_on_Infrastructure_or_Presentation()
    {
        var result = Types.InAssembly(Application)
            .ShouldNot().HaveDependencyOnAny(InfrastructureNamespace, PresentationNamespace)
            .GetResult();

        AssertNoOffenders(result);
    }

    [Fact]
    public void Command_and_query_handlers_must_be_internal()
    {
        var handlers = Types.InAssembly(Application)
            .That().ImplementInterface(typeof(ICommandHandler<,>))
            .Or().ImplementInterface(typeof(IQueryHandler<,>));

        // Guard against a filter that silently matches nothing and passes vacuously.
        Assert.NotEmpty(handlers.GetTypes());

        var result = handlers.Should().NotBePublic().GetResult();

        AssertNoOffenders(result);
    }

    [Fact]
    public void Only_Tickets_types_may_depend_on_Tickets_Infrastructure()
    {
        // Every Helpdesk assembly except Host, the composition root exempted by the rule.
        var result = Types.InAssemblies([SharedKernel, Domain, Application, Infrastructure, Presentation])
            .That().DoNotResideInNamespace("Helpdesk.Tickets")
            .ShouldNot().HaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        AssertNoOffenders(result);
    }

    private static void AssertNoOffenders(TestResult result) =>
        Assert.True(result.IsSuccessful,
            $"Offending types: {string.Join(", ", result.FailingTypeNames ?? [])}");
}

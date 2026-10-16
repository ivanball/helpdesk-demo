using Helpdesk.SharedKernel;
using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Application.Tickets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Helpdesk.Tickets.Presentation;

public static class TicketEndpoints
{
    public sealed record CreateTicketRequest(string Title);

    public static IEndpointRouteBuilder MapTicketEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/tickets", async (
            CreateTicketRequest request,
            ICommandHandler<CreateTicket, Guid> handler,
            CancellationToken ct) =>
        {
            var result = await handler.Handle(new CreateTicket(request.Title), ct);
            return result.IsSuccess
                ? Results.Created($"/tickets/{result.Value}", new { id = result.Value })
                : ToFailure(result.Error);
        });

        return app;
    }

    private static IResult ToFailure(Error error) =>
        error.Code.EndsWith("NotFound", StringComparison.Ordinal)
            ? Results.NotFound(new { code = error.Code, message = error.Message })
            : Results.BadRequest(new { code = error.Code, message = error.Message });
}

using Helpdesk.Tickets.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Helpdesk.Tickets.Application;

public static class TicketsApplication
{
    public static IServiceCollection AddTicketsApplication(this IServiceCollection services)
    {
        // Slices register here as they are added, e.g.:
        // services.AddCommandHandler<CreateTicket, Guid, CreateTicketHandler>();
        // services.AddScoped<IValidator<CreateTicket>, CreateTicketValidator>();
        return services;
    }
}

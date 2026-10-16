using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Application.Tickets;
using Microsoft.Extensions.DependencyInjection;

namespace Helpdesk.Tickets.Application;

public static class TicketsApplication
{
    public static IServiceCollection AddTicketsApplication(this IServiceCollection services)
    {
        services.AddCommandHandler<CreateTicket, Guid, CreateTicketHandler>();
        services.AddScoped<IValidator<CreateTicket>, CreateTicketValidator>();
        return services;
    }
}

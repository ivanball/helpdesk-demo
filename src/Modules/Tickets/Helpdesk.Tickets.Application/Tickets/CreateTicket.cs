using Helpdesk.SharedKernel;
using Helpdesk.Tickets.Application.Abstractions;
using Helpdesk.Tickets.Domain;

namespace Helpdesk.Tickets.Application.Tickets;

public sealed record CreateTicket(string Title) : ICommand<Guid>;

public sealed class CreateTicketHandler(ITicketRepository tickets, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateTicket, Guid>
{
    public async Task<Result<Guid>> Handle(CreateTicket command, CancellationToken ct)
    {
        var created = Ticket.Create(command.Title);
        if (created.IsFailure)
            return Result.Failure<Guid>(created.Error);

        tickets.Add(created.Value);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success(created.Value.Id);
    }
}

internal sealed class CreateTicketValidator : IValidator<CreateTicket>
{
    public const int MaxTitleLength = 200;

    public static readonly Error EmptyTitle =
        new("Tickets.Validation.EmptyTitle", "Title must not be blank.");
    public static readonly Error TitleTooLong =
        new("Tickets.Validation.TitleTooLong", $"Title must be at most {MaxTitleLength} characters.");

    public IReadOnlyList<Error> Validate(CreateTicket instance)
    {
        if (string.IsNullOrWhiteSpace(instance.Title))
            return [EmptyTitle];

        // Measured after trimming, because Ticket.Create stores the trimmed title.
        if (instance.Title.Trim().Length > MaxTitleLength)
            return [TitleTooLong];

        return [];
    }
}

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace Helpdesk.ApiTests;

public sealed class CloseTicketTests(HelpdeskApiFactory factory) : IClassFixture<HelpdeskApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Closing_an_open_ticket_returns_204_and_the_ticket_reports_Closed()
    {
        var id = await CreateTicket("Printer on fire");

        var close = await _client.PostAsync($"/tickets/{id}/close", content: null);
        Assert.Equal(HttpStatusCode.NoContent, close.StatusCode);

        var get = await _client.GetAsync($"/tickets/{id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        var ticket = await get.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Closed", ticket.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Closing_a_ticket_twice_returns_400_with_AlreadyClosed()
    {
        var id = await CreateTicket("Monitor flickers");

        var first = await _client.PostAsync($"/tickets/{id}/close", content: null);
        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);

        var second = await _client.PostAsync($"/tickets/{id}/close", content: null);
        Assert.Equal(HttpStatusCode.BadRequest, second.StatusCode);
        var error = await second.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Tickets.AlreadyClosed", error.GetProperty("code").GetString());
    }

    private async Task<Guid> CreateTicket(string title)
    {
        var response = await _client.PostAsJsonAsync("/tickets", new { title });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        return body.GetProperty("id").GetGuid();
    }
}

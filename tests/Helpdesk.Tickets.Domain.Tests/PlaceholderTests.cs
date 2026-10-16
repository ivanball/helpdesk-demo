namespace Helpdesk.Tickets.Domain.Tests;

public class PlaceholderTests
{
    [Fact]
    public void ModuleInfo_Name_IsSet()
    {
        Assert.Equal("Tickets", Helpdesk.Tickets.Domain.ModuleInfo.Name);
    }
}

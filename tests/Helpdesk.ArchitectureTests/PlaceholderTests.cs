namespace Helpdesk.ArchitectureTests;

public class PlaceholderTests
{
    [Fact]
    public void ModuleInfo_Name_IsSet()
    {
        Assert.Equal("SharedKernel", Helpdesk.SharedKernel.ModuleInfo.Name);
    }
}

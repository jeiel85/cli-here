using CliHere.App.Services;

namespace CliHere.Tests;

public sealed class CliDefinitionServiceTests
{
    [Fact]
    public void GetAll_IncludesExpectedBuiltInCliIds()
    {
        CliDefinitionService service = new();
        var ids = service.GetAll().Select(x => x.Id).ToArray();

        Assert.Contains("gemini", ids);
        Assert.Contains("opencode", ids);
        Assert.Contains("claude", ids);
        Assert.Contains("codex", ids);
    }
}

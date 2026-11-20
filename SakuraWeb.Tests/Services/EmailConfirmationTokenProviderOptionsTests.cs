using SakuraWeb.Services;

namespace SakuraWeb.Tests.Services;

public sealed class EmailConfirmationTokenProviderOptionsTests
{
    [Fact]
    public void Constructor_UsesEmailConfirmationTokenSettings()
    {
        var options = new EmailConfirmationTokenProviderOptions();

        Assert.Equal("EmailDataProtectorTokenProvider", options.Name);
        Assert.Equal(TimeSpan.FromMinutes(30), options.TokenLifespan);
    }
}

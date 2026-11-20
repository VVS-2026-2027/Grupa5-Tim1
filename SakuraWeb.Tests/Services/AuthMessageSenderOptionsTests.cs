using SakuraWeb.Services;

namespace SakuraWeb.Tests.Services;

public sealed class AuthMessageSenderOptionsTests
{
    [Fact]
    public void Properties_StoreConfiguredEmailAndPassword()
    {
        var options = new AuthMessageSenderOptions
        {
            FromEmail = "sender@example.com",
            AppPassword = "application-password"
        };

        Assert.Equal("sender@example.com", options.FromEmail);
        Assert.Equal("application-password", options.AppPassword);
    }

    [Fact]
    public void Properties_DefaultToNull()
    {
        var options = new AuthMessageSenderOptions();

        Assert.Null(options.FromEmail);
        Assert.Null(options.AppPassword);
    }
}

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SakuraWeb.Services;

namespace SakuraWeb.Tests.Services;

public sealed class EmailSenderTests
{
    [Fact]
    public async Task SendEmailAsync_ThrowsWhenFromEmailIsMissing()
    {
        var sender = CreateSender(new AuthMessageSenderOptions
        {
            AppPassword = "password"
        });

        var exception = await Assert.ThrowsAsync<Exception>(
            () => sender.SendEmailAsync(
                "to@test.com",
                "subject",
                "body"));

        Assert.Equal("Null FromEmail", exception.Message);
    }

    [Fact]
    public async Task SendEmailAsync_ThrowsWhenAppPasswordIsMissing()
    {
        var sender = CreateSender(new AuthMessageSenderOptions
        {
            FromEmail = "from@test.com"
        });

        var exception = await Assert.ThrowsAsync<Exception>(
            () => sender.SendEmailAsync(
                "to@test.com",
                "subject",
                "body"));

        Assert.Equal("Null AppPassword", exception.Message);
    }

    [Theory]
    [InlineData("", "password", "Null FromEmail")]
    [InlineData("from@test.com", "", "Null AppPassword")]
    public async Task SendEmailAsync_WithExplicitCredentials_ThrowsWhenCredentialIsMissing(
        string fromEmail,
        string appPassword,
        string expectedMessage)
    {
        var sender = CreateSender(new AuthMessageSenderOptions());

        var exception = await Assert.ThrowsAsync<Exception>(
            () => sender.SendEmailAsync(
                "to@test.com",
                "subject",
                "body",
                fromEmail,
                appPassword));

        Assert.Equal(expectedMessage, exception.Message);
    }

    private static EmailSender CreateSender(AuthMessageSenderOptions options)
    {
        return new EmailSender(
            Options.Create(options),
            NullLogger<EmailSender>.Instance);
    }
}

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SakuraWeb.Services;

namespace SakuraWeb.Tests.Services;

public class EmailSenderTests
{
    [Fact]
    public async Task SendEmailAsync_ThrowsWhenFromEmailIsMissing()
    {
        var sender = new EmailSender(
            Options.Create(
                new AuthMessageSenderOptions
                {
                    AppPassword = "password"
                }),
            NullLogger<EmailSender>.Instance);

        var exception = await Assert.ThrowsAsync<Exception>(
            () => sender.SendEmailAsync(
                "to@test.com",
                "subject",
                "body"));

        Assert.Equal(
            "Null FromEmail",
            exception.Message);
    }

    [Fact]
    public async Task SendEmailAsync_ThrowsWhenPasswordIsMissing()
    {
        var sender = new EmailSender(
            Options.Create(
                new AuthMessageSenderOptions
                {
                    FromEmail = "from@test.com"
                }),
            NullLogger<EmailSender>.Instance);

        var exception = await Assert.ThrowsAsync<Exception>(
            () => sender.SendEmailAsync(
                "to@test.com",
                "subject",
                "body"));

        Assert.Equal(
            "Null AppPassword",
            exception.Message);
    }
}
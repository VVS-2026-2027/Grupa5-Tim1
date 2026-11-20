using System.Net;
using System.Net.Mail;
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
    public async Task SendEmailAsync_WithConfiguredCredentials_ValidatesRecipientBeforeSending()
    {
        var sender = CreateSender(new AuthMessageSenderOptions
        {
            FromEmail = "sender@example.com",
            AppPassword = "password"
        });

        await Assert.ThrowsAsync<FormatException>(
            () => sender.SendEmailAsync(
                "not-an-email",
                "subject",
                "body"));
    }

    [Fact]
    public async Task SendEmailAsync_UsesConfiguredCredentialsAndSendsMessage()
    {
        var sender = new TestEmailSender(
            new AuthMessageSenderOptions
            {
                FromEmail = "sender@example.com",
                AppPassword = "configured-password"
            },
            _ => Task.CompletedTask);

        await sender.SendEmailAsync(
            "recipient@example.com",
            "subject",
            "body");

        Assert.Equal("sender@example.com", sender.Credentials!.UserName);
        Assert.Equal("configured-password", sender.Credentials.Password);
        Assert.Equal("recipient@example.com", sender.SentMessage!.To.Single().Address);
        Assert.Equal(1, sender.SendCount);
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

    [Theory]
    [InlineData(null, null, "configured@example.com", "configured-password")]
    [InlineData("override@example.com", null, "override@example.com", "configured-password")]
    [InlineData(null, "override-password", "configured@example.com", "override-password")]
    [InlineData("override@example.com", "override-password", "override@example.com", "override-password")]
    public async Task SendEmailAsync_UsesFallbackOrExplicitCredentials(
        string? fromEmail,
        string? appPassword,
        string expectedFromEmail,
        string expectedPassword)
    {
        var sender = new TestEmailSender(
            new AuthMessageSenderOptions
            {
                FromEmail = "configured@example.com",
                AppPassword = "configured-password"
            },
            _ => Task.CompletedTask);

        await sender.SendEmailAsync(
            "recipient@example.com",
            "subject",
            "<p>body</p>",
            fromEmail,
            appPassword);

        Assert.Equal(expectedFromEmail, sender.Credentials!.UserName);
        Assert.Equal(expectedPassword, sender.Credentials.Password);
        Assert.Equal("subject", sender.SentMessage!.Subject);
        Assert.Equal("<p>body</p>", sender.SentMessage.Body);
        Assert.True(sender.SentMessage.IsBodyHtml);
        Assert.Equal("recipient@example.com", sender.SentMessage.To.Single().Address);
        Assert.Equal(1, sender.SendCount);
    }

    [Fact]
    public async Task Execute_MalformedRecipient_ThrowsFormatException()
    {
        var sender = CreateSender(new AuthMessageSenderOptions());

        await Assert.ThrowsAsync<FormatException>(
            () => sender.Execute(
                "sender@example.com",
                "password",
                "subject",
                "body",
                "not-an-email"));
    }

    [Fact]
    public async Task Execute_EmptyRecipient_ThrowsArgumentException()
    {
        var sender = CreateSender(new AuthMessageSenderOptions());

        await Assert.ThrowsAsync<ArgumentException>(
            () => sender.Execute(
                "sender@example.com",
                "password",
                "subject",
                "body",
                string.Empty));
    }

    [Fact]
    public async Task Execute_InvalidSenderAddress_ThrowsFormatException()
    {
        var sender = CreateSender(new AuthMessageSenderOptions());

        await Assert.ThrowsAsync<FormatException>(
            () => sender.Execute(
                "not-an-email",
                "password",
                "subject",
                "body",
                "recipient@example.com"));
    }

    [Fact]
    public async Task Execute_WhenSmtpSendSucceeds_CompletesSending()
    {
        var sender = new TestEmailSender(
            new AuthMessageSenderOptions(),
            _ => Task.CompletedTask);

        await sender.Execute(
            "sender@example.com",
            "password",
            "subject",
            "body",
            "recipient@example.com");

        Assert.Equal(1, sender.SendCount);
        Assert.Equal("sender@example.com", sender.SentMessage!.From!.Address);
        Assert.Equal("recipient@example.com", sender.SentMessage.To.Single().Address);
    }

    [Fact]
    public async Task Execute_WhenSmtpSendFails_HandlesFailure()
    {
        var sender = new TestEmailSender(
            new AuthMessageSenderOptions(),
            _ => Task.FromException(new SmtpException("SMTP unavailable")));

        var exception = await Record.ExceptionAsync(
            () => sender.Execute(
                "sender@example.com",
                "password",
                "subject",
                "body",
                "recipient@example.com"));

        Assert.Null(exception);
        Assert.Equal(1, sender.SendCount);
    }

    private static EmailSender CreateSender(AuthMessageSenderOptions options)
    {
        return new EmailSender(
            Options.Create(options),
            NullLogger<EmailSender>.Instance);
    }

    private sealed class TestEmailSender : EmailSender
    {
        private readonly Func<MailMessage, Task> _send;

        public TestEmailSender(
            AuthMessageSenderOptions options,
            Func<MailMessage, Task> send)
            : base(Options.Create(options), NullLogger<EmailSender>.Instance)
        {
            _send = send;
        }

        public MailMessage? SentMessage { get; private set; }
        public NetworkCredential? Credentials { get; private set; }
        public int SendCount { get; private set; }

        protected override Task SendMailAsync(SmtpClient smtpClient, MailMessage mailMessage)
        {
            SendCount++;
            SentMessage = mailMessage;
            Credentials = Assert.IsType<NetworkCredential>(smtpClient.Credentials);
            return _send(mailMessage);
        }
    }
}

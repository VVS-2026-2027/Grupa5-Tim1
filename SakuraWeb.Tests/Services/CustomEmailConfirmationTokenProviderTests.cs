using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SakuraWeb.Models;
using SakuraWeb.Services;

namespace SakuraWeb.Tests.Services;

public sealed class CustomEmailConfirmationTokenProviderTests
{
    [Fact]
    public void Constructor_CreatesDataProtectorTokenProviderForUserType()
    {
        IDataProtectionProvider dataProtectionProvider =
            DataProtectionProvider.Create("SakuraWeb.Tests");
        var options = Options.Create(new EmailConfirmationTokenProviderOptions());

        var provider = new CustomEmailConfirmationTokenProvider<Korisnik>(
            dataProtectionProvider,
            options,
            NullLogger<DataProtectorTokenProvider<Korisnik>>.Instance);

        Assert.IsAssignableFrom<DataProtectorTokenProvider<Korisnik>>(provider);
    }
}

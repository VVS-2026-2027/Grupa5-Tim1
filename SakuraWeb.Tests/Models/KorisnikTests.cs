using System.Reflection;
using SakuraWeb.Models;

namespace SakuraWeb.Tests.Models;

public class KorisnikTests
{
    public static IEnumerable<object[]> SubscriptionCases()
    {
        var path = Path.Combine(
            Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location)!,
            "TestData",
            "subscription-cases.csv");

        return File.ReadLines(path)
            .Skip(1)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => line.Split(','))
            .Select(values => new object[]
            {
                bool.Parse(values[0]),
                bool.Parse(values[1])
            });
    }

    [Theory]
    [MemberData(nameof(SubscriptionCases))]
    public void PromjeniStanjePretplate_ChangesSubscription(
        bool initial,
        bool expected)
    {
        var user = new Korisnik
        {
            jePretplacenNaNewsletter = initial
        };

        user.promjeniStanjePretplate(expected);

        Assert.Equal(
            expected,
            user.jePretplacenNaNewsletter);
    }

    [Fact]
    public void ParameterlessConstructor_SetsEmptyDefaults()
    {
        var user = new Korisnik();

        Assert.Equal(string.Empty, user.korisnickoIme);
        Assert.Equal(string.Empty, user.emailAdresa);
        Assert.Equal(string.Empty, user.lozinka);
        Assert.False(user.jePretplacenNaNewsletter);
    }

    [Theory]
    [InlineData("admin", "admin@test.com", "Lozinka1!", true, Uloga.Administrator)]
    [InlineData("klijent", "klijent@test.com", "Lozinka2!", false, Uloga.Klijent)]
    [InlineData("frizer", "frizer@test.com", "Lozinka3!", true, Uloga.Frizer)]
    public void ParameterizedConstructor_SetsAllProperties(
        string korisnickoIme,
        string emailAdresa,
        string lozinka,
        bool subscribed,
        Uloga role)
    {
        var user = new Korisnik(
            korisnickoIme,
            emailAdresa,
            lozinka,
            subscribed,
            role);

        Assert.Equal(korisnickoIme, user.korisnickoIme);
        Assert.Equal(emailAdresa, user.emailAdresa);
        Assert.Equal(lozinka, user.lozinka);
        Assert.Equal(subscribed, user.jePretplacenNaNewsletter);
        Assert.Equal(role, user.ulogaKorisnika);
    }

    [Theory]
    [InlineData("ime1", "ime1@test.com", Uloga.Frizer)]
    [InlineData("ime2", "ime2@test.com", Uloga.Klijent)]
    [InlineData("ime3", "ime3@test.com", Uloga.Administrator)]
    public void IdentityProperties_AssignedValues_AreRetained(
        string korisnickoIme,
        string emailAdresa,
        Uloga role)
    {
        var user = new Korisnik
        {
            Id = "user-id",
            korisnickoIme = korisnickoIme,
            emailAdresa = emailAdresa,
            lozinka = "password-hash",
            ulogaKorisnika = role,
            UserName = korisnickoIme,
            Email = emailAdresa
        };

        Assert.Equal("user-id", user.Id);
        Assert.Equal(korisnickoIme, user.korisnickoIme);
        Assert.Equal(korisnickoIme, user.UserName);
        Assert.Equal(emailAdresa, user.emailAdresa);
        Assert.Equal(emailAdresa, user.Email);
        Assert.Equal("password-hash", user.lozinka);
        Assert.Equal(role, user.ulogaKorisnika);
    }
}

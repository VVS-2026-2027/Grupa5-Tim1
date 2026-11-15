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
}
using SakuraWeb.Models;

namespace SakuraWeb.Tests.Models;

public sealed class ProizvodSastojakTests
{
    [Fact]
    public void DefaultConstructor_InitializesZeroIdsAndNullNavigations()
    {
        var link = new ProizvodSastojak();

        Assert.Equal(0, link.id);
        Assert.Equal(0, link.proizvodId);
        Assert.Equal(0, link.sastojakId);
        Assert.Null(link.proizvod);
        Assert.Null(link.sastojak);
    }

    [Theory]
    [MemberData(nameof(JsonCases))]
    public void Constructor_CopiesIdsAndNavigationReferences(ModelConstructionCase testCase)
    {
        var proizvod = new Proizvod { naziv = testCase.ProizvodNaziv };
        var sastojak = new Sastojak(
            4,
            testCase.SastojakNaziv,
            testCase.SastojakOpis,
            testCase.SastojakSlika)
        {
            naziv = testCase.SastojakNaziv,
            opis = testCase.SastojakOpis,
            URLSlike = testCase.SastojakSlika
        };

        var link = new ProizvodSastojak(15, 6, 4, proizvod, sastojak);

        Assert.Equal(15, link.id);
        Assert.Equal(6, link.proizvodId);
        Assert.Equal(4, link.sastojakId);
        Assert.Same(proizvod, link.proizvod);
        Assert.Same(sastojak, link.sastojak);
    }

    [Fact]
    public void PropertyAssignments_StoreIdsAndNavigationReferences()
    {
        var proizvod = new Proizvod { naziv = "Aloe gel" };
        var sastojak = new Sastojak
        {
            naziv = "Aloe vera",
            opis = "Hidratantni sastojak",
            URLSlike = "aloe.jpg"
        };
        var link = new ProizvodSastojak
        {
            id = 5,
            proizvodId = 6,
            sastojakId = 7,
            proizvod = proizvod,
            sastojak = sastojak
        };

        Assert.Equal(5, link.id);
        Assert.Equal(6, link.proizvodId);
        Assert.Equal(7, link.sastojakId);
        Assert.Same(proizvod, link.proizvod);
        Assert.Same(sastojak, link.sastojak);
    }

    public static IEnumerable<object[]> JsonCases() =>
        ModelTestData.JsonConstructionCases();
}

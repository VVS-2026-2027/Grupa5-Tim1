using SakuraWeb.Models;

namespace SakuraWeb.Tests.Models;

public sealed class SastojakTests
{
    [Fact]
    public void DefaultConstructor_InitializesDefaultsAndEmptyCollection()
    {
        var sastojak = new Sastojak
        {
            naziv = string.Empty,
            opis = string.Empty,
            URLSlike = string.Empty
        };

        Assert.Equal(-1, sastojak.id);
        Assert.Equal(string.Empty, sastojak.naziv);
        Assert.Equal(string.Empty, sastojak.opis);
        Assert.Equal(string.Empty, sastojak.URLSlike);
        Assert.Empty(sastojak.ProizvodSastojci);
    }

    [Theory]
    [MemberData(nameof(JsonCases))]
    public void Constructor_CopiesAllValues(ModelConstructionCase testCase)
    {
        var sastojak = new Sastojak(
            18,
            testCase.SastojakNaziv,
            testCase.SastojakOpis,
            testCase.SastojakSlika)
        {
            naziv = testCase.SastojakNaziv,
            opis = testCase.SastojakOpis,
            URLSlike = testCase.SastojakSlika
        };

        Assert.Equal(18, sastojak.id);
        Assert.Equal(testCase.SastojakNaziv, sastojak.naziv);
        Assert.Equal(testCase.SastojakOpis, sastojak.opis);
        Assert.Equal(testCase.SastojakSlika, sastojak.URLSlike);
    }

    [Theory]
    [MemberData(nameof(NazivCases))]
    public void DataAnnotations_ValidateNaziv(string value, bool expectedValid)
    {
        var sastojak = CreateValidIngredient();
        sastojak.naziv = value;

        Assert.Equal(expectedValid, ModelTestData.IsValid(sastojak));
    }

    [Theory]
    [MemberData(nameof(OpisCases))]
    public void DataAnnotations_ValidateOpis(string value, bool expectedValid)
    {
        var sastojak = CreateValidIngredient();
        sastojak.opis = value;

        Assert.Equal(expectedValid, ModelTestData.IsValid(sastojak));
    }

    [Fact]
    public void Collection_StoresProductIngredientLinks()
    {
        var sastojak = CreateValidIngredient();
        var link = new ProizvodSastojak();

        sastojak.ProizvodSastojci.Add(link);

        Assert.Contains(link, sastojak.ProizvodSastojci);
    }

    private static Sastojak CreateValidIngredient()
    {
        return new Sastojak
        {
            naziv = "Aloe vera",
            opis = "Hidratantni sastojak",
            URLSlike = "aloe.jpg"
        };
    }

    public static IEnumerable<object[]> JsonCases() =>
        ModelTestData.JsonConstructionCases();

    public static IEnumerable<object[]> NazivCases() =>
        ModelTestData.CsvValidationCases("NazivSastojka");

    public static IEnumerable<object[]> OpisCases() =>
        ModelTestData.CsvValidationCases("OpisSastojka");
}

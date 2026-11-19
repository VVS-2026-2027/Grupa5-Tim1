using SakuraWeb.Models;

namespace SakuraWeb.Tests.Models;

public sealed class PorukaTests
{
    [Fact]
    public void DefaultConstructor_InitializesEmptyValues()
    {
        var poruka = new Poruka();

        Assert.Equal(0, poruka.id);
        Assert.Equal(0, poruka.newsletterId);
        Assert.Null(poruka.newsletter);
        Assert.Equal(string.Empty, poruka.naziv);
        Assert.Equal(string.Empty, poruka.tekst);
    }

    [Theory]
    [MemberData(nameof(JsonCases))]
    public void Constructor_CopiesNameAndTextFromJson(ModelConstructionCase testCase)
    {
        var poruka = new Poruka(testCase.PorukaNaziv, testCase.PorukaTekst);

        Assert.Equal(testCase.PorukaNaziv, poruka.naziv);
        Assert.Equal(testCase.PorukaTekst, poruka.tekst);
    }

    [Theory]
    [InlineData("Obavijest", true)]
    [InlineData("", false)]
    public void DataAnnotations_ValidateNaziv(string naziv, bool expectedValid)
    {
        var poruka = new Poruka(naziv, "Tekst poruke");

        Assert.Equal(expectedValid, ModelTestData.IsValid(poruka));
    }

    [Theory]
    [MemberData(nameof(StoredValidationCases))]
    public void DataAnnotations_ValidateStoredNazivCases(string naziv, bool expectedValid)
    {
        var poruka = new Poruka(naziv, "Tekst poruke");

        Assert.Equal(expectedValid, ModelTestData.IsValid(poruka));
    }

    public static IEnumerable<object[]> JsonCases() =>
        ModelTestData.JsonConstructionCases();

    public static IEnumerable<object[]> StoredValidationCases() =>
        ModelTestData.CsvValidationCases("PorukaNaziv");
}

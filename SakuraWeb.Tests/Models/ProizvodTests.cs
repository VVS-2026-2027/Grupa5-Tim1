using System.Globalization;
using SakuraWeb.Models;

namespace SakuraWeb.Tests.Models;

public sealed class ProizvodTests
{
    [Fact]
    public void DefaultConstructor_InitializesDefaultsAndCollections()
    {
        var proizvod = new Proizvod { naziv = string.Empty };

        Assert.Equal(0, proizvod.id);
        Assert.Equal(string.Empty, proizvod.naziv);
        Assert.Equal(0, proizvod.cijena);
        Assert.Equal(default(KategorijaProizvoda), proizvod.kategorija);
        Assert.Equal(0, proizvod.volumen);
        Assert.Null(proizvod.slikaPutanja);
        Assert.Empty(proizvod.ProizvodSastojci);
        Assert.Empty(proizvod.ProizvodBenefiti);
        Assert.Equal(0, proizvod.poeniPr);
    }

    [Theory]
    [MemberData(nameof(JsonCases))]
    public void Constructor_CopiesProductValuesFromJson(ModelConstructionCase testCase)
    {
        var proizvod = new Proizvod(
            testCase.ProizvodNaziv,
            testCase.ProizvodCijena,
            KategorijaProizvoda.Ulje,
            testCase.ProizvodVolumen)
        {
            naziv = testCase.ProizvodNaziv
        };

        Assert.Equal(testCase.ProizvodNaziv, proizvod.naziv);
        Assert.Equal(testCase.ProizvodCijena, proizvod.cijena);
        Assert.Equal(KategorijaProizvoda.Ulje, proizvod.kategorija);
        Assert.Equal(testCase.ProizvodVolumen, proizvod.volumen);
    }

    [Theory]
    [MemberData(nameof(CijenaCases))]
    public void DataAnnotations_ValidateCijena(string value, bool expectedValid)
    {
        var proizvod = CreateValidProduct();
        proizvod.cijena = double.Parse(value, CultureInfo.InvariantCulture);

        Assert.Equal(expectedValid, ModelTestData.IsValid(proizvod));
    }

    [Theory]
    [MemberData(nameof(VolumenCases))]
    public void DataAnnotations_ValidateVolumen(string value, bool expectedValid)
    {
        var proizvod = CreateValidProduct();
        proizvod.volumen = double.Parse(value, CultureInfo.InvariantCulture);

        Assert.Equal(expectedValid, ModelTestData.IsValid(proizvod));
    }

    [Theory]
    [MemberData(nameof(PoeniCases))]
    public void DataAnnotations_ValidatePoeni(string value, bool expectedValid)
    {
        var proizvod = CreateValidProduct();
        proizvod.poeniPr = int.Parse(value, CultureInfo.InvariantCulture);

        Assert.Equal(expectedValid, ModelTestData.IsValid(proizvod));
    }

    [Fact]
    public void Collections_StoreIngredientAndBenefitLinks()
    {
        var proizvod = CreateValidProduct();
        var sastojak = new ProizvodSastojak();
        var benefit = new ProizvodBenefit();

        proizvod.ProizvodSastojci.Add(sastojak);
        proizvod.ProizvodBenefiti.Add(benefit);

        Assert.Contains(sastojak, proizvod.ProizvodSastojci);
        Assert.Contains(benefit, proizvod.ProizvodBenefiti);
    }

    private static Proizvod CreateValidProduct()
    {
        return new Proizvod
        {
            naziv = "Aloe gel",
            cijena = 18.5,
            volumen = 250,
            kategorija = KategorijaProizvoda.Ulje,
            poeniPr = 5
        };
    }

    public static IEnumerable<object[]> JsonCases() =>
        ModelTestData.JsonConstructionCases();

    public static IEnumerable<object[]> CijenaCases() =>
        ModelTestData.CsvValidationCases("CijenaProizvoda");

    public static IEnumerable<object[]> VolumenCases() =>
        ModelTestData.CsvValidationCases("VolumenProizvoda");

    public static IEnumerable<object[]> PoeniCases() =>
        ModelTestData.CsvValidationCases("PoeniProizvoda");
}

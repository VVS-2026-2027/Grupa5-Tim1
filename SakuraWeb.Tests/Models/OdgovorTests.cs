using System.ComponentModel.DataAnnotations;
using SakuraWeb.Models;

namespace SakuraWeb.Tests.Models;

public class OdgovorTests
{
    [Fact]
    public void ParameterlessConstructor_SetsEmptyContentAndZeroPoints()
    {
        var odgovor = new Odgovor();

        Assert.Equal(string.Empty, odgovor.sadrzaj);
        Assert.Equal(0, odgovor.poeni);
    }

    [Theory]
    [InlineData("Nikad", 0)]
    [InlineData("Ponekad", 1)]
    [InlineData("Uvijek", 3)]
    public void ParameterizedConstructor_SetsContentAndPoints(
        string sadrzaj,
        int poeni)
    {
        var odgovor = new Odgovor(sadrzaj, poeni);

        Assert.Equal(sadrzaj, odgovor.sadrzaj);
        Assert.Equal(poeni, odgovor.poeni);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public void PoeniProperty_ValueInRange_IsValid(int poeni)
    {
        var odgovor = new Odgovor("Validan odgovor", poeni);

        var results = Validate(odgovor);

        Assert.Empty(results);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void PoeniProperty_ValueOutsideRange_IsInvalid(int poeni)
    {
        var odgovor = new Odgovor("Nevalidan odgovor", poeni);

        var results = Validate(odgovor);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(Odgovor.poeni)));
    }

    [Fact]
    public void SadrzajProperty_EmptyValue_IsInvalid()
    {
        var odgovor = new Odgovor
        {
            sadrzaj = string.Empty,
            poeni = 1
        };

        var results = Validate(odgovor);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(Odgovor.sadrzaj)));
    }

    private static IReadOnlyCollection<ValidationResult> Validate(Odgovor odgovor)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(
            odgovor,
            new ValidationContext(odgovor),
            results,
            validateAllProperties: true);

        return results;
    }
}

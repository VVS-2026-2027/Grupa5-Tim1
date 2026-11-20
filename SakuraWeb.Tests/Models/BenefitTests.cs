using System.ComponentModel.DataAnnotations;
using SakuraWeb.Models;

namespace SakuraWeb.Tests.Models;

public class BenefitTests
{
    [Fact]
    public void ParameterlessConstructor_SetsSentinelIdAndEmptyProperties()
    {
        var benefit = new Benefit
        {
            opis = string.Empty,
            URLSlike = string.Empty
        };

        Assert.Equal(-1, benefit.id);
        Assert.Equal(string.Empty, benefit.opis);
        Assert.Equal(string.Empty, benefit.URLSlike);
        Assert.Empty(benefit.ProizvodBenefiti);
    }

    [Theory]
    [InlineData(-1, "", "")]
    [InlineData(0, "Sjaj", "sjaj.png")]
    [InlineData(42, "Jaca kosu", "jaca.png")]
    public void ParameterizedConstructor_SetsProperties(
        int id,
        string opis,
        string urlSlike)
    {
        var benefit = new Benefit(id, opis, urlSlike)
        {
            opis = opis,
            URLSlike = urlSlike
        };

        Assert.Equal(id, benefit.id);
        Assert.Equal(opis, benefit.opis);
        Assert.Equal(urlSlike, benefit.URLSlike);
    }

    [Theory]
    [InlineData("Sjajna kosa", "sjajna-kosa.svg")]
    [InlineData("Jaca kosu", "jaca-kosu.svg")]
    [InlineData("Povecana gustoca", "gustoca.svg")]
    public void Properties_AssignedValues_AreRetained(
        string opis,
        string urlSlike)
    {
        var benefit = new Benefit
        {
            opis = opis,
            URLSlike = urlSlike
        };

        Assert.Equal(opis, benefit.opis);
        Assert.Equal(urlSlike, benefit.URLSlike);
    }

    [Theory]
    [InlineData("Sjajna kosa")]
    [InlineData("Jaca kosu")]
    [InlineData("Povecana gustoca")]
    public void OpisProperty_ValidText_IsValid(string opis)
    {
        var benefit = new Benefit
        {
            opis = opis,
            URLSlike = "benefit.svg"
        };

        var results = Validate(benefit);

        Assert.DoesNotContain(results, result =>
            result.MemberNames.Contains(nameof(Benefit.opis)));
    }

    [Fact]
    public void OpisProperty_EmptyValue_IsInvalid()
    {
        var benefit = new Benefit
        {
            opis = string.Empty,
            URLSlike = "benefit.svg"
        };

        var results = Validate(benefit);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(Benefit.opis)));
    }

    [Fact]
    public void URLSlikeProperty_EmptyValue_IsInvalid()
    {
        var benefit = new Benefit
        {
            opis = "Sjajna kosa",
            URLSlike = string.Empty
        };

        var results = Validate(benefit);

        Assert.Contains(results, result =>
            result.MemberNames.Contains(nameof(Benefit.URLSlike)));
    }

    [Fact]
    public void ProizvodBenefitiProperty_AddingLink_RetainsCollection()
    {
        var benefit = new Benefit(1, "Sjajna kosa", "sjaj.svg")
        {
            opis = "Sjajna kosa",
            URLSlike = "sjaj.svg"
        };
        var link = new ProizvodBenefit { benefitId = 1 };

        benefit.ProizvodBenefiti.Add(link);

        Assert.Single(benefit.ProizvodBenefiti);
        Assert.Same(link, benefit.ProizvodBenefiti.Single());
    }

    private static IReadOnlyCollection<ValidationResult> Validate(Benefit benefit)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(
            benefit,
            new ValidationContext(benefit),
            results,
            validateAllProperties: true);

        return results;
    }
}

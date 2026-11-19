using SakuraWeb.Models;

namespace SakuraWeb.Tests.Models;

public sealed class ProizvodBenefitTests
{
    [Fact]
    public void DefaultConstructor_InitializesZeroIdsAndNullNavigations()
    {
        var link = new ProizvodBenefit();

        Assert.Equal(0, link.id);
        Assert.Equal(0, link.proizvodId);
        Assert.Equal(0, link.benefitId);
        Assert.Null(link.proizvod);
        Assert.Null(link.benefit);
    }

    [Theory]
    [MemberData(nameof(JsonCases))]
    public void Constructor_CopiesIdsAndNavigationReferences(ModelConstructionCase testCase)
    {
        var proizvod = new Proizvod { naziv = testCase.ProizvodNaziv };
        var benefit = new Benefit { opis = "Njega", URLSlike = "benefit.jpg" };

        var link = new ProizvodBenefit(12, 3, 4, proizvod, benefit);

        Assert.Equal(12, link.id);
        Assert.Equal(3, link.proizvodId);
        Assert.Equal(4, link.benefitId);
        Assert.Same(proizvod, link.proizvod);
        Assert.Same(benefit, link.benefit);
    }

    [Fact]
    public void PropertyAssignments_StoreIdsAndNavigationReferences()
    {
        var proizvod = new Proizvod { naziv = "Aloe gel" };
        var benefit = new Benefit { opis = "Njega", URLSlike = "benefit.jpg" };
        var link = new ProizvodBenefit
        {
            id = 5,
            proizvodId = 6,
            benefitId = 7,
            proizvod = proizvod,
            benefit = benefit
        };

        Assert.Equal(5, link.id);
        Assert.Equal(6, link.proizvodId);
        Assert.Equal(7, link.benefitId);
        Assert.Same(proizvod, link.proizvod);
        Assert.Same(benefit, link.benefit);
    }

    public static IEnumerable<object[]> JsonCases() =>
        ModelTestData.JsonConstructionCases();
}

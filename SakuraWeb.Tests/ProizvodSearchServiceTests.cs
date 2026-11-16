using SakuraWeb.Models;
using SakuraWeb.Services;

namespace SakuraWeb.Tests;

public class ProizvodSearchServiceTests
{
    private readonly ProizvodSearchService _service = new();

    [Fact]
    public void Search_FiltersProductsByName_IgnoringCase()
    {
        var proizvodi = new[]
        {
            new Proizvod
            {
                id = 1,
                naziv = "Hidratantni šampon",
                kategorija = KategorijaProizvoda.Šampon
            },

            new Proizvod
            {
                id = 2,
                naziv = "Regenerator za sjaj",
                kategorija = KategorijaProizvoda.Regenerator
            }
        };

        var rezultat =
            _service.Search(
                proizvodi,
                "ŠAMPON",
                null);

        Assert.Single(rezultat);
        Assert.Equal(1, rezultat[0].id);
    }

    [Fact]
    public void Search_FiltersProductsByCategory()
    {
        var proizvodi = new[]
        {
            new Proizvod
            {
                id = 1,
                naziv = "Šampon",
                kategorija = KategorijaProizvoda.Šampon
            },

            new Proizvod
            {
                id = 2,
                naziv = "Ulje",
                kategorija = KategorijaProizvoda.Ulje
            }
        };

        var rezultat =
            _service.Search(
                proizvodi,
                null,
                KategorijaProizvoda.Ulje);

        Assert.Single(rezultat);
        Assert.Equal(2, rezultat[0].id);
    }

    [Fact]
    public void Search_ReturnsAllProducts_WhenFiltersAreEmpty()
    {
        var proizvodi = new[]
        {
            new Proizvod
            {
                id = 1,
                naziv = "Šampon"
            },

            new Proizvod
            {
                id = 2,
                naziv = "Ulje"
            }
        };

        var rezultat =
            _service.Search(
                proizvodi,
                " ",
                null);

        Assert.Equal(2, rezultat.Count);
    }
}
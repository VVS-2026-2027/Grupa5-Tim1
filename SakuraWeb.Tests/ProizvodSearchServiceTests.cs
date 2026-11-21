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
                null,
                null);

        Assert.Equal(2, rezultat.Count);
        Assert.Equal(new[] { 1, 2 }, rezultat.Select(item => item.id));
    }

    [Fact]
    public void Search_CombinesNameAndCategoryFilters()
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
                naziv = "Šampon za volumen",
                kategorija = KategorijaProizvoda.Ulje
            },

            new Proizvod
            {
                id = 3,
                naziv = "Maska za sjaj",
                kategorija = KategorijaProizvoda.Maska
            }
        };

        var rezultat =
            _service.Search(
                proizvodi,
                "ŠAMPON",
                KategorijaProizvoda.Šampon);

        Assert.Single(rezultat);
        Assert.Equal(1, rezultat[0].id);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public void Search_ReturnsAllProducts_WhenNameContainsOnlyWhitespace(string naziv)
    {
        var proizvodi = new[]
        {
            new Proizvod { id = 1, naziv = "Šampon" },
            new Proizvod { id = 2, naziv = "Ulje" }
        };

        var rezultat =
            _service.Search(
                proizvodi,
                naziv,
                null);

        Assert.Equal(2, rezultat.Count);
    }

    [Fact]
    public void Search_ReturnsEmptyList_WhenNameDoesNotExist()
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
                "Nema takvog proizvoda",
                null);

        Assert.Empty(rezultat);
    }

    [Fact]
    public void Search_ReturnsEmptyList_WhenNameAndCategoryDoNotIntersect()
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
                "Šampon",
                KategorijaProizvoda.Ulje);

        Assert.Empty(rezultat);
    }

    [Fact]
    public void Search_CombinesNameAndCategoryWithSurroundingWhitespace()
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
                naziv = "Šampon za volumen",
                kategorija = KategorijaProizvoda.Ulje
            },

            new Proizvod
            {
                id = 3,
                naziv = "Maska za hidrataciju",
                kategorija = KategorijaProizvoda.Maska
            }
        };

        var rezultat =
            _service.Search(
                proizvodi,
                "  ŠAMPON  ",
                KategorijaProizvoda.Šampon);

        Assert.Single(rezultat);
        Assert.Equal(1, rezultat[0].id);
    }

    [Fact]
    public void Search_CombinesNameAndCategoryAcrossMultipleProducts()
    {
        var proizvodi = new[]
        {
            new Proizvod
            {
                id = 1,
                naziv = "Šampon za suvu kosu",
                kategorija = KategorijaProizvoda.Šampon
            },

            new Proizvod
            {
                id = 2,
                naziv = "Hidratantni šampon",
                kategorija = KategorijaProizvoda.Šampon
            },

            new Proizvod
            {
                id = 3,
                naziv = "Šampon za volumen",
                kategorija = KategorijaProizvoda.Ulje
            }
        };

        var rezultat =
            _service.Search(
                proizvodi,
                "šampon",
                KategorijaProizvoda.Šampon);

        Assert.Equal(new[] { 1, 2 }, rezultat.Select(item => item.id));
    }

    [Fact]
    public void Search_ReturnsEmptyList_WhenProductCollectionIsNull()
    {
        IReadOnlyList<Proizvod>? proizvodi = null;

        var rezultat =
            _service.Search(
                proizvodi!,
                "Šampon",
                KategorijaProizvoda.Šampon);

        Assert.Empty(rezultat);
    }

    [Fact]
    public void Search_ReturnsEmptyList_WhenProductCollectionIsEmpty()
    {
        var proizvodi = Array.Empty<Proizvod>();

        var rezultat =
            _service.Search(
                proizvodi,
                null,
                null);

        Assert.Empty(rezultat);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Search_WithNoNameAndCategoryHasNoMatch_ReturnsEmptyList(
        string? naziv)
    {
        var proizvodi = new[]
        {
            new Proizvod
            {
                id = 1,
                naziv = "Šampon",
                kategorija = KategorijaProizvoda.Šampon
            }
        };

        var rezultat =
            _service.Search(
                proizvodi,
                naziv,
                KategorijaProizvoda.Ulje);

        Assert.Empty(rezultat);
    }
}

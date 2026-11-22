using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SakuraWeb.Controllers;
using SakuraWeb.Models;
using SakuraWeb.Services;
using SakuraWeb.Tests.Helpers;

namespace SakuraWeb.Tests.Controllers;

public class ProizvodiControllerTests
{
    private static ProizvodiController CreateController(
        SakuraWeb.Data.ApplicationDbContext db)
    {
        var environment = new Mock<IWebHostEnvironment>();

        environment
            .SetupGet(item => item.WebRootPath)
            .Returns(Path.GetTempPath());

        return new ProizvodiController(
            db,
            environment.Object);
    }

    [Theory]
    [InlineData(
        "CijenaUzlazno",
        new[] { 2, 1 })]
    [InlineData(
        "CijenaSilazno",
        new[] { 1, 2 })]
    [InlineData(
        "NazivUzlazno",
        new[] { 2, 1 })]
    [InlineData(
        "NazivSilazno",
        new[] { 1, 2 })]
    public async Task Index_SortsProducts(
        string sort,
        int[] expectedIds)
    {
        await using var db = TestContext.Create();

        db.proizvodi.AddRange(
            new Proizvod
            {
                id = 1,
                naziv = "Z",
                cijena = 20,
                volumen = 100
            },
            new Proizvod
            {
                id = 2,
                naziv = "A",
                cijena = 10,
                volumen = 100
            });

        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var result = await controller.Index(sort, null);

        var view = Assert.IsType<ViewResult>(result);

        var model =
            Assert.IsAssignableFrom<List<Proizvod>>(view.Model);

        Assert.Equal(
            expectedIds,
            model.Select(item => item.id));
    }

    [Fact]
    public async Task Index_FiltersByCategory()
    {
        await using var db = TestContext.Create();

        db.proizvodi.AddRange(
            new Proizvod
            {
                id = 1,
                naziv = "Šampon",
                cijena = 1,
                volumen = 1,
                kategorija = KategorijaProizvoda.Šampon
            },
            new Proizvod
            {
                id = 2,
                naziv = "Regenerator",
                cijena = 1,
                volumen = 1,
                kategorija = KategorijaProizvoda.Regenerator
            });

        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var result =
            await controller.Index(
                null,
                KategorijaProizvoda.Šampon);

        var view = Assert.IsType<ViewResult>(result);

        var model =
            Assert.IsAssignableFrom<List<Proizvod>>(view.Model);

        Assert.Single(model);
        Assert.Equal(1, model[0].id);
    }

    [Fact]
    public async Task Index_SearchCriteria_FiltersAndSortsThroughController()
    {
        await using var db = TestContext.Create();
        db.proizvodi.AddRange(
            new Proizvod
            {
                id = 1,
                naziv = "Repair shampoo",
                cijena = 20,
                volumen = 250,
                kategorija = KategorijaProizvoda.Šampon
            },
            new Proizvod
            {
                id = 2,
                naziv = "Repair mask",
                cijena = 10,
                volumen = 200,
                kategorija = KategorijaProizvoda.Maska
            },
            new Proizvod
            {
                id = 3,
                naziv = "Daily shampoo",
                cijena = 15,
                volumen = 250,
                kategorija = KategorijaProizvoda.Šampon
            },
            new Proizvod
            {
                id = 4,
                naziv = "Repair shampoo budget",
                cijena = 12,
                volumen = 250,
                kategorija = KategorijaProizvoda.Šampon
            });
        await db.SaveChangesAsync();

        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(item => item.WebRootPath).Returns(Path.GetTempPath());
        var controller = new ProizvodiController(
            db, environment.Object, new ProizvodSearchService());

        var result = await controller.Index(
            "CijenaUzlazno", KategorijaProizvoda.Šampon, "REPAIR");

        var view = Assert.IsType<ViewResult>(result);
        var products = Assert.IsAssignableFrom<List<Proizvod>>(view.Model);
        Assert.Equal(new[] { 4, 1 }, products.Select(item => item.id));
        Assert.Equal("REPAIR", controller.ViewBag.Pretraga);
        Assert.Equal(KategorijaProizvoda.Šampon, controller.ViewBag.Kategorija);
    }

    [Fact]
    public async Task Details_ReturnsNotFoundForNullId()
    {
        await using var db = TestContext.Create();

        var controller = CreateController(db);

        var result = await controller.Details(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFoundForUnknownId()
    {
        await using var db = TestContext.Create();

        var controller = CreateController(db);

        var result = await controller.Details(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_ValidProduct_SavesAndRedirects()
    {
        await using var db = TestContext.Create();

        var product = new Proizvod
        {
            naziv = "Šampon",
            cijena = 10,
            volumen = 250
        };

        var controller = CreateController(db);

        var result =
            await controller.Create(
                product,
                null,
                null,
                null);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Single(db.proizvodi);
    }

    [Fact]
    public async Task Edit_MismatchedId_ReturnsNotFound()
    {
        await using var db = TestContext.Create();

        var controller = CreateController(db);

        var product = new Proizvod
        {
            id = 2,
            naziv = "Test",
            cijena = 1,
            volumen = 1
        };

        var result =
            await controller.Edit(
                1,
                product,
                null,
                null,
                null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteConfirmed_RemovesProduct()
    {
        await using var db = TestContext.Create();

        db.proizvodi.Add(
            new Proizvod
            {
                id = 1,
                naziv = "Test",
                cijena = 1,
                volumen = 1
            });

        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var result =
            await controller.DeleteConfirmed(1);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Empty(db.proizvodi);
    }
}
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SakuraWeb.Controllers;
using SakuraWeb.Models;
using SakuraWeb.Tests.Helpers;

using System.IO;
using Microsoft.AspNetCore.Http;

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


    [Fact]
    public void Create_Get_ReturnsViewWithLookupData()
    {
        using var db = TestContext.Create();
        var controller = CreateController(db);

        var result = controller.Create();

        var view = Assert.IsType<ViewResult>(result);
        Assert.NotNull(view.ViewData["Benefiti"]);
        Assert.NotNull(view.ViewData["Sastojci"]);
    }

    [Fact]
    public async Task Details_ExistingProduct_ReturnsProductView()
    {
        await using var db = TestContext.Create();
        db.proizvodi.Add(new Proizvod
        {
            id = 1,
            naziv = "Šampon",
            cijena = 10,
            volumen = 250
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var result = await controller.Details(1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Proizvod>(view.Model);
        Assert.Equal(1, model.id);
        Assert.Equal("Šampon", model.naziv);
        Assert.NotNull(view.ViewData["ProizvodBenefiti"]);
    }

    [Fact]
    public async Task Create_InvalidModelState_ReturnsViewWithoutSaving()
    {
        await using var db = TestContext.Create();
        var controller = CreateController(db);
        controller.ModelState.AddModelError("naziv", "Naziv je obavezan.");

        var product = new Proizvod
        {
            naziv = "Test",
            cijena = 10,
            volumen = 100
        };

        var result = await controller.Create(product, null, null, null);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(product, view.Model);
        Assert.Empty(db.proizvodi);
        Assert.NotNull(view.ViewData["Benefiti"]);
        Assert.NotNull(view.ViewData["Sastojci"]);
    }

    [Fact]
    public async Task Edit_Get_NullId_ReturnsNotFound()
    {
        await using var db = TestContext.Create();
        var controller = CreateController(db);

        var result = await controller.Edit(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_UnknownId_ReturnsNotFound()
    {
        await using var db = TestContext.Create();
        var controller = CreateController(db);

        var result = await controller.Edit(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_ExistingProduct_ReturnsProductAndLookupData()
    {
        await using var db = TestContext.Create();
        db.proizvodi.Add(new Proizvod
        {
            id = 1,
            naziv = "Šampon",
            cijena = 10,
            volumen = 250
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var result = await controller.Edit(1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Proizvod>(view.Model);
        Assert.Equal(1, model.id);
        Assert.NotNull(view.ViewData["Benefiti"]);
        Assert.NotNull(view.ViewData["Sastojci"]);
        Assert.NotNull(view.ViewData["SelectedBenefitIds"]);
        Assert.NotNull(view.ViewData["SelectedSastojakIds"]);
    }

    [Fact]
    public async Task Edit_ValidProduct_UpdatesProductAndRedirects()
    {
        await using var db = TestContext.Create();
        db.proizvodi.Add(new Proizvod
        {
            id = 1,
            naziv = "Stari naziv",
            cijena = 10,
            volumen = 100
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db);
        var updatedProduct = new Proizvod
        {
            id = 1,
            naziv = "Novi naziv",
            cijena = 15,
            volumen = 250
        };

        var result = await controller.Edit(1, updatedProduct, null, null, null);

        Assert.IsType<RedirectToActionResult>(result);
        var savedProduct = await db.proizvodi.FindAsync(1);
        Assert.NotNull(savedProduct);
        Assert.Equal("Novi naziv", savedProduct.naziv);
        Assert.Equal(15, savedProduct.cijena);
        Assert.Equal(250, savedProduct.volumen);
    }

    [Fact]
    public async Task Edit_IdDoesNotExist_ReturnsNotFound()
    {
        await using var db = TestContext.Create();
        var controller = CreateController(db);
        var product = new Proizvod
        {
            id = 1,
            naziv = "Test",
            cijena = 10,
            volumen = 100
        };

        var result = await controller.Edit(1, product, null, null, null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_InvalidModelState_ReturnsViewWithLookupData()
    {
        await using var db = TestContext.Create();
        var controller = CreateController(db);
        controller.ModelState.AddModelError("naziv", "Naziv je obavezan.");

        var product = new Proizvod
        {
            id = 1,
            naziv = "Test",
            cijena = 10,
            volumen = 100
        };

        var result = await controller.Edit(1, product, null, null, null);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(product, view.Model);
        Assert.NotNull(view.ViewData["Benefiti"]);
        Assert.NotNull(view.ViewData["Sastojci"]);
        Assert.Empty(db.proizvodi);
    }

    [Fact]
    public async Task Delete_Get_NullId_ReturnsNotFound()
    {
        await using var db = TestContext.Create();
        var controller = CreateController(db);

        var result = await controller.Delete(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Get_UnknownId_ReturnsNotFound()
    {
        await using var db = TestContext.Create();
        var controller = CreateController(db);

        var result = await controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Get_ExistingProduct_ReturnsProductView()
    {
        await using var db = TestContext.Create();
        db.proizvodi.Add(new Proizvod
        {
            id = 1,
            naziv = "Šampon",
            cijena = 10,
            volumen = 250
        });
        await db.SaveChangesAsync();

        var controller = CreateController(db);

        var result = await controller.Delete(1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Proizvod>(view.Model);
        Assert.Equal(1, model.id);
    }

    [Fact]
    public async Task DeleteConfirmed_UnknownId_StillRedirectsToIndex()
    {
        await using var db = TestContext.Create();
        var controller = CreateController(db);

        var result = await controller.DeleteConfirmed(999);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Empty(db.proizvodi);
    }

    [Fact]
    public async Task Create_WithImage_SavesImageAndProduct()
    {
        await using var db = TestContext.Create();

        var environment = new Mock<IWebHostEnvironment>();
        var webRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        environment.SetupGet(e => e.WebRootPath).Returns(webRoot);

        var controller = new ProizvodiController(db, environment.Object);

        var imageContent = new byte[] { 1, 2, 3, 4 };

        var image = new Mock<IFormFile>();

        image.SetupGet(i => i.Length).Returns(imageContent.Length);
        image.SetupGet(i => i.FileName).Returns("test.jpg");

        image.Setup(i => i.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns<Stream, CancellationToken>((stream, token) =>
                stream.WriteAsync(imageContent, token).AsTask());

        var product = new Proizvod
        {
            naziv = "Šampon",
            cijena = 10,
            volumen = 250
        };

        var result = await controller.Create(
            product, image.Object, null, null);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.NotNull(product.slikaPutanja);
        Assert.EndsWith(".jpg", product.slikaPutanja);

        var savedFile = Path.Combine(webRoot, "img", product.slikaPutanja);

        Assert.True(File.Exists(savedFile));
        Assert.Equal(imageContent, await File.ReadAllBytesAsync(savedFile));

        Directory.Delete(webRoot, true);
    }

    [Fact]
    public async Task Create_WithBenefits_SavesProductBenefits()
    {
        await using var db = TestContext.Create();

        var controller = CreateController(db);

        var product = new Proizvod
        {
            naziv = "Šampon",
            cijena = 10,
            volumen = 250
        };

        int[] benefitIds = { 1, 2 };

        var result = await controller.Create(
            product, null, benefitIds, null);

        Assert.IsType<RedirectToActionResult>(result);

        var savedBenefits = db.proizvodiBenefiti
            .Where(pb => pb.proizvodId == product.id)
            .ToList();

        Assert.Equal(2, savedBenefits.Count);
        Assert.Contains(savedBenefits, b => b.benefitId == 1);
        Assert.Contains(savedBenefits, b => b.benefitId == 2);
    }

    [Fact]
    public async Task Create_WithIngredients_SavesProductIngredients()
    {
        await using var db = TestContext.Create();

        var controller = CreateController(db);

        var product = new Proizvod
        {
            naziv = "Šampon",
            cijena = 10,
            volumen = 250
        };

        int[] sastojakIds = { 1, 2, 3 };

        var result = await controller.Create(
            product, null, null, sastojakIds);

        Assert.IsType<RedirectToActionResult>(result);

        var savedIngredients = db.proizvodiSastojci
            .Where(ps => ps.proizvodId == product.id)
            .ToList();

        Assert.Equal(3, savedIngredients.Count);
        Assert.Contains(savedIngredients, s => s.sastojakId == 1);
        Assert.Contains(savedIngredients, s => s.sastojakId == 2);
        Assert.Contains(savedIngredients, s => s.sastojakId == 3);
    }

    [Fact]
    public async Task Edit_WithImageAndRelationships_UpdatesProductAndLinks()
    {
        await using var db = TestContext.Create();
        db.proizvodi.Add(new Proizvod
        {
            id = 1,
            naziv = "Stari naziv",
            cijena = 10,
            volumen = 100
        });
        await db.SaveChangesAsync();

        var webRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var environment = new Mock<IWebHostEnvironment>();
        environment.SetupGet(item => item.WebRootPath).Returns(webRoot);
        var controller = new ProizvodiController(db, environment.Object);
        var imageContent = new byte[] { 4, 3, 2, 1 };
        var image = new Mock<IFormFile>();
        image.SetupGet(item => item.Length).Returns(imageContent.Length);
        image.SetupGet(item => item.FileName).Returns("edited.png");
        image.Setup(item => item.CopyToAsync(
                It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns<Stream, CancellationToken>((stream, token) =>
                stream.WriteAsync(imageContent, token).AsTask());

        try
        {
            var updated = new Proizvod
            {
                id = 1,
                naziv = "Novi naziv",
                cijena = 15,
                volumen = 250,
                poeniPr = 4
            };

            var result = await controller.Edit(
                1, updated, image.Object, new[] { 1, 2 }, new[] { 3, 4 });

            Assert.IsType<RedirectToActionResult>(result);
            var saved = await db.proizvodi.FindAsync(1);
            Assert.NotNull(saved);
            Assert.Equal("Novi naziv", saved.naziv);
            Assert.Equal(15, saved.cijena);
            Assert.Equal(250, saved.volumen);
            Assert.Equal(4, saved.poeniPr);
            Assert.EndsWith(".png", saved.slikaPutanja);

            var savedImage = Path.Combine(webRoot, "img", saved.slikaPutanja!);
            Assert.Equal(imageContent, await File.ReadAllBytesAsync(savedImage));
            Assert.Equal(new[] { 1, 2 }, db.proizvodiBenefiti
                .OrderBy(item => item.benefitId)
                .Select(item => item.benefitId).ToArray());
            Assert.Equal(new[] { 3, 4 }, db.proizvodiSastojci
                .OrderBy(item => item.sastojakId)
                .Select(item => item.sastojakId).ToArray());
        }
        finally
        {
            if (Directory.Exists(webRoot))
            {
                Directory.Delete(webRoot, recursive: true);
            }
        }
    }

    [Fact]
    public async Task Edit_ConcurrencyException_WhenProductWasDeleted_ReturnsNotFound()
    {
        await using var db = CreateConcurrencyContext(deleteProductOnConflict: true);
        db.proizvodi.Add(new Proizvod
        {
            id = 1,
            naziv = "Postojeći proizvod",
            cijena = 10,
            volumen = 100
        });
        await db.SaveChangesAsync();
        db.ThrowOnNextSave = true;

        var controller = CreateController(db);
        var result = await controller.Edit(1, new Proizvod
        {
            id = 1,
            naziv = "Izmijenjen proizvod",
            cijena = 12,
            volumen = 120
        }, null, null, null);

        Assert.IsType<NotFoundResult>(result);
        Assert.Empty(db.proizvodi);
    }

    [Fact]
    public async Task Edit_ConcurrencyException_WhenProductStillExists_Rethrows()
    {
        await using var db = CreateConcurrencyContext(deleteProductOnConflict: false);
        db.proizvodi.Add(new Proizvod
        {
            id = 1,
            naziv = "Postojeći proizvod",
            cijena = 10,
            volumen = 100
        });
        await db.SaveChangesAsync();
        db.ThrowOnNextSave = true;

        var controller = CreateController(db);
        var product = new Proizvod
        {
            id = 1,
            naziv = "Izmijenjen proizvod",
            cijena = 12,
            volumen = 120
        };

        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => controller.Edit(1, product, null, null, null));
    }

    private static ConcurrencyTestContext CreateConcurrencyContext(
        bool deleteProductOnConflict)
    {
        var options = new DbContextOptionsBuilder<
                SakuraWeb.Data.ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ConcurrencyTestContext(options, deleteProductOnConflict);
    }

    private sealed class ConcurrencyTestContext : SakuraWeb.Data.ApplicationDbContext
    {
        private readonly bool _deleteProductOnConflict;

        public ConcurrencyTestContext(
            DbContextOptions<SakuraWeb.Data.ApplicationDbContext> options,
            bool deleteProductOnConflict)
            : base(options)
        {
            _deleteProductOnConflict = deleteProductOnConflict;
        }

        public bool ThrowOnNextSave { get; set; }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            if (!ThrowOnNextSave)
            {
                return await base.SaveChangesAsync(cancellationToken);
            }

            ThrowOnNextSave = false;
            if (_deleteProductOnConflict)
            {
                var product = proizvodi.Local.FirstOrDefault();
                if (product is not null)
                {
                    proizvodi.Remove(product);
                    await base.SaveChangesAsync(cancellationToken);
                }
            }

            throw new DbUpdateConcurrencyException("Simulated update conflict.");
        }
    }

}

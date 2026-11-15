using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SakuraWeb.Controllers;
using SakuraWeb.Models;
using SakuraWeb.Tests.Helpers;

namespace SakuraWeb.Tests.Controllers;

public class UslugaControllerTests
{
    // --- Index ---
    [Fact]
    public async Task Index_ReturnsServices()
    {
        await using var db = TestContext.Create();

        db.usluge.AddRange(
            new Usluga { id = 2, naziv = "Z", opis = "opis", cijena = 10, trajanje = 30 },
            new Usluga { id = 1, naziv = "A", opis = "opis", cijena = 10, trajanje = 30 });

        await db.SaveChangesAsync();

        var controller = new UslugaController(db);
        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Usluga>>(view.Model);

        Assert.Equal(new[] { 1, 2 }, model.Select(item => item.id));
    }

    // --- Details ---
    [Fact]
    public async Task Details_ReturnsNotFoundForNullId()
    {
        await using var db = TestContext.Create();
        var controller = new UslugaController(db);

        var result = await controller.Details(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFoundForUnknownId()
    {
        await using var db = TestContext.Create();
        var controller = new UslugaController(db);

        var result = await controller.Details(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsViewForExistingId()
    {
        await using var db = TestContext.Create();
        db.usluge.Add(new Usluga { id = 1, naziv = "Test", opis = "Opis", cijena = 20, trajanje = 45 });
        await db.SaveChangesAsync();

        var controller = new UslugaController(db);
        var result = await controller.Details(1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<Usluga>(view.Model);
        Assert.Equal(1, model.id);
    }

    // --- Create (GET & POST) ---
    [Fact]
    public void Create_Get_ReturnsView()
    {
        using var db = TestContext.Create();
        var controller = new UslugaController(db);

        var result = controller.Create();

        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Create_ValidService_SavesAndRedirects()
    {
        await using var db = TestContext.Create();

        var service = new Usluga
        {
            naziv = "Šišanje",
            opis = "Opis",
            cijena = 15,
            trajanje = 30
        };

        var controller = new UslugaController(db);
        var result = await controller.Create(service);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Single(db.usluge);
    }

    [Fact]
    public async Task Create_InvalidModel_ReturnsView()
    {
        await using var db = TestContext.Create();
        var controller = new UslugaController(db);
        controller.ModelState.AddModelError("naziv", "Obavezno polje");

        var service = new Usluga { naziv = "", opis = "Opis", cijena = 15, trajanje = 30 };
        var result = await controller.Create(service);

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsAssignableFrom<Usluga>(view.Model);
        Assert.Empty(db.usluge);
    }

    // --- Edit (GET & POST) ---
    [Fact]
    public async Task Edit_Get_ReturnsNotFoundForNullId()
    {
        await using var db = TestContext.Create();
        var controller = new UslugaController(db);

        var result = await controller.Edit((int?)null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_ReturnsNotFoundForUnknownId()
    {
        await using var db = TestContext.Create();
        var controller = new UslugaController(db);

        var result = await controller.Edit(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_ReturnsViewForExistingId()
    {
        await using var db = TestContext.Create();
        db.usluge.Add(new Usluga { id = 1, naziv = "Test", opis = "Opis", cijena = 20, trajanje = 45 });
        await db.SaveChangesAsync();

        var controller = new UslugaController(db);
        var result = await controller.Edit(1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<Usluga>(view.Model);
        Assert.Equal(1, model.id);
    }

    [Fact]
    public async Task Edit_MismatchedId_ReturnsNotFound()
    {
        await using var db = TestContext.Create();
        var controller = new UslugaController(db);

        var result = await controller.Edit(1, new Usluga { id = 2 });

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_InvalidModel_ReturnsView()
    {
        await using var db = TestContext.Create();
        var controller = new UslugaController(db);
        controller.ModelState.AddModelError("naziv", "Greška");

        var usluga = new Usluga { id = 1, naziv = "Test", opis = "Opis", cijena = 10, trajanje = 30 };
        var result = await controller.Edit(1, usluga);

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsAssignableFrom<Usluga>(view.Model);
    }

    [Fact]
    public async Task Edit_Post_ValidModel_UpdatesAndRedirects()
    {
        await using var db = TestContext.Create();
        db.usluge.Add(new Usluga { id = 1, naziv = "Stari naziv", opis = "Opis", cijena = 10, trajanje = 30 });
        await db.SaveChangesAsync();

        var controller = new UslugaController(db);
        db.ChangeTracker.Clear();

        var updated = new Usluga { id = 1, naziv = "Novi naziv", opis = "Novi opis", cijena = 25, trajanje = 60 };
        var result = await controller.Edit(1, updated);

        Assert.IsType<RedirectToActionResult>(result);
        var dbUsluga = await db.usluge.FindAsync(1);
        Assert.Equal("Novi naziv", dbUsluga!.naziv);
    }

    [Fact]
    public async Task Edit_Post_ConcurrencyException_WhenNotExists_ReturnsNotFound()
    {
        await using var db = TestContext.Create();
        var controller = new UslugaController(db);
        var usluga = new Usluga { id = 999, naziv = "Nepostojeća", opis = "Opis", cijena = 10, trajanje = 30 };

        db.usluge.Attach(usluga);
        db.Entry(usluga).State = EntityState.Modified;

        var result = await controller.Edit(999, usluga);

        Assert.IsType<NotFoundResult>(result);
    }

    // --- Delete (GET & POST) ---
    [Fact]
    public async Task Delete_Get_ReturnsNotFoundForNullId()
    {
        await using var db = TestContext.Create();
        var controller = new UslugaController(db);

        var result = await controller.Delete(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Get_ReturnsNotFoundForUnknownId()
    {
        await using var db = TestContext.Create();
        var controller = new UslugaController(db);

        var result = await controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Get_ReturnsViewForExistingId()
    {
        await using var db = TestContext.Create();
        db.usluge.Add(new Usluga { id = 1, naziv = "Test", opis = "Opis", cijena = 10, trajanje = 30 });
        await db.SaveChangesAsync();

        var controller = new UslugaController(db);
        var result = await controller.Delete(1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<Usluga>(view.Model);
        Assert.Equal(1, model.id);
    }

    [Fact]
    public async Task DeleteConfirmed_RemovesService()
    {
        await using var db = TestContext.Create();

        db.usluge.Add(
            new Usluga
            {
                id = 1,
                naziv = "Test",
                opis = "Opis",
                cijena = 1,
                trajanje = 30
            });

        await db.SaveChangesAsync();

        var controller = new UslugaController(db);
        var result = await controller.DeleteConfirmed(1);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Empty(db.usluge);
    }

    [Fact]
    public async Task DeleteConfirmed_WhenNotFound_Redirects()
    {
        await using var db = TestContext.Create();
        var controller = new UslugaController(db);

        var result = await controller.DeleteConfirmed(999);

        Assert.IsType<RedirectToActionResult>(result);
    }
}
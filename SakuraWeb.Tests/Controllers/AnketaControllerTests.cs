using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SakuraWeb.Controllers;
using SakuraWeb.Models;
using SakuraWeb.Tests.Helpers;

namespace SakuraWeb.Tests.Controllers;

public class AnketaControllerTests
{
    private sealed class RecommendationCase
    {
        public List<int> AnswerIds { get; set; } = new();
        public int ExpectedCount { get; set; }
    }

    public static IEnumerable<object[]> RecommendationCases()
    {
        var path = Path.Combine(
            Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location)!,
            "TestData",
            "recommendation-cases.json");

        using var stream = File.OpenRead(path);

        var cases =
            JsonSerializer.Deserialize<List<RecommendationCase>>(
                stream,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;

        return cases.Select(item => new object[]
        {
            item.AnswerIds,
            item.ExpectedCount
        });
    }

    // --- Index Tests ---
    [Fact]
    public async Task Index_ReturnsQuestionsOrderedById()
    {
        await using var db = TestContext.Create();

        db.pitanja.AddRange(
            new Pitanje { id = 2, sadrzaj = "Drugo" },
            new Pitanje { id = 1, sadrzaj = "Prvo" });

        await db.SaveChangesAsync();

        var controller = new AnketaController(db);
        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Pitanje>>(view.Model);

        Assert.Equal(new[] { 1, 2 }, model.Select(item => item.id));
    }

    // --- Details Tests ---
    [Fact]
    public async Task Details_ReturnsNotFoundForNullId()
    {
        await using var db = TestContext.Create();
        var controller = new AnketaController(db);

        var result = await controller.Details(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFoundForUnknownId()
    {
        await using var db = TestContext.Create();
        var controller = new AnketaController(db);

        var result = await controller.Details(404);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsViewWithAnketa()
    {
        await using var db = TestContext.Create();
        var anketa = new Anketa { id = 1, poeni = 5 };
        db.ankete.Add(anketa);
        await db.SaveChangesAsync();

        var controller = new AnketaController(db);
        var result = await controller.Details(1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<Anketa>(view.Model);
        Assert.Equal(1, model.id);
    }

    // --- Create Tests (GET) ---
    [Fact]
    public async Task Create_Get_ReturnsView()
    {
        await using var db = TestContext.Create();
        var controller = new AnketaController(db);

        var result = controller.Create();
        var view = Assert.IsType<ViewResult>(result);
        Assert.True(controller.ViewData.ContainsKey("korisnikId"));
    }

    // --- Create Tests (POST) ---
    [Fact]
    public async Task Create_ValidModel_SavesAndRedirects()
    {
        await using var db = TestContext.Create();
        var controller = new AnketaController(db);

        var anketa = new Anketa { poeni = 5, korisnikId = "user" };
        var result = await controller.Create(anketa);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Single(db.ankete);
    }

    [Fact]
    public async Task Create_InvalidModel_ReturnsView()
    {
        await using var db = TestContext.Create();
        var controller = new AnketaController(db);

        controller.ModelState.AddModelError("poeni", "Neispravan unos");
        var anketa = new Anketa();
        var result = await controller.Create(anketa);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Empty(db.ankete);
        Assert.True(controller.ViewData.ContainsKey("korisnikId"));
    }

    // --- Edit Tests (GET) ---
    [Fact]
    public async Task Edit_Get_ReturnsNotFoundForNullId()
    {
        await using var db = TestContext.Create();
        var controller = new AnketaController(db);

        var result = await controller.Edit((int?)null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_ReturnsNotFoundForUnknownId()
    {
        await using var db = TestContext.Create();
        var controller = new AnketaController(db);

        var result = await controller.Edit(404);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_ReturnsViewWithAnketa()
    {
        await using var db = TestContext.Create();
        var anketa = new Anketa { id = 1, poeni = 5 };
        db.ankete.Add(anketa);
        await db.SaveChangesAsync();

        var controller = new AnketaController(db);
        var result = await controller.Edit(1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<Anketa>(view.Model);
        Assert.Equal(1, model.id);
        Assert.True(controller.ViewData.ContainsKey("korisnikId"));
    }

    // --- Edit Tests (POST) ---
    [Fact]
    public async Task Edit_Post_MismatchedId_ReturnsNotFound()
    {
        await using var db = TestContext.Create();
        var controller = new AnketaController(db);

        var result = await controller.Edit(1, new Anketa { id = 2 });
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_InvalidModel_ReturnsView()
    {
        await using var db = TestContext.Create();
        var controller = new AnketaController(db);
        controller.ModelState.AddModelError("poeni", "Error");

        var anketa = new Anketa { id = 1 };
        var result = await controller.Edit(1, anketa);

        var view = Assert.IsType<ViewResult>(result);
        Assert.True(controller.ViewData.ContainsKey("korisnikId"));
    }

    [Fact]
    public async Task Edit_Post_ValidModel_UpdatesAndRedirects()
    {
        await using var db = TestContext.Create();
        var anketa = new Anketa { id = 1, poeni = 5 };
        db.ankete.Add(anketa);
        await db.SaveChangesAsync();

        var controller = new AnketaController(db);

        db.ChangeTracker.Clear();

        var updatedAnketa = new Anketa { id = 1, poeni = 10 };
        var result = await controller.Edit(1, updatedAnketa);

        Assert.IsType<RedirectToActionResult>(result);
        var dbAnketa = await db.ankete.FindAsync(1);
        Assert.Equal(10, dbAnketa.poeni);
    }

    [Fact]
    public async Task Edit_Post_DbUpdateConcurrencyException_AnketaDoesNotExist()
    {
        await using var db = TestContext.Create();
        var controller = new AnketaController(db);
        var anketa = new Anketa { id = 99 };

        db.ankete.Attach(anketa);
        db.Entry(anketa).State = EntityState.Modified;

        var result = await controller.Edit(99, anketa);
        Assert.IsType<NotFoundResult>(result);
    }

    // --- Delete Tests (GET) ---
    [Fact]
    public async Task Delete_Get_ReturnsNotFoundForNullId()
    {
        await using var db = TestContext.Create();
        var controller = new AnketaController(db);

        var result = await controller.Delete(null);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Get_ReturnsNotFoundForUnknownId()
    {
        await using var db = TestContext.Create();
        var controller = new AnketaController(db);

        var result = await controller.Delete(404);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Get_ReturnsViewWithAnketa()
    {
        await using var db = TestContext.Create();
        var anketa = new Anketa { id = 1, poeni = 5 };
        db.ankete.Add(anketa);
        await db.SaveChangesAsync();

        var controller = new AnketaController(db);
        var result = await controller.Delete(1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<Anketa>(view.Model);
        Assert.Equal(1, model.id);
    }

    // --- Delete Tests (POST) ---
    [Fact]
    public async Task DeleteConfirmed_RemovesAnketaAndRedirects()
    {
        await using var db = TestContext.Create();
        db.ankete.Add(new Anketa { id = 1 });
        await db.SaveChangesAsync();

        var controller = new AnketaController(db);
        var result = await controller.DeleteConfirmed(1);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Empty(db.ankete);
    }

    [Fact]
    public async Task DeleteConfirmed_AnketaNotFound_Redirects()
    {
        await using var db = TestContext.Create();
        var controller = new AnketaController(db);

        var result = await controller.DeleteConfirmed(1);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Empty(db.ankete);
    }

    // --- Popuni Tests ---
    [Fact]
    public async Task Popuni_ReturnsQuestionsOrderedById()
    {
        await using var db = TestContext.Create();
        db.pitanja.AddRange(
            new Pitanje { id = 2, sadrzaj = "Drugo" },
            new Pitanje { id = 1, sadrzaj = "Prvo" });
        await db.SaveChangesAsync();

        var controller = new AnketaController(db);
        var result = await controller.Popuni();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<List<Pitanje>>(view.Model);
        Assert.Equal(new[] { 1, 2 }, model.Select(item => item.id));
    }

    // --- Preporuka Tests ---
    [Fact]
    public async Task Preporuka_ReturnsClosestProducts()
    {
        await using var db = TestContext.Create();

        db.odgovori.Add(new Odgovor { id = 1, poeni = 5 });

        db.proizvodi.AddRange(
            new Proizvod { id = 1, naziv = "Šampon", poeniPr = 5, kategorija = KategorijaProizvoda.Šampon },
            new Proizvod { id = 2, naziv = "Regenerator", poeniPr = 4, kategorija = KategorijaProizvoda.Regenerator },
            new Proizvod { id = 3, naziv = "Ulje", poeniPr = 6, kategorija = KategorijaProizvoda.Ulje });

        await db.SaveChangesAsync();

        var controller = new AnketaController(db);
        var result = await controller.Preporuka(new List<int> { 1 });

        var json = Assert.IsType<JsonResult>(result);
        var recommendations = Assert.IsAssignableFrom<IEnumerable<object>>(json.Value);

        Assert.Equal(3, recommendations.Count());
    }

    [Theory]
    [MemberData(nameof(RecommendationCases))]
    public async Task Preporuka_UsesJsonData(List<int> answerIds, int expectedCount)
    {
        await using var db = TestContext.Create();

        db.odgovori.AddRange(
            new Odgovor { id = 1, poeni = 1 },
            new Odgovor { id = 2, poeni = 2 });

        db.proizvodi.AddRange(
            new Proizvod { id = 1, naziv = "Šampon", kategorija = KategorijaProizvoda.Šampon },
            new Proizvod { id = 2, naziv = "Regenerator", kategorija = KategorijaProizvoda.Regenerator },
            new Proizvod { id = 3, naziv = "Ulje", kategorija = KategorijaProizvoda.Ulje });

        await db.SaveChangesAsync();

        var controller = new AnketaController(db);
        var result = await controller.Preporuka(answerIds);

        var json = Assert.IsType<JsonResult>(result);
        var recommendations = Assert.IsAssignableFrom<IEnumerable<object>>(json.Value);

        Assert.Equal(expectedCount, recommendations.Count());
    }
}

using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using SakuraWeb.Controllers;
using SakuraWeb.Models;
using SakuraWeb.Tests.Helpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SakuraWeb.Services;

namespace SakuraWeb.Tests.Controllers;

public class NewsletterControllerTests
{
    // --- Index Tests ---
    [Fact]
    public async Task Index_ReturnsViewWithPoruke()
    {
        await using var db = TestContext.Create();
        db.poruke.Add(new Poruka { id = 1, naziv = "Naslov", tekst = "Test" });
        await db.SaveChangesAsync();

        var controller = new NewsletterController(db, Mock.Of<IEmailSender>());
        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Poruka>>(view.Model);
        Assert.Single(model);
    }

    // --- Details Tests ---
    [Fact]
    public async Task Details_ReturnsNotFoundForNullId()
    {
        await using var db = TestContext.Create();

        var controller = new NewsletterController(db, Mock.Of<IEmailSender>());
        var result = await controller.Details(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ReturnsNotFoundForUnknownId()
    {
        await using var db = TestContext.Create();

        var controller = new NewsletterController(db, Mock.Of<IEmailSender>());
        var result = await controller.Details(999);

        Assert.IsType<NotFoundResult>(result);
    }

   [Fact]
    public async Task Details_ReturnsViewWithNewsletter()
    {
        await using var db = TestContext.Create();
        db.newsletteri.Add(new Newsletter { id = 1, emailAdresa = "test@test.com", lozinkaZaEmail = "lozinka123" });
        await db.SaveChangesAsync();

        var controller = new NewsletterController(db, Mock.Of<IEmailSender>());
        var result = await controller.Details(1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<Newsletter>(view.Model);
        Assert.Equal(1, model.id);
    }

    // --- Create Tests ---
    [Fact]
    public void Create_Get_ReturnsView()
    {
        using var db = TestContext.Create();
        var controller = new NewsletterController(db, Mock.Of<IEmailSender>());
        
        var result = controller.Create();
        
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Create_SendsEmailToSubscriber()
    {
        await using var db = TestContext.Create();

        db.newsletteri.Add(new Newsletter
        {
            id = 1,
            emailAdresa = "sender@test.com",
            lozinkaZaEmail = "password"
        });

        db.korisnici.Add(new Korisnik
        {
            Id = "1",
            emailAdresa = "subscriber@test.com",
            jePretplacenNaNewsletter = true
        });

        await db.SaveChangesAsync();

        var email = new Mock<IEmailSender>();
        var controller = new NewsletterController(db, email.Object);

        var result = await controller.Create(new Poruka
        {
            naziv = "Naslov",
            tekst = "Tekst"
        });

        Assert.IsType<RedirectToActionResult>(result);

        email.Verify(
            item => item.SendEmailAsync(
                "subscriber@test.com",
                "Naslov",
                "Tekst"),
            Times.Once);

        Assert.Single(db.poruke);
    }

    [Fact]
    public async Task Create_WithoutSettings_ReturnsView()
    {
        await using var db = TestContext.Create();
        var controller = new NewsletterController(db, Mock.Of<IEmailSender>());

        var result = await controller.Create(new Poruka
        {
            naziv = "Naslov",
            tekst = "Tekst"
        });

        Assert.IsType<ViewResult>(result);
        Assert.Empty(db.poruke);
    }

    // --- Edit Tests ---
    [Fact]
    public async Task Edit_Get_ReturnsNotFoundForNullId()
    {
        await using var db = TestContext.Create();
        var controller = new NewsletterController(db, Mock.Of<IEmailSender>());
        
        var result = await controller.Edit((int?)null);
        
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_ReturnsNotFoundForUnknownId()
    {
        await using var db = TestContext.Create();
        var controller = new NewsletterController(db, Mock.Of<IEmailSender>());
        
        var result = await controller.Edit(999);
        
        Assert.IsType<NotFoundResult>(result);
    }

   [Fact]
    public async Task Edit_Get_ReturnsViewWithNewsletter()
    {
        await using var db = TestContext.Create();
        db.newsletteri.Add(new Newsletter { id = 1, emailAdresa = "test@test.com", lozinkaZaEmail = "lozinka123" });
        await db.SaveChangesAsync();

        var controller = new NewsletterController(db, Mock.Of<IEmailSender>());
        var result = await controller.Edit(1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<Newsletter>(view.Model);
        Assert.Equal(1, model.id);
    }
    [Fact]
    public async Task Edit_Post_ReturnsNotFoundForMismatchedId()
    {
        await using var db = TestContext.Create();
        var controller = new NewsletterController(db, Mock.Of<IEmailSender>());
        
        var result = await controller.Edit(1, new Newsletter { id = 2 });
        
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_ReturnsViewWhenModelStateInvalid()
    {
        await using var db = TestContext.Create();
        var controller = new NewsletterController(db, Mock.Of<IEmailSender>());
        controller.ModelState.AddModelError("emailAdresa", "Neispravan format");

        var result = await controller.Edit(1, new Newsletter { id = 1 });

        var view = Assert.IsType<ViewResult>(result);
        Assert.IsAssignableFrom<Newsletter>(view.Model);
    }

   [Fact]
    public async Task Edit_Post_UpdatesAndRedirects()
    {
        await using var db = TestContext.Create();
        db.newsletteri.Add(new Newsletter { id = 1, emailAdresa = "stari@test.com", lozinkaZaEmail = "lozinka123" });
        await db.SaveChangesAsync();

        var controller = new NewsletterController(db, Mock.Of<IEmailSender>());
        db.ChangeTracker.Clear();

        var result = await controller.Edit(1, new Newsletter { id = 1, emailAdresa = "novi@test.com", lozinkaZaEmail = "novaLozinka" });

        Assert.IsType<RedirectToActionResult>(result);
        var updated = await db.newsletteri.FindAsync(1);
        Assert.Equal("novi@test.com", updated.emailAdresa);
    }

    [Fact]
    public async Task Edit_Post_DbUpdateConcurrencyException_ReturnsNotFound()
    {
        await using var db = TestContext.Create();
        var controller = new NewsletterController(db, Mock.Of<IEmailSender>());
        var newsletter = new Newsletter { id = 999 };

        db.newsletteri.Attach(newsletter);
        db.Entry(newsletter).State = EntityState.Modified;

        var result = await controller.Edit(999, newsletter);
        
        Assert.IsType<NotFoundResult>(result);
    }

    // --- Delete Tests ---
    [Fact]
    public async Task Delete_ReturnsNotFoundForNullId()
    {
        await using var db = TestContext.Create();

        var controller = new NewsletterController(db, Mock.Of<IEmailSender>());
        var result = await controller.Delete(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFoundForUnknownId()
    {
        await using var db = TestContext.Create();

        var controller = new NewsletterController(db, Mock.Of<IEmailSender>());
        var result = await controller.Delete(999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsViewWithNewsletter()
    {
        await using var db = TestContext.Create();
        db.newsletteri.Add(new Newsletter { id = 1, emailAdresa = "test@test.com", lozinkaZaEmail = "lozinka123" });
        await db.SaveChangesAsync();

        var controller = new NewsletterController(db, Mock.Of<IEmailSender>());
        var result = await controller.Delete(1);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<Newsletter>(view.Model);
        Assert.Equal(1, model.id);
    }

    [Fact]
    public async Task DeleteConfirmed_RemovesNewsletterAndRedirects()
    {
        await using var db = TestContext.Create();
        db.newsletteri.Add(new Newsletter { id = 1, emailAdresa = "test@test.com", lozinkaZaEmail = "lozinka123" });
        await db.SaveChangesAsync();

        var controller = new NewsletterController(db, Mock.Of<IEmailSender>());
        var result = await controller.DeleteConfirmed(1);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Empty(db.newsletteri);
    }
    [Fact]
    public async Task Create_Post_NewsletterNull_ReturnsView()
    {
        await using var db = TestContext.Create();
        var controller = new NewsletterController(db, Mock.Of<IEmailSender>());
        var result = await controller.Create(new Poruka { naziv = "Test", tekst = "Test" });
        Assert.IsType<ViewResult>(result);
    }
    [Fact]
    public async Task Create_Post_UsesConcreteEmailSender()
    {
        await using var db = TestContext.Create();
        
        // Moramo imati newsletter i pretplatnika da bi kod došao do slanja emaila
        db.newsletteri.Add(new Newsletter { id = 1, emailAdresa = "posiljalac@test.com", lozinkaZaEmail = "lozinka123" });
        db.korisnici.Add(new Korisnik { Id = "1", emailAdresa = "pretplatnik@test.com", jePretplacenNaNewsletter = true });
        await db.SaveChangesAsync();

        // Pravimo mock konkretne klase EmailSender umjesto interfejsa IEmailSender
        var emailOptions =
    Options.Create(
        new AuthMessageSenderOptions
        {
            FromEmail = "posiljalac@test.com",
            AppPassword = "lozinka123"
        });

var logger =
    Mock.Of<ILogger<EmailSender>>();

var concreteSenderMock =
    new Mock<EmailSender>(
        emailOptions,
        logger);
        
        var controller = new NewsletterController(db, concreteSenderMock.Object);

        var result = await controller.Create(new Poruka { naziv = "Naslov", tekst = "Tekst poruke" });

        Assert.IsType<RedirectToActionResult>(result);
        
        // Provjeravamo da li je newsletter zaista uspješno sačuvan
        Assert.Single(db.poruke);
    }
    [Fact]
    public async Task DeleteConfirmed_NewsletterNotFound_Redirects()
    {
        await using var db = TestContext.Create();
        var controller = new NewsletterController(db, Mock.Of<IEmailSender>());
        // Pokušavamo obrisati ID 999 koji ne postoji, što pokriva else granu
        var result = await controller.DeleteConfirmed(999);
        Assert.IsType<RedirectToActionResult>(result);
    }
}
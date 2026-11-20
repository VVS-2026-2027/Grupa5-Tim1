using Microsoft.AspNetCore.Mvc;
using Moq;
using SakuraWeb.Controllers;
using SakuraWeb.Models;
using SakuraWeb.Tests.Helpers;

namespace SakuraWeb.Tests.Controllers;

public class RezervacijaControllerAdditionalTests
{
    private static Korisnik CreateUser(string id, Uloga role, string email) => new()
    {
        Id = id,
        korisnickoIme = id,
        emailAdresa = email,
        ulogaKorisnika = role
    };

    [Theory]
    [InlineData("Administrator", 2)]
    [InlineData("Frizer", 1)]
    [InlineData("Klijent", 1)]
    public async Task Index_RoleReturnsVisibleReservations(string role, int expectedCount)
    {
        await using var db = TestContext.Create();
        var stylist = CreateUser("stylist", Uloga.Frizer, "stylist@test.com");
        var service = new Usluga { id = 1, naziv = "Šišanje", opis = "Opis", cijena = 10, trajanje = 30 };
        db.korisnici.Add(stylist);
        db.usluge.Add(service);
        db.rezervacije.AddRange(
            new Rezervacija { id = 1, datumRezervacije = DateTime.Today, vrijemeTermina = DateTime.Today.AddHours(9), klijentId = "client", frizerId = "stylist", uslugaId = 1 },
            new Rezervacija { id = 2, datumRezervacije = DateTime.Today, vrijemeTermina = DateTime.Today.AddHours(12), klijentId = "other", frizerId = "stylist", uslugaId = 1 });
        await db.SaveChangesAsync();

        var controller = new RezervacijaController(db, Mock.Of<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>());
        ControllerContextFactory.SetUser(controller, role == "Administrator" ? "admin" : role == "Frizer" ? "stylist" : "client", role);

        var result = await controller.Index();

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(expectedCount, Assert.IsAssignableFrom<List<Rezervacija>>(view.Model).Count);
    }

    [Fact]
    public async Task Otkazi_UnknownReservationReturnsNotFound()
    {
        await using var db = TestContext.Create();
        var controller = new RezervacijaController(db, Mock.Of<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>());
        ControllerContextFactory.SetUser(controller, "client", "Klijent");

        Assert.IsType<NotFoundResult>(await controller.Otkazi(999));
    }

    [Fact]
    public async Task Otkazi_AuthorizedReservationCancelsAndSendsEmails()
    {
        await using var db = TestContext.Create();
        var client = CreateUser("client", Uloga.Klijent, "client@test.com");
        var stylist = CreateUser("stylist", Uloga.Frizer, "stylist@test.com");
        var service = new Usluga { id = 1, naziv = "Šišanje", opis = "Opis", cijena = 10, trajanje = 30 };
        db.korisnici.AddRange(client, stylist);
        db.usluge.Add(service);
        db.rezervacije.Add(new Rezervacija
        {
            id = 1,
            datumRezervacije = DateTime.Today,
            vrijemeTermina = DateTime.Today.AddHours(10),
            klijentId = "client",
            klijent = client,
            frizerId = "stylist",
            frizer = stylist,
            uslugaId = 1,
            usluga = service
        });
        await db.SaveChangesAsync();

        var email = new Mock<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>();
        var controller = new RezervacijaController(db, email.Object);
        ControllerContextFactory.SetUser(controller, "client", "Klijent");

        Assert.IsType<RedirectToActionResult>(await controller.Otkazi(1));
        Assert.True(db.rezervacije.Single().otkazana);
        email.Verify(item => item.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Ocijeni_UnknownReservationReturnsNotFound()
    {
        await using var db = TestContext.Create();
        var controller = new RezervacijaController(db, Mock.Of<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>());
        ControllerContextFactory.SetUser(controller, "client", "Klijent");

        Assert.IsType<NotFoundResult>(await controller.Ocijeni(999, 5));
    }

    [Fact]
    public async Task Ocijeni_OtherUserReturnsForbid()
    {
        await using var db = TestContext.Create();
        db.rezervacije.Add(new Rezervacija { id = 1, klijentId = "owner", ocjena = 0 });
        await db.SaveChangesAsync();
        var controller = new RezervacijaController(db, Mock.Of<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>());
        ControllerContextFactory.SetUser(controller, "other", "Klijent");

        Assert.IsType<ForbidResult>(await controller.Ocijeni(1, 4));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(999)]
    public async Task Details_InvalidIdReturnsNotFound(int? id)
    {
        await using var db = TestContext.Create();
        var controller = new RezervacijaController(db, Mock.Of<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>());

        Assert.IsType<NotFoundResult>(await controller.Details(id));
    }

    [Fact]
    public async Task Details_KnownReservationReturnsView()
    {
        await using var db = TestContext.Create();
        var client = CreateUser("client", Uloga.Klijent, "client@test.com");
        var service = new Usluga { id = 1, naziv = "Šišanje", opis = "Opis", cijena = 10, trajanje = 30 };
        db.korisnici.Add(client);
        db.usluge.Add(service);
        db.rezervacije.Add(new Rezervacija { id = 1, datumRezervacije = DateTime.Today, vrijemeTermina = DateTime.Today.AddHours(10), klijentId = "client", klijent = client, uslugaId = 1, usluga = service });
        await db.SaveChangesAsync();

        var controller = new RezervacijaController(db, Mock.Of<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>());
        var result = await controller.Details(1);

        Assert.Equal(1, Assert.IsType<Rezervacija>(Assert.IsType<ViewResult>(result).Model).id);
    }

    [Fact]
    public async Task Create_GetPopulatesServices()
    {
        await using var db = TestContext.Create();
        db.usluge.AddRange(
            new Usluga { id = 1, naziv = "A", opis = "Opis", cijena = 10, trajanje = 30 },
            new Usluga { id = 2, naziv = "B", opis = "Opis", cijena = 20, trajanje = 60 });
        await db.SaveChangesAsync();
        var controller = new RezervacijaController(db, Mock.Of<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>());

        Assert.IsType<ViewResult>(await controller.Create());
        Assert.NotNull(controller.ViewBag.Usluge);
    }

    [Fact]
    public async Task Create_UnknownStylistReturnsView()
    {
        await using var db = TestContext.Create();
        db.usluge.Add(new Usluga { id = 1, naziv = "Šišanje", opis = "Opis", cijena = 10, trajanje = 30 });
        await db.SaveChangesAsync();
        var controller = new RezervacijaController(db, Mock.Of<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>());
        ControllerContextFactory.SetUser(controller, "client", "Klijent");

        var result = await controller.Create(DateTime.Today, TimeSpan.FromHours(10), 1, "missing");

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Create_OccupiedTermReturnsView()
    {
        await using var db = TestContext.Create();
        var date = DateTime.Today;
        var stylist = CreateUser("stylist", Uloga.Frizer, "stylist@test.com");
        var service = new Usluga { id = 1, naziv = "Šišanje", opis = "Opis", cijena = 10, trajanje = 30 };
        db.korisnici.Add(stylist);
        db.usluge.Add(service);
        db.rezervacije.Add(new Rezervacija { id = 1, datumRezervacije = date, vrijemeTermina = date.AddHours(10), uslugaId = 1, usluga = service });
        await db.SaveChangesAsync();

        var controller = new RezervacijaController(db, Mock.Of<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>());
        ControllerContextFactory.SetUser(controller, "client", "Klijent");

        Assert.IsType<ViewResult>(await controller.Create(date, TimeSpan.FromHours(10), 1, "stylist"));
        Assert.Single(db.rezervacije);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(999)]
    public async Task Edit_GetInvalidIdReturnsNotFound(int? id)
    {
        await using var db = TestContext.Create();
        var controller = new RezervacijaController(db, Mock.Of<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>());

        Assert.IsType<NotFoundResult>(await controller.Edit(id));
    }

    [Fact]
    public async Task Edit_GetKnownReservationReturnsView()
    {
        await using var db = TestContext.Create();
        var client = CreateUser("client", Uloga.Klijent, "client@test.com");
        var stylist = CreateUser("stylist", Uloga.Frizer, "stylist@test.com");
        var service = new Usluga { id = 1, naziv = "Šišanje", opis = "Opis", cijena = 10, trajanje = 30 };
        db.korisnici.AddRange(client, stylist);
        db.usluge.Add(service);
        db.rezervacije.Add(new Rezervacija { id = 1, datumRezervacije = DateTime.Today, vrijemeTermina = DateTime.Today.AddHours(10), klijentId = "client", klijent = client, frizerId = "stylist", frizer = stylist, uslugaId = 1, usluga = service });
        await db.SaveChangesAsync();

        var controller = new RezervacijaController(db, Mock.Of<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>());
        Assert.Equal(1, Assert.IsType<Rezervacija>(Assert.IsType<ViewResult>(await controller.Edit(1)).Model).id);
    }

    [Fact]
    public async Task Edit_PostMovesReservationAndSendsEmail()
    {
        await using var db = TestContext.Create();
        var date = DateTime.Today;
        var client = CreateUser("client", Uloga.Klijent, "client@test.com");
        var stylist = CreateUser("stylist", Uloga.Frizer, "stylist@test.com");
        var service = new Usluga { id = 1, naziv = "Šišanje", opis = "Opis", cijena = 10, trajanje = 30 };
        db.korisnici.AddRange(client, stylist);
        db.usluge.Add(service);
        db.rezervacije.Add(new Rezervacija { id = 1, datumRezervacije = date, vrijemeTermina = date.AddHours(10), klijentId = "client", klijent = client, frizerId = "stylist", frizer = stylist, uslugaId = 1, usluga = service });
        await db.SaveChangesAsync();

        var email = new Mock<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>();
        var controller = new RezervacijaController(db, email.Object);

        Assert.IsType<RedirectToActionResult>(await controller.Edit(1, date.AddDays(1), TimeSpan.FromHours(11)));
        email.Verify(item => item.SendEmailAsync("client@test.com", "[Sakura] Pomjerena rezervacija", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Edit_PostUnknownReservationReturnsNotFound()
    {
        await using var db = TestContext.Create();
        var controller = new RezervacijaController(db, Mock.Of<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>());

        Assert.IsType<NotFoundResult>(await controller.Edit(999, DateTime.Today, TimeSpan.FromHours(10)));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(999)]
    public async Task Delete_InvalidIdReturnsNotFound(int? id)
    {
        await using var db = TestContext.Create();
        var controller = new RezervacijaController(db, Mock.Of<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>());

        Assert.IsType<NotFoundResult>(await controller.Delete(id));
    }

    [Fact]
    public async Task Delete_KnownReservationReturnsView()
    {
        await using var db = TestContext.Create();
        db.rezervacije.Add(new Rezervacija { id = 1, datumRezervacije = DateTime.Today, vrijemeTermina = DateTime.Today.AddHours(10) });
        await db.SaveChangesAsync();
        var controller = new RezervacijaController(db, Mock.Of<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>());

        Assert.Equal(1, Assert.IsType<Rezervacija>(Assert.IsType<ViewResult>(await controller.Delete(1)).Model).id);
    }

    [Fact]
    public async Task DeleteConfirmed_RemovesReservation()
    {
        await using var db = TestContext.Create();
        db.rezervacije.Add(new Rezervacija { id = 1, datumRezervacije = DateTime.Today, vrijemeTermina = DateTime.Today.AddHours(10) });
        await db.SaveChangesAsync();
        var controller = new RezervacijaController(db, Mock.Of<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>());

        Assert.IsType<RedirectToActionResult>(await controller.DeleteConfirmed(1));
        Assert.Empty(db.rezervacije);
    }

    [Fact]
    public async Task DeleteConfirmed_UnknownReservationRedirects()
    {
        await using var db = TestContext.Create();
        var controller = new RezervacijaController(db, Mock.Of<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>());

        Assert.IsType<RedirectToActionResult>(await controller.DeleteConfirmed(999));
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(999, false)]
    public async Task RezervacijaExists_PrivateHelperReturnsDatabaseMatch(int id, bool expected)
    {
        await using var db = TestContext.Create();
        db.rezervacije.Add(new Rezervacija { id = 1, datumRezervacije = DateTime.Today, vrijemeTermina = DateTime.Today.AddHours(10) });
        await db.SaveChangesAsync();
        var controller = new RezervacijaController(db, Mock.Of<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>());
        var method = typeof(RezervacijaController).GetMethod("RezervacijaExists", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        Assert.Equal(expected, Assert.IsType<bool>(method!.Invoke(controller, new object[] { id })));
    }
}

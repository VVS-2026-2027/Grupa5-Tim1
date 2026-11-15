using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SakuraWeb.Controllers;
using SakuraWeb.Models;
using SakuraWeb.Tests.Helpers;

namespace SakuraWeb.Tests.Controllers;

public class RezervacijaControllerTests
{
    private static Korisnik CreateUser(
        string id,
        Uloga role,
        string email)
    {
        return new Korisnik
        {
            Id = id,
            korisnickoIme = id,
            emailAdresa = email,
            ulogaKorisnika = role
        };
    }

    [Fact]
    public async Task Create_ValidReservation_Saves()
    {
        await using var db = TestContext.Create();

        var client =
            CreateUser(
                "client",
                Uloga.Klijent,
                "client@test.com");

        var stylist =
            CreateUser(
                "stylist",
                Uloga.Frizer,
                "stylist@test.com");

        db.korisnici.AddRange(client, stylist);

        db.usluge.Add(
            new Usluga
            {
                id = 1,
                naziv = "Šišanje",
                opis = "Opis",
                cijena = 10,
                trajanje = 30
            });

        await db.SaveChangesAsync();

        var email = new Mock<IEmailSender>();

        var controller =
            new RezervacijaController(
                db,
                email.Object);

        ControllerContextFactory.SetUser(
            controller,
            "client",
            "Klijent");

        var result =
            await controller.Create(
                new DateTime(2026, 11, 16),
                new TimeSpan(10, 0, 0),
                1,
                "stylist");

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Single(db.rezervacije);

        email.Verify(
            item => item.SendEmailAsync(
                "client@test.com",
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task Create_UnknownService_ReturnsView()
    {
        await using var db = TestContext.Create();

        var controller =
            new RezervacijaController(
                db,
                Mock.Of<IEmailSender>());

        ControllerContextFactory.SetUser(
            controller,
            "client",
            "Klijent");

        var result =
            await controller.Create(
                DateTime.Today,
                TimeSpan.FromHours(10),
                999,
                "stylist");

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
    }

    [Fact]
    public async Task Create_TermOutsideWorkingHours_ReturnsView()
    {
        await using var db = TestContext.Create();

        db.usluge.Add(
            new Usluga
            {
                id = 1,
                naziv = "Duga usluga",
                opis = "Opis",
                cijena = 10,
                trajanje = 60
            });

        db.korisnici.Add(
            CreateUser(
                "stylist",
                Uloga.Frizer,
                "stylist@test.com"));

        await db.SaveChangesAsync();

        var controller =
            new RezervacijaController(
                db,
                Mock.Of<IEmailSender>());

        ControllerContextFactory.SetUser(
            controller,
            "client",
            "Klijent");

        var result =
            await controller.Create(
                DateTime.Today,
                new TimeSpan(16, 30, 0),
                1,
                "stylist");

        Assert.IsType<ViewResult>(result);
        Assert.Empty(db.rezervacije);
    }

    [Fact]
    public async Task Ocijeni_ValidRating_IsSaved()
    {
        await using var db = TestContext.Create();

        db.rezervacije.Add(
            new Rezervacija
            {
                id = 1,
                klijentId = "client",
                datumRezervacije = DateTime.Today,
                vrijemeTermina =
                    DateTime.Today.AddHours(10),
                uslugaId = 1
            });

        await db.SaveChangesAsync();

        var controller =
            new RezervacijaController(
                db,
                Mock.Of<IEmailSender>());

        ControllerContextFactory.SetUser(
            controller,
            "client",
            "Klijent");

        var result =
            await controller.Ocijeni(1, 5);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(5, db.rezervacije.Single().ocjena);
    }

    [Fact]
    public async Task Ocijeni_InvalidRating_DoesNotSave()
    {
        await using var db = TestContext.Create();

        db.rezervacije.Add(
            new Rezervacija
            {
                id = 1,
                klijentId = "client",
                ocjena = 0
            });

        await db.SaveChangesAsync();

        var controller =
            new RezervacijaController(
                db,
                Mock.Of<IEmailSender>());

        ControllerContextFactory.SetUser(
            controller,
            "client",
            "Klijent");

        await controller.Ocijeni(1, 6);

        Assert.Equal(0, db.rezervacije.Single().ocjena);
    }

    [Fact]
    public async Task Otkazi_OtherUser_ReturnsForbid()
    {
        await using var db = TestContext.Create();

        var owner =
            CreateUser(
                "owner",
                Uloga.Klijent,
                "owner@test.com");

        var stylist =
            CreateUser(
                "stylist",
                Uloga.Frizer,
                "stylist@test.com");

        var service =
            new Usluga
            {
                id = 1,
                naziv = "Šišanje",
                opis = "Opis",
                cijena = 10,
                trajanje = 30
            };

        db.korisnici.AddRange(owner, stylist);
        db.usluge.Add(service);

        db.rezervacije.Add(
            new Rezervacija
            {
                id = 1,
                klijentId = "owner",
                klijent = owner,
                frizerId = "stylist",
                frizer = stylist,
                uslugaId = 1,
                usluga = service,
                datumRezervacije = DateTime.Today,
                vrijemeTermina =
                    DateTime.Today.AddHours(10)
            });

        await db.SaveChangesAsync();

        var controller =
            new RezervacijaController(
                db,
                Mock.Of<IEmailSender>());

        ControllerContextFactory.SetUser(
            controller,
            "other",
            "Klijent");

        var result = await controller.Otkazi(1);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task ZauzetiTermini_ReturnsAllSlots()
    {
        await using var db = TestContext.Create();

        var date = new DateTime(2026, 11, 16);

        var service =
            new Usluga
            {
                id = 1,
                naziv = "Duga usluga",
                opis = "Opis",
                cijena = 10,
                trajanje = 60
            };

        db.usluge.Add(service);

        db.rezervacije.Add(
            new Rezervacija
            {
                id = 1,
                datumRezervacije = date,
                vrijemeTermina =
                    date.AddHours(10),
                uslugaId = 1,
                usluga = service,
                otkazana = false
            });

        await db.SaveChangesAsync();

        var controller =
            new RezervacijaController(
                db,
                Mock.Of<IEmailSender>());

        var result =
            await controller.ZauzetiTermini(date);

        var json = Assert.IsType<JsonResult>(result);

        var slots =
            Assert.IsAssignableFrom<HashSet<string>>(
                json.Value);

        Assert.Contains("10:00", slots);
        Assert.Contains("10:30", slots);
    }
}
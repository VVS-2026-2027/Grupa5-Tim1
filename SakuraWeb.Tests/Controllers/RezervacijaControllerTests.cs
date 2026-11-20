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

    [Theory]
    [InlineData("client", "Klijent", 1)]
    [InlineData("stylist", "Frizer", 1)]
    [InlineData("admin", "Administrator", 2)]
    public async Task Index_ReturnsReservationsForCurrentRole(
        string userId,
        string role,
        int expectedCount)
    {
        await using var db = TestContext.Create();
        var firstClient = CreateUser("client", Uloga.Klijent, "client@test.com");
        var secondClient = CreateUser("other-client", Uloga.Klijent, "other@test.com");
        var stylist = CreateUser("stylist", Uloga.Frizer, "stylist@test.com");
        var secondStylist = CreateUser("other-stylist", Uloga.Frizer, "other-stylist@test.com");
        db.korisnici.AddRange(firstClient, secondClient, stylist, secondStylist);
        db.usluge.Add(new Usluga { id = 1, naziv = "Sisanje", opis = "Opis", cijena = 10, trajanje = 30 });
        db.rezervacije.AddRange(
            new Rezervacija { id = 1, klijentId = firstClient.Id, klijent = firstClient, frizerId = stylist.Id, frizer = stylist, uslugaId = 1, datumRezervacije = DateTime.Today, vrijemeTermina = DateTime.Today.AddHours(10) },
            new Rezervacija { id = 2, klijentId = secondClient.Id, klijent = secondClient, frizerId = secondStylist.Id, frizer = secondStylist, uslugaId = 1, datumRezervacije = DateTime.Today, vrijemeTermina = DateTime.Today.AddHours(11) });
        await db.SaveChangesAsync();

        var controller = new RezervacijaController(db, Mock.Of<IEmailSender>());
        ControllerContextFactory.SetUser(controller, userId, role);

        var result = Assert.IsType<ViewResult>(await controller.Index());
        var reservations = Assert.IsAssignableFrom<List<Rezervacija>>(result.Model);

        Assert.Equal(expectedCount, reservations.Count);
        Assert.Contains(reservations, reservation => reservation.id == 1);
    }

    [Fact]
    public async Task Create_Get_ReturnsViewWithServicesOrderedByCategoryAndName()
    {
        await using var db = TestContext.Create();
        db.usluge.AddRange(
            new Usluga { id = 1, naziv = "Zadnja", opis = "Opis", cijena = 10, trajanje = 30 },
            new Usluga { id = 2, naziv = "Prva", opis = "Opis", cijena = 10, trajanje = 30 });
        await db.SaveChangesAsync();
        var controller = new RezervacijaController(db, Mock.Of<IEmailSender>());

        var result = Assert.IsType<ViewResult>(await controller.Create());

        var services = (List<Usluga>)controller.ViewBag.Usluge;
        Assert.Equal(new[] { "Prva", "Zadnja" }, services.Select(service => service.naziv));
        Assert.Null(result.Model);
    }

    [Theory]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    public async Task Create_CancelledOrDifferentDayReservation_DoesNotBlockSlot(
        bool existingReservationCancelled,
        int existingReservationDayOffset)
    {
        await using var db = TestContext.Create();
        var client = CreateUser("client", Uloga.Klijent, "client@test.com");
        var stylist = CreateUser("stylist", Uloga.Frizer, "stylist@test.com");
        var service = new Usluga { id = 1, naziv = "Sisanje", opis = "Opis", cijena = 10, trajanje = 30 };
        db.korisnici.AddRange(client, stylist);
        db.usluge.Add(service);
        var reservationDate = new DateTime(2026, 11, 20);
        db.rezervacije.Add(new Rezervacija
        {
            id = 1,
            klijentId = client.Id,
            klijent = client,
            frizerId = stylist.Id,
            frizer = stylist,
            uslugaId = service.id,
            usluga = service,
            datumRezervacije = reservationDate.AddDays(existingReservationDayOffset),
            vrijemeTermina = reservationDate.AddHours(10),
            otkazana = existingReservationCancelled
        });
        await db.SaveChangesAsync();
        var controller = new RezervacijaController(db, Mock.Of<IEmailSender>());
        ControllerContextFactory.SetUser(controller, client.Id, "Klijent");

        var result = await controller.Create(reservationDate, TimeSpan.FromHours(10), service.id, stylist.Id);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(2, db.rezervacije.Count());
        Assert.Contains(db.rezervacije, reservation => reservation.id != 1 && !reservation.otkazana);
    }

    [Fact]
    public async Task Create_ReservationOverlapsSecondRequiredSlot_ReturnsView()
    {
        await using var db = TestContext.Create();
        var client = CreateUser("client", Uloga.Klijent, "client@test.com");
        var stylist = CreateUser("stylist", Uloga.Frizer, "stylist@test.com");
        var requestedService = new Usluga { id = 1, naziv = "Duga usluga", opis = "Opis", cijena = 20, trajanje = 60 };
        var existingService = new Usluga { id = 2, naziv = "Kratka usluga", opis = "Opis", cijena = 10, trajanje = 30 };
        db.korisnici.AddRange(client, stylist);
        db.usluge.AddRange(requestedService, existingService);
        var date = new DateTime(2026, 11, 20);
        db.rezervacije.Add(new Rezervacija
        {
            id = 1,
            klijentId = client.Id,
            klijent = client,
            frizerId = stylist.Id,
            frizer = stylist,
            uslugaId = existingService.id,
            usluga = existingService,
            datumRezervacije = date,
            vrijemeTermina = date.AddHours(10).AddMinutes(30)
        });
        await db.SaveChangesAsync();
        var controller = new RezervacijaController(db, Mock.Of<IEmailSender>());
        ControllerContextFactory.SetUser(controller, client.Id, "Klijent");

        var result = await controller.Create(date, TimeSpan.FromHours(10), requestedService.id, stylist.Id);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Single(db.rezervacije);
        Assert.Contains(controller.ModelState[string.Empty]!.Errors, error => error.ErrorMessage == "Odabrani termin je zauzet. Molimo odaberite drugi termin.");
    }

    [Fact]
    public async Task Create_UnknownStylist_ReturnsViewWithModelError()
    {
        await using var db = TestContext.Create();
        db.usluge.Add(new Usluga { id = 1, naziv = "Sisanje", opis = "Opis", cijena = 10, trajanje = 30 });
        await db.SaveChangesAsync();
        var controller = new RezervacijaController(db, Mock.Of<IEmailSender>());
        ControllerContextFactory.SetUser(controller, "client", "Klijent");

        var result = await controller.Create(DateTime.Today, TimeSpan.FromHours(10), 1, "missing-stylist");

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains(controller.ModelState[string.Empty]!.Errors, error => error.ErrorMessage == "Odabrani frizer ne postoji.");
    }

    [Fact]
    public async Task Create_ExistingUserThatIsNotStylist_ReturnsViewWithModelError()
    {
        await using var db = TestContext.Create();
        db.usluge.Add(new Usluga { id = 1, naziv = "Sisanje", opis = "Opis", cijena = 10, trajanje = 30 });
        db.korisnici.Add(CreateUser("not-stylist", Uloga.Klijent, "user@test.com"));
        await db.SaveChangesAsync();
        var controller = new RezervacijaController(db, Mock.Of<IEmailSender>());
        ControllerContextFactory.SetUser(controller, "client", "Klijent");

        var result = await controller.Create(DateTime.Today, TimeSpan.FromHours(10), 1, "not-stylist");

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Contains(controller.ModelState[string.Empty]!.Errors, error => error.ErrorMessage == "Odabrani frizer ne postoji.");
    }

    [Fact]
    public async Task Create_OverlappingReservation_ReturnsViewWithoutCreatingAnotherReservation()
    {
        await using var db = TestContext.Create();
        var client = CreateUser("client", Uloga.Klijent, "client@test.com");
        var stylist = CreateUser("stylist", Uloga.Frizer, "stylist@test.com");
        var service = new Usluga { id = 1, naziv = "Duga usluga", opis = "Opis", cijena = 10, trajanje = 60 };
        db.korisnici.AddRange(client, stylist);
        db.usluge.Add(service);
        var date = new DateTime(2026, 11, 20);
        db.rezervacije.Add(new Rezervacija
        {
            id = 1,
            klijentId = client.Id,
            klijent = client,
            frizerId = stylist.Id,
            frizer = stylist,
            uslugaId = service.id,
            usluga = service,
            datumRezervacije = date,
            vrijemeTermina = date.AddHours(10)
        });
        await db.SaveChangesAsync();
        var controller = new RezervacijaController(db, Mock.Of<IEmailSender>());
        ControllerContextFactory.SetUser(controller, client.Id, "Klijent");

        var result = await controller.Create(date, new TimeSpan(10, 30, 0), service.id, stylist.Id);

        Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.Equal(1, db.rezervacije.Count());
    }

    [Fact]
    public async Task Otkazi_UnknownReservation_ReturnsNotFound()
    {
        await using var db = TestContext.Create();
        var email = new Mock<IEmailSender>();
        var controller = new RezervacijaController(db, email.Object);

        var result = await controller.Otkazi(404);

        Assert.IsType<NotFoundResult>(result);
        email.Verify(sender => sender.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Otkazi_ClientReservation_CancelsAndNotifiesClientAndStylist()
    {
        await using var db = TestContext.Create();
        var client = CreateUser("client", Uloga.Klijent, "client@test.com");
        var stylist = CreateUser("stylist", Uloga.Frizer, "stylist@test.com");
        var service = new Usluga { id = 1, naziv = "Sisanje", opis = "Opis", cijena = 10, trajanje = 30 };
        db.korisnici.AddRange(client, stylist);
        db.usluge.Add(service);
        db.rezervacije.Add(new Rezervacija
        {
            id = 1,
            klijentId = client.Id,
            klijent = client,
            frizerId = stylist.Id,
            frizer = stylist,
            uslugaId = service.id,
            usluga = service,
            datumRezervacije = new DateTime(2026, 11, 20),
            vrijemeTermina = new DateTime(2026, 11, 20, 10, 30, 0)
        });
        await db.SaveChangesAsync();

        var email = new Mock<IEmailSender>();
        var controller = new RezervacijaController(db, email.Object);
        ControllerContextFactory.SetUser(controller, client.Id, "Klijent");

        var result = await controller.Otkazi(1);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.True(db.rezervacije.Single().otkazana);
        email.Verify(sender => sender.SendEmailAsync(client.emailAdresa, "[Sakura] Otkazana rezervacija", It.Is<string>(body => body.Contains("20.11.2026.") && body.Contains("10:30"))), Times.Once);
        email.Verify(sender => sender.SendEmailAsync(stylist.emailAdresa, "[Sakura] Otkazana rezervacija", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Otkazi_ReservationWithoutStylist_NotifiesOnlyClient()
    {
        await using var db = TestContext.Create();
        var client = CreateUser("client", Uloga.Klijent, "client@test.com");
        var service = new Usluga { id = 1, naziv = "Sisanje", opis = "Opis", cijena = 10, trajanje = 30 };
        db.korisnici.Add(client);
        db.usluge.Add(service);
        db.rezervacije.Add(new Rezervacija
        {
            id = 1,
            klijentId = client.Id,
            klijent = client,
            uslugaId = service.id,
            usluga = service,
            datumRezervacije = new DateTime(2026, 11, 20),
            vrijemeTermina = new DateTime(2026, 11, 20, 10, 0, 0)
        });
        await db.SaveChangesAsync();
        var email = new Mock<IEmailSender>();
        var controller = new RezervacijaController(db, email.Object);
        ControllerContextFactory.SetUser(controller, client.Id, "Klijent");

        var result = await controller.Otkazi(1);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.True(db.rezervacije.Single().otkazana);
        email.Verify(sender => sender.SendEmailAsync(client.emailAdresa, "[Sakura] Otkazana rezervacija", It.Is<string>(body => body.Contains("Nije dodijeljen"))), Times.Once);
        email.Verify(sender => sender.SendEmailAsync(It.Is<string>(address => address != client.emailAdresa), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Ocijeni_UnknownReservation_ReturnsNotFound()
    {
        await using var db = TestContext.Create();
        var controller = new RezervacijaController(db, Mock.Of<IEmailSender>());

        var result = await controller.Ocijeni(404, 5);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Ocijeni_OtherClient_ReturnsForbidWithoutChangingRating()
    {
        await using var db = TestContext.Create();
        db.rezervacije.Add(new Rezervacija { id = 1, klijentId = "owner", ocjena = 0 });
        await db.SaveChangesAsync();
        var controller = new RezervacijaController(db, Mock.Of<IEmailSender>());
        ControllerContextFactory.SetUser(controller, "other", "Klijent");

        var result = await controller.Ocijeni(1, 4);

        Assert.IsType<ForbidResult>(result);
        Assert.Equal(0, db.rezervacije.Single().ocjena);
    }

    [Fact]
    public async Task Details_NullOrUnknownId_ReturnsNotFound()
    {
        await using var db = TestContext.Create();
        var controller = new RezervacijaController(db, Mock.Of<IEmailSender>());

        Assert.IsType<NotFoundResult>(await controller.Details(null));
        Assert.IsType<NotFoundResult>(await controller.Details(404));
    }

    [Fact]
    public async Task Details_ExistingReservation_ReturnsReservationView()
    {
        await using var db = TestContext.Create();
        var client = CreateUser("client", Uloga.Klijent, "client@test.com");
        var service = new Usluga { id = 1, naziv = "Sisanje", opis = "Opis", cijena = 10, trajanje = 30 };
        db.korisnici.Add(client);
        db.usluge.Add(service);
        db.rezervacije.Add(new Rezervacija
        {
            id = 1,
            klijentId = client.Id,
            klijent = client,
            uslugaId = service.id,
            usluga = service,
            datumRezervacije = DateTime.Today
        });
        await db.SaveChangesAsync();
        var controller = new RezervacijaController(db, Mock.Of<IEmailSender>());

        var result = Assert.IsType<ViewResult>(await controller.Details(1));

        Assert.Equal(1, Assert.IsType<Rezervacija>(result.Model).id);
    }

    [Fact]
    public async Task Edit_Get_NullOrUnknownId_ReturnsNotFound()
    {
        await using var db = TestContext.Create();
        var controller = new RezervacijaController(db, Mock.Of<IEmailSender>());

        Assert.IsType<NotFoundResult>(await controller.Edit(null));
        Assert.IsType<NotFoundResult>(await controller.Edit(404));
    }

    [Fact]
    public async Task Edit_Get_ExistingReservation_ReturnsView()
    {
        await using var db = TestContext.Create();
        var client = CreateUser("client", Uloga.Klijent, "client@test.com");
        var service = new Usluga { id = 1, naziv = "Sisanje", opis = "Opis", cijena = 10, trajanje = 30 };
        db.korisnici.Add(client);
        db.usluge.Add(service);
        db.rezervacije.Add(new Rezervacija { id = 1, klijentId = client.Id, klijent = client, uslugaId = service.id, usluga = service });
        await db.SaveChangesAsync();
        var controller = new RezervacijaController(db, Mock.Of<IEmailSender>());

        var result = Assert.IsType<ViewResult>(await controller.Edit(1));

        Assert.Equal(1, Assert.IsType<Rezervacija>(result.Model).id);
    }

    [Fact]
    public async Task Edit_Post_UpdatesReservationAndNotifiesClient()
    {
        await using var db = TestContext.Create();
        var client = CreateUser("client", Uloga.Klijent, "client@test.com");
        var stylist = CreateUser("stylist", Uloga.Frizer, "stylist@test.com");
        var service = new Usluga { id = 1, naziv = "Sisanje", opis = "Opis", cijena = 10, trajanje = 30 };
        var oldDate = new DateTime(2026, 11, 20);
        db.korisnici.AddRange(client, stylist);
        db.usluge.Add(service);
        db.rezervacije.Add(new Rezervacija
        {
            id = 1, klijentId = client.Id, klijent = client, frizerId = stylist.Id, frizer = stylist,
            uslugaId = service.id, usluga = service, datumRezervacije = oldDate,
            vrijemeTermina = oldDate.AddHours(10)
        });
        await db.SaveChangesAsync();

        var email = new Mock<IEmailSender>();
        var controller = new RezervacijaController(db, email.Object);
        var newDate = oldDate.AddDays(1);

        var result = await controller.Edit(1, newDate, new TimeSpan(11, 30, 0));

        Assert.IsType<RedirectToActionResult>(result);
        var updated = db.rezervacije.Single();
        Assert.Equal(newDate, updated.datumRezervacije);
        Assert.Equal(newDate.AddHours(11).AddMinutes(30), updated.vrijemeTermina);
        email.Verify(sender => sender.SendEmailAsync(client.emailAdresa, "[Sakura] Pomjerena rezervacija", It.Is<string>(body => body.Contains("pomjerena na datum"))), Times.Once);
    }

    [Fact]
    public async Task Edit_Post_UnchangedDateAndTime_DoesNotSendNotification()
    {
        await using var db = TestContext.Create();
        var client = CreateUser("client", Uloga.Klijent, "client@test.com");
        var service = new Usluga { id = 1, naziv = "Sisanje", opis = "Opis", cijena = 10, trajanje = 30 };
        var date = new DateTime(2026, 11, 20);
        db.korisnici.Add(client);
        db.usluge.Add(service);
        db.rezervacije.Add(new Rezervacija
        {
            id = 1,
            klijentId = client.Id,
            klijent = client,
            uslugaId = service.id,
            usluga = service,
            datumRezervacije = date,
            vrijemeTermina = date.AddHours(10)
        });
        await db.SaveChangesAsync();
        var email = new Mock<IEmailSender>();
        var controller = new RezervacijaController(db, email.Object);

        var result = await controller.Edit(1, date, TimeSpan.FromHours(10));

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(date, db.rezervacije.Single().datumRezervacije);
        Assert.Equal(date.AddHours(10), db.rezervacije.Single().vrijemeTermina);
        email.Verify(sender => sender.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Edit_Post_UnknownReservation_ReturnsNotFound()
    {
        await using var db = TestContext.Create();
        var controller = new RezervacijaController(db, Mock.Of<IEmailSender>());

        var result = await controller.Edit(404, DateTime.Today, TimeSpan.FromHours(10));

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Get_NullOrUnknownId_ReturnsNotFound()
    {
        await using var db = TestContext.Create();
        var controller = new RezervacijaController(db, Mock.Of<IEmailSender>());

        Assert.IsType<NotFoundResult>(await controller.Delete(null));
        Assert.IsType<NotFoundResult>(await controller.Delete(404));
    }

    [Fact]
    public async Task Delete_Get_ExistingReservation_ReturnsView()
    {
        await using var db = TestContext.Create();
        var client = CreateUser("client", Uloga.Klijent, "client@test.com");
        var service = new Usluga { id = 1, naziv = "Sisanje", opis = "Opis", cijena = 10, trajanje = 30 };
        db.korisnici.Add(client);
        db.usluge.Add(service);
        db.rezervacije.Add(new Rezervacija { id = 1, klijentId = client.Id, klijent = client, uslugaId = service.id, usluga = service });
        await db.SaveChangesAsync();
        var controller = new RezervacijaController(db, Mock.Of<IEmailSender>());

        var result = Assert.IsType<ViewResult>(await controller.Delete(1));

        Assert.Equal(1, Assert.IsType<Rezervacija>(result.Model).id);
    }

    [Fact]
    public async Task DeleteConfirmed_ExistingReservation_RemovesIt()
    {
        await using var db = TestContext.Create();
        db.rezervacije.Add(new Rezervacija { id = 1, klijentId = "client" });
        await db.SaveChangesAsync();
        var controller = new RezervacijaController(db, Mock.Of<IEmailSender>());

        var result = await controller.DeleteConfirmed(1);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Empty(db.rezervacije);
    }
}

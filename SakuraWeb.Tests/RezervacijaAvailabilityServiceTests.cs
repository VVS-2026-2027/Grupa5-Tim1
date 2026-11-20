using SakuraWeb.Models;
using SakuraWeb.Services;

namespace SakuraWeb.Tests;

public class RezervacijaAvailabilityServiceTests
{
    private readonly RezervacijaAvailabilityService _service = new();

    [Fact]
    public void Check_ReturnsUnavailable_WhenSameFrizerHasOverlappingReservation()
    {
        var usluga =
            new Usluga(
                "Šišanje",
                20,
                KategorijaUsluga.Šišanje,
                60);

        var postojece = new[]
        {
            new Rezervacija
            {
                datumRezervacije =
                    new DateTime(2026, 11, 18),

                vrijemeTermina =
                    new DateTime(2026, 11, 18, 10, 0, 0),

                frizerId = "frizer-1",
                usluga = usluga,
                otkazana = false
            }
        };

        var rezultat =
            _service.Check(
                new DateTime(2026, 11, 18),
                new TimeSpan(10, 30, 0),
                usluga,
                "frizer-1",
                postojece);

        Assert.False(rezultat.IsAvailable);

        Assert.Equal(
            "Odabrani termin je zauzet. Molimo odaberite drugi termin.",
            rezultat.ErrorMessage);
    }

    [Fact]
    public void Check_AllowsSameTime_WhenReservationBelongsToAnotherFrizer()
    {
        var usluga =
            new Usluga(
                "Šišanje",
                20,
                KategorijaUsluga.Šišanje,
                60);

        var postojece = new[]
        {
            new Rezervacija
            {
                datumRezervacije =
                    new DateTime(2026, 11, 18),

                vrijemeTermina =
                    new DateTime(2026, 11, 18, 10, 0, 0),

                frizerId = "frizer-2",
                usluga = usluga,
                otkazana = false
            }
        };

        var rezultat =
            _service.Check(
                new DateTime(2026, 11, 18),
                new TimeSpan(10, 30, 0),
                usluga,
                "frizer-1",
                postojece);

        Assert.True(rezultat.IsAvailable);
        Assert.Null(rezultat.ErrorMessage);
    }

    [Fact]
    public void Check_ReturnsUnavailable_WhenServiceDoesNotFitWorkingHours()
    {
        var usluga =
            new Usluga(
                "Farbanje",
                50,
                KategorijaUsluga.Farbanje,
                120);

        var rezultat =
            _service.Check(
                new DateTime(2026, 11, 18),
                new TimeSpan(16, 0, 0),
                usluga,
                "frizer-1",
                Array.Empty<Rezervacija>());

        Assert.False(rezultat.IsAvailable);

        Assert.Equal(
            "Odabrana usluga ne staje u radno vrijeme za odabrani termin.",
            rezultat.ErrorMessage);
    }

    [Fact]
    public void Check_AllowsSlot_WhenExistingReservationIsCancelled()
    {
        var usluga = new Usluga("Šišanje", 20, KategorijaUsluga.Šišanje, 30);
        var postojece = new[]
        {
            new Rezervacija
            {
                datumRezervacije = new DateTime(2026, 11, 18),
                vrijemeTermina = new DateTime(2026, 11, 18, 10, 0, 0),
                frizerId = "frizer-1",
                usluga = usluga,
                otkazana = true
            }
        };

        var rezultat = _service.Check(
            new DateTime(2026, 11, 18),
            new TimeSpan(10, 0, 0),
            usluga,
            "frizer-1",
            postojece);

        Assert.True(rezultat.IsAvailable);
        Assert.Null(rezultat.ErrorMessage);
    }

    [Fact]
    public void Check_AllowsSlot_WhenExistingReservationIsOnAnotherDate()
    {
        var usluga = new Usluga("Šišanje", 20, KategorijaUsluga.Šišanje, 30);
        var postojece = new[]
        {
            new Rezervacija
            {
                datumRezervacije = new DateTime(2026, 11, 19),
                vrijemeTermina = new DateTime(2026, 11, 19, 10, 0, 0),
                frizerId = "frizer-1",
                usluga = usluga
            }
        };

        var rezultat = _service.Check(
            new DateTime(2026, 11, 18),
            new TimeSpan(10, 0, 0),
            usluga,
            "frizer-1",
            postojece);

        Assert.True(rezultat.IsAvailable);
        Assert.Null(rezultat.ErrorMessage);
    }

    [Fact]
    public void Check_ReturnsUnavailable_WhenLongerServiceOverlapsLaterSlot()
    {
        var newService = new Usluga("Farbanje", 50, KategorijaUsluga.Farbanje, 60);
        var existingService = new Usluga("Šišanje", 20, KategorijaUsluga.Šišanje, 30);
        var postojece = new[]
        {
            new Rezervacija
            {
                datumRezervacije = new DateTime(2026, 11, 18),
                vrijemeTermina = new DateTime(2026, 11, 18, 10, 30, 0),
                frizerId = "frizer-1",
                usluga = existingService
            }
        };

        var rezultat = _service.Check(
            new DateTime(2026, 11, 18),
            new TimeSpan(10, 0, 0),
            newService,
            "frizer-1",
            postojece);

        Assert.False(rezultat.IsAvailable);
        Assert.Equal(
            "Odabrani termin je zauzet. Molimo odaberite drugi termin.",
            rezultat.ErrorMessage);
    }

    [Fact]
    public void Check_AllowsSlot_WhenServiceStartsAtLatestAllowedTime()
    {
        var usluga = new Usluga("Šišanje", 20, KategorijaUsluga.Šišanje, 30);

        var rezultat = _service.Check(
            new DateTime(2026, 11, 18),
            new TimeSpan(16, 30, 0),
            usluga,
            "frizer-1",
            Array.Empty<Rezervacija>());

        Assert.True(rezultat.IsAvailable);
        Assert.Null(rezultat.ErrorMessage);
    }

    [Fact]
    public void Check_ReturnsUnavailable_WhenServiceStartsAfterLatestAllowedTime()
    {
        var usluga = new Usluga("Šišanje", 20, KategorijaUsluga.Šišanje, 30);

        var rezultat = _service.Check(
            new DateTime(2026, 11, 18),
            new TimeSpan(17, 0, 0),
            usluga,
            "frizer-1",
            Array.Empty<Rezervacija>());

        Assert.False(rezultat.IsAvailable);
        Assert.Equal(
            "Odabrana usluga ne staje u radno vrijeme za odabrani termin.",
            rezultat.ErrorMessage);
    }

    [Fact]
    public void Check_ReturnsUnavailable_WhenStartTimeIsNotOnHalfHourBoundary()
    {
        var usluga = new Usluga("Šišanje", 20, KategorijaUsluga.Šišanje, 30);

        var rezultat = _service.Check(
            new DateTime(2026, 11, 18),
            new TimeSpan(10, 15, 0),
            usluga,
            "frizer-1",
            Array.Empty<Rezervacija>());

        Assert.False(rezultat.IsAvailable);
        Assert.Equal(
            "Termin mora početi na puni sat ili pola sata.",
            rezultat.ErrorMessage);
    }

    [Fact]
    public void Check_ReturnsUnavailable_WhenStartTimeIsBeforeWorkingHours()
    {
        var usluga = new Usluga("Šišanje", 20, KategorijaUsluga.Šišanje, 30);

        var rezultat = _service.Check(
            new DateTime(2026, 11, 18),
            new TimeSpan(8, 30, 0),
            usluga,
            "frizer-1",
            Array.Empty<Rezervacija>());

        Assert.False(rezultat.IsAvailable);
        Assert.Equal(
            "Odabrana usluga ne staje u radno vrijeme za odabrani termin.",
            rezultat.ErrorMessage);
    }
}

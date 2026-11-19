using SakuraWeb.Models;

namespace SakuraWeb.Tests.Models;

public sealed class RezervacijaTests
{
    [Fact]
    public void DefaultConstructor_InitializesDefaults()
    {
        var rezervacija = new Rezervacija();

        Assert.Equal(-1, rezervacija.id);
        Assert.Equal(default, rezervacija.datumRezervacije);
        Assert.Equal(default, rezervacija.vrijemeTermina);
        Assert.False(rezervacija.otkazana);
        Assert.Equal(0, rezervacija.ocjena);
        Assert.Equal(string.Empty, rezervacija.klijentId);
        Assert.Null(rezervacija.klijent);
        Assert.Null(rezervacija.frizerId);
        Assert.Null(rezervacija.frizer);
        Assert.Equal(0, rezervacija.uslugaId);
        Assert.Null(rezervacija.usluga);
    }

    [Theory]
    [MemberData(nameof(JsonCases))]
    public void Constructor_CopiesReservationValues(ModelConstructionCase testCase)
    {
        var klijent = new Korisnik { korisnickoIme = "Klijent" };
        var frizer = testCase.FrizerId is null
            ? null
            : new Korisnik { korisnickoIme = "Frizer" };
        var usluga = new Usluga("Sisanje", 20, KategorijaUsluga.Šišanje, 60);

        var rezervacija = new Rezervacija(
            testCase.RezervacijaId,
            testCase.DatumRezervacije,
            testCase.VrijemeTermina,
            testCase.Otkazana,
            testCase.KlijentId,
            klijent,
            testCase.FrizerId,
            frizer,
            testCase.UslugaId,
            usluga);

        Assert.Equal(testCase.RezervacijaId, rezervacija.id);
        Assert.Equal(testCase.DatumRezervacije, rezervacija.datumRezervacije);
        Assert.Equal(testCase.VrijemeTermina, rezervacija.vrijemeTermina);
        Assert.Equal(testCase.Otkazana, rezervacija.otkazana);
        Assert.Equal(testCase.KlijentId, rezervacija.klijentId);
        Assert.Same(klijent, rezervacija.klijent);
        Assert.Equal(testCase.FrizerId, rezervacija.frizerId);
        Assert.Same(frizer, rezervacija.frizer);
        Assert.Equal(testCase.UslugaId, rezervacija.uslugaId);
        Assert.Same(usluga, rezervacija.usluga);
    }

    [Theory]
    [ClassData(typeof(RezervacijaOcjenaCases))]
    public void DataAnnotations_ValidateOcjena(int ocjena, bool expectedValid)
    {
        var rezervacija = CreateValidReservation();
        rezervacija.ocjena = ocjena;

        Assert.Equal(expectedValid, ModelTestData.IsValid(rezervacija));
    }

    private static Rezervacija CreateValidReservation()
    {
        return new Rezervacija
        {
            datumRezervacije = new DateTime(2026, 11, 20),
            vrijemeTermina = new DateTime(2026, 11, 20, 10, 30, 0),
            klijentId = "klijent-1",
            uslugaId = 1,
            usluga = new Usluga()
        };
    }

    public static IEnumerable<object[]> JsonCases() =>
        ModelTestData.JsonConstructionCases();
}

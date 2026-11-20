using SakuraWeb.Models;

namespace SakuraWeb.Services;

public sealed record AvailabilityResult(
    bool IsAvailable,
    string? ErrorMessage);

public sealed class RezervacijaAvailabilityService
{
    private static readonly TimeSpan EarliestAllowedSlot =
        new(9, 0, 0);

    private static readonly TimeSpan LatestAllowedSlot =
        new(16, 30, 0);

    public AvailabilityResult Check(
        DateTime datum,
        TimeSpan pocetnoVrijeme,
        Usluga usluga,
        string frizerId,
        IEnumerable<Rezervacija> postojeceRezervacije)
    {
        if (pocetnoVrijeme.Ticks % TimeSpan.FromMinutes(30).Ticks != 0)
        {
            return new AvailabilityResult(
                false,
                "Termin mora poceti na puni sat ili pola sata.");
        }

        var brojTermina =
            (int)Math.Ceiling(usluga.trajanje / 30.0);

        var potrebniTermini =
            GenerisiTermine(pocetnoVrijeme, brojTermina);

        if (pocetnoVrijeme < EarliestAllowedSlot ||
            potrebniTermini.Last() > LatestAllowedSlot)
        {
            return new AvailabilityResult(
                false,
                "Odabrana usluga ne staje u radno vrijeme za odabrani termin.");
        }

        var zauzetaVremena =
            IzracunajZauzetaVremena(
                datum,
                frizerId,
                postojeceRezervacije);

        if (potrebniTermini.Any(zauzetaVremena.Contains))
        {
            return new AvailabilityResult(
                false,
                "Odabrani termin je zauzet. Molimo odaberite drugi termin.");
        }

        return new AvailabilityResult(true, null);
    }

    private static List<TimeSpan> GenerisiTermine(
        TimeSpan pocetnoVrijeme,
        int brojTermina)
    {
        return Enumerable
            .Range(0, brojTermina)
            .Select(index =>
                pocetnoVrijeme.Add(
                    TimeSpan.FromMinutes(30 * index)))
            .ToList();
    }

    private static HashSet<TimeSpan> IzracunajZauzetaVremena(
        DateTime datum,
        string frizerId,
        IEnumerable<Rezervacija> postojeceRezervacije)
    {
        var zauzetaVremena = new HashSet<TimeSpan>();

        foreach (var rezervacija in postojeceRezervacije.Where(
            rezervacija =>
                !rezervacija.otkazana &&
                rezervacija.frizerId == frizerId &&
                rezervacija.datumRezervacije.Date == datum.Date))
        {
            var trajanjeUsluge =
                (int)Math.Ceiling(
                    rezervacija.usluga.trajanje / 30.0);

            foreach (var termin in GenerisiTermine(
                rezervacija.vrijemeTermina.TimeOfDay,
                trajanjeUsluge))
            {
                zauzetaVremena.Add(termin);
            }
        }

        return zauzetaVremena;
    }
}
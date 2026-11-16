using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SakuraWeb.Data;
using SakuraWeb.Models;
using SakuraWeb.Services;
using System.Security.Claims;

namespace SakuraWeb.Controllers
{
    public class RezervacijaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;
        private readonly RezervacijaAvailabilityService
    _availabilityService;

public RezervacijaController(
    ApplicationDbContext context,
    IEmailSender emailSender,
    RezervacijaAvailabilityService?
        availabilityService = null)
{
    _context = context;
    _emailSender = emailSender;

    _availabilityService =
        availabilityService ??
        new RezervacijaAvailabilityService();
}

        private static string FormatDatum(DateTime datum)
        {
            return datum.ToString("dd.MM.yyyy.");
        }

        private static string FormatVrijeme(DateTime vrijeme)
        {
            return vrijeme.ToString("HH:mm");
        }
        // GET: Rezervacija
        [Authorize(Roles = "Administrator, Klijent, Frizer")]
        public async Task<IActionResult> Index()
        {
            var rezervacije = _context.rezervacije
                .Include(r => r.klijent)
                .Include(r => r.frizer)
                .Include(r => r.usluga);

            if (User.IsInRole("Administrator"))
            {
                return View(await rezervacije.OrderByDescending(r => r.vrijemeTermina).ToListAsync());
            }

            var korisnikId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var moje = User.IsInRole("Frizer")
                ? rezervacije.Where(r => r.frizerId == korisnikId)
                : rezervacije.Where(r => r.klijentId == korisnikId);

            return View(await moje.OrderByDescending(r => r.vrijemeTermina).ToListAsync());
        }

        // POST: Rezervacija/Otkazi/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator, Klijent, Frizer")]
        public async Task<IActionResult> Otkazi(int id)
        {
            var rezervacija = await _context.rezervacije
                .Include(r => r.klijent)
                .Include(r => r.frizer)
                .Include(r => r.usluga)
                .FirstOrDefaultAsync(r => r.id == id);
            if (rezervacija == null)
            {
                return NotFound();
            }

            var korisnikId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!User.IsInRole("Administrator") && rezervacija.klijentId != korisnikId && rezervacija.frizerId != korisnikId)
            {
                return Forbid();
            }

            rezervacija.otkazana = true;
            await _context.SaveChangesAsync();

            var frizerIme = rezervacija.frizer?.korisnickoIme ?? "Nije dodijeljen";
            var datum = FormatDatum(rezervacija.datumRezervacije);
            var vrijeme = FormatVrijeme(rezervacija.vrijemeTermina);

            await _emailSender.SendEmailAsync(
                rezervacija.klijent.emailAdresa,
                "[Sakura] Otkazana rezervacija",
                $"Vaša rezervacija usluge \"{rezervacija.usluga.naziv}\" kod frizera {frizerIme} na datum {datum} u {vrijeme} sati je otkazana");

            if (rezervacija.frizer != null)
            {
                await _emailSender.SendEmailAsync(
                    rezervacija.frizer.emailAdresa,
                    "[Sakura] Otkazana rezervacija",
                    $"Rezervacija usluge \"{rezervacija.usluga.naziv}\" klijent {rezervacija.klijent.korisnickoIme} kod vas na datum {datum} u {vrijeme} sati je otkazana");
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Rezervacija/Ocijeni/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Klijent")]
        public async Task<IActionResult> Ocijeni(int id, int ocjena)
        {
            var rezervacija = await _context.rezervacije.FindAsync(id);
            if (rezervacija == null)
            {
                return NotFound();
            }

            var korisnikId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (rezervacija.klijentId != korisnikId)
            {
                return Forbid();
            }

            if (ocjena >= 1 && ocjena <= 5)
            {
                rezervacija.ocjena = ocjena;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Rezervacija/Details/5
        [Authorize(Roles = "Administrator, Klijent, Frizer")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezervacija = await _context.rezervacije
                .Include(r => r.klijent)
                .Include(r => r.frizer)
                .Include(r => r.usluga)
                .FirstOrDefaultAsync(m => m.id == id);
            if (rezervacija == null)
            {
                return NotFound();
            }

            return View(rezervacija);
        }

        // GET: Rezervacija/Create
        [Authorize(Roles = "Klijent")]
public async Task<IActionResult> Create()
{
    var usluge =
        await _context.usluge
            .OrderBy(u => u.kategorija)
            .ThenBy(u => u.naziv)
            .ToListAsync();

    var frizeri =
        await _context.korisnici
            .Where(k => k.ulogaKorisnika == Uloga.Frizer)
            .OrderBy(k => k.korisnickoIme)
            .ToListAsync();

    ViewBag.Usluge = usluge;
    ViewBag.Frizeri = frizeri;

    return View();
}

        // POST: Rezervacija/Create
        [HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "Klijent")]
public async Task<IActionResult> Create(
    DateTime datumRezervacije,
    TimeSpan pocetnoVrijeme,
    int uslugaId,
    string frizerId)
{
    var usluga =
        await _context.usluge.FindAsync(uslugaId);

    if (usluga == null)
    {
        ModelState.AddModelError(
            "",
            "Odabrana usluga ne postoji.");

        return await VratiCreateView();
    }

    var frizer =
        await _context.korisnici.FindAsync(frizerId);

    if (frizer == null ||
        frizer.ulogaKorisnika != Uloga.Frizer)
    {
        ModelState.AddModelError(
            "",
            "Odabrani frizer ne postoji.");

        return await VratiCreateView();
    }

    var pocetakDana =
        datumRezervacije.Date;

    var krajDana =
        pocetakDana.AddDays(1);

    var zauzetiTermini =
        await _context.rezervacije
            .Where(r =>
                !r.otkazana &&
                r.frizerId == frizerId &&
                r.datumRezervacije >= pocetakDana &&
                r.datumRezervacije < krajDana)
            .Include(r => r.usluga)
            .ToListAsync();

    var provjeraTermina =
        _availabilityService.Check(
            datumRezervacije,
            pocetnoVrijeme,
            usluga,
            frizerId,
            zauzetiTermini);

    if (!provjeraTermina.IsAvailable)
    {
        ModelState.AddModelError(
            "",
            provjeraTermina.ErrorMessage!);

        return await VratiCreateView();
    }

    var klijentId =
        User.FindFirstValue(
            ClaimTypes.NameIdentifier);

    var rezervacija = new Rezervacija
    {
        datumRezervacije =
            datumRezervacije.Date,

        vrijemeTermina =
            datumRezervacije.Date
                .Add(pocetnoVrijeme),

        otkazana = false,
        ocjena = 0,
        klijentId = klijentId,
        frizerId = frizerId,
        uslugaId = uslugaId
    };

    _context.Entry(rezervacija)
        .Property(r => r.id)
        .IsTemporary = true;

    _context.Add(rezervacija);

    await _context.SaveChangesAsync();

    var klijent =
        await _context.korisnici
            .FindAsync(klijentId);

    await _emailSender.SendEmailAsync(
        klijent!.emailAdresa,
        "[Sakura] Uspješna rezervacija",
        $"Vaša rezervacija usluge \"{usluga.naziv}\" " +
        $"kod frizera {frizer.korisnickoIme} " +
        $"na datum {FormatDatum(rezervacija.datumRezervacije)} " +
        $"u {FormatVrijeme(rezervacija.vrijemeTermina)} " +
        "sati je uspješna");

    return RedirectToAction(nameof(Index));
}

       private async Task<IActionResult> VratiCreateView()
{
    var usluge =
        await _context.usluge
            .OrderBy(u => u.kategorija)
            .ThenBy(u => u.naziv)
            .ToListAsync();

    var frizeri =
        await _context.korisnici
            .Where(k => k.ulogaKorisnika == Uloga.Frizer)
            .OrderBy(k => k.korisnickoIme)
            .ToListAsync();

    ViewBag.Usluge = usluge;
    ViewBag.Frizeri = frizeri;

    return View();
}

        // GET: Rezervacija/Edit/5
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezervacija = await _context.rezervacije
                .Include(r => r.klijent)
                .Include(r => r.frizer)
                .Include(r => r.usluga)
                .FirstOrDefaultAsync(r => r.id == id);
            if (rezervacija == null)
            {
                return NotFound();
            }
            return View(rezervacija);
        }

        // POST: Rezervacija/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Edit(int id, DateTime datumRezervacije, TimeSpan vrijemeTermina)
        {
            var rezervacija = await _context.rezervacije
                .Include(r => r.klijent)
                .Include(r => r.frizer)
                .Include(r => r.usluga)
                .FirstOrDefaultAsync(r => r.id == id);
            if (rezervacija == null)
            {
                return NotFound();
            }

            var stariDatum = rezervacija.datumRezervacije;
            var staroVrijeme = rezervacija.vrijemeTermina;

            rezervacija.datumRezervacije = datumRezervacije.Date;
            rezervacija.vrijemeTermina = datumRezervacije.Date.Add(vrijemeTermina);

            await _context.SaveChangesAsync();

            if (rezervacija.datumRezervacije != stariDatum || rezervacija.vrijemeTermina != staroVrijeme)
            {
                await _emailSender.SendEmailAsync(
                    rezervacija.klijent.emailAdresa,
                    "[Sakura] Pomjerena rezervacija",
                    $"Vaša rezervacija usluge \"{rezervacija.usluga.naziv}\" kod frizera {rezervacija.frizer?.korisnickoIme ?? "Nije dodijeljen"} na datum {FormatDatum(stariDatum)} u {FormatVrijeme(staroVrijeme)} sati je pomjerena na datum {FormatDatum(rezervacija.datumRezervacije)} u {FormatVrijeme(rezervacija.vrijemeTermina)} sati");
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Rezervacija/Delete/5
        [Authorize(Roles = "Administrator, Klijent")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rezervacija = await _context.rezervacije
                .Include(r => r.klijent)
                .Include(r => r.frizer)
                .Include(r => r.usluga)
                .FirstOrDefaultAsync(m => m.id == id);
            if (rezervacija == null)
            {
                return NotFound();
            }

            return View(rezervacija);
        }

        // POST: Rezervacija/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rezervacija = await _context.rezervacije.FindAsync(id);
            if (rezervacija != null)
            {
                _context.rezervacije.Remove(rezervacija);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // GET: Rezervacija/ZauzetiTermini?datum=2026-06-15
        [HttpGet]
public async Task<IActionResult> ZauzetiTermini(
    DateTime datum,
    string? frizerId = null)
{
    var pocetakDana =
        datum.Date;

    var krajDana =
        pocetakDana.AddDays(1);

    var rezervacije =
        await _context.rezervacije
            .Where(r =>
                !r.otkazana &&
                (frizerId == null ||
                 r.frizerId == frizerId) &&
                r.datumRezervacije >= pocetakDana &&
                r.datumRezervacije < krajDana)
            .Include(r => r.usluga)
            .ToListAsync();

    var zauzeto =
        new HashSet<string>();

    foreach (var rez in rezervacije)
    {
        var brojTermina =
            (int)Math.Ceiling(
                rez.usluga.trajanje / 30.0);

        for (var i = 0; i < brojTermina; i++)
        {
            var vrijeme =
                rez.vrijemeTermina.TimeOfDay
                    .Add(TimeSpan.FromMinutes(30 * i));

            zauzeto.Add(
                vrijeme.ToString(@"hh\:mm"));
        }
    }

    return Json(zauzeto);
}

        private bool RezervacijaExists(int id)
        {
            return _context.rezervacije.Any(e => e.id == id);
        }
    }
}

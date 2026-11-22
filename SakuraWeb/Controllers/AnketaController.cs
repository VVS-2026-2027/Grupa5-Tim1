using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SakuraWeb.Data;
using SakuraWeb.Models;

namespace SakuraWeb.Controllers
{
    public class AnketaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnketaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Anketa
        public async Task<IActionResult> Index()
        {
            return View(await DohvatiPitanjaAsync());
        }

        // GET: Anketa/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anketa = await _context.ankete
                .Include(a => a.korisnik)
                .FirstOrDefaultAsync(m => m.id == id);
            if (anketa == null)
            {
                return NotFound();
            }

            return View(anketa);
        }

        // GET: Anketa/Create
        public IActionResult Create()
        {
            PopuniKorisnike();
            return View();
        }

        // POST: Anketa/Create
  
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("id,poeni,vrijemePopunjavanja,korisnikId")] Anketa anketa)
        {
            if (ModelState.IsValid)
            {
                _context.Add(anketa);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopuniKorisnike(anketa.korisnikId);
            return View(anketa);
        }

        // GET: Anketa/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anketa = await _context.ankete.FindAsync(id);
            if (anketa == null)
            {
                return NotFound();
            }
            PopuniKorisnike(anketa.korisnikId);
            return View(anketa);
        }

        // POST: Anketa/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("id,poeni,vrijemePopunjavanja,korisnikId")] Anketa anketa)
        {
            if (id != anketa.id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(anketa);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnketaExists(anketa.id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            PopuniKorisnike(anketa.korisnikId);
            return View(anketa);
        }

        // GET: Anketa/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var anketa = await _context.ankete
                .Include(a => a.korisnik)
                .FirstOrDefaultAsync(m => m.id == id);
            if (anketa == null)
            {
                return NotFound();
            }

            return View(anketa);
        }

        // POST: Anketa/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var anketa = await _context.ankete.FindAsync(id);
            if (anketa != null)
            {
                _context.ankete.Remove(anketa);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Anketa/Popuni - prikazuje anketu sa pitanjima iz baze
        public async Task<IActionResult> Popuni()
        {
            return View(await DohvatiPitanjaAsync());
        }

        // POST: Anketa/Preporuka - računa preporuku na osnovu odabranih odgovora
        [HttpPost]
        public async Task<IActionResult> Preporuka([FromBody] List<int> odgovorIds)
        {
            int ukupnoBodova = await _context.odgovori
                .Where(o => odgovorIds.Contains(o.id))
                .SumAsync(o => o.poeni);

            var proizvodi = await _context.proizvodi.ToListAsync();

            Proizvod? NajbliziProfilu(params KategorijaProizvoda[] kategorije)
            {
                return proizvodi
                    .Where(p => kategorije.Contains(p.kategorija))
                    .OrderBy(p => Math.Abs(p.poeniPr - ukupnoBodova))
                    .FirstOrDefault();
            }

            static object? UPreporuku(Proizvod? p) =>
                p == null ? null : new { id = p.id, naziv = p.naziv, slika = p.slikaPutanja };

            var preporuke = new[]
            {
                UPreporuku(NajbliziProfilu(KategorijaProizvoda.Šampon)),
                UPreporuku(NajbliziProfilu(KategorijaProizvoda.Regenerator)),
                UPreporuku(NajbliziProfilu(KategorijaProizvoda.Maska, KategorijaProizvoda.Ulje))
            }.Where(p => p != null);

            return Json(preporuke);
        }


        private Task<List<Pitanje>> DohvatiPitanjaAsync()
        {
            return _context.pitanja
                .Include(p => p.odgovori)
                .OrderBy(p => p.id)
                .ToListAsync();
        }

        private void PopuniKorisnike(string? odabraniId = null)
        {
            ViewData["korisnikId"] = new SelectList(_context.korisnici, "id", "id", odabraniId);
        }

        private bool AnketaExists(int id)
        {
            return _context.ankete.Any(e => e.id == id);
        }
    }
}
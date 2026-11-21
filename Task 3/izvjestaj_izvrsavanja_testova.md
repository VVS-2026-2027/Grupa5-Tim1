# TDD analiza: RED → GREEN za test `Create_ValidModel_SavesAndRedirects`

## 1. Cilj i pristup

Cilj je pokazati da test `Create_ValidModel_SavesAndRedirects` stvarno otkriva grešku u kodu: da **pada** kada je funkcionalnost neispravna (RED) i da **prolazi** kada je ispravna (GREEN).

Test je pokretan pojedinačno u obje faze, bez ikakvih izmjena u samom testu. Mijenjan je samo kod kontrolera.

Naredba (ista u obje faze):

```powershell
dotnet test SakuraWeb.Tests/SakuraWeb.Tests.csproj --filter "FullyQualifiedName=SakuraWeb.Tests.Controllers.AnketaControllerTests.Create_ValidModel_SavesAndRedirects"
```

---

## 2. Testni kod

```csharp
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
```

Test provjerava dvije stvari:

| # | Provjera | Šta dokazuje |
|:---:|---|---|
| 1 | `Assert.IsType<RedirectToActionResult>(result)` | Kontroler nakon uspješnog kreiranja preusmjerava korisnika |
| 2 | `Assert.Single(db.ankete)` | Anketa je stvarno **sačuvana** u bazi (tačno jedan zapis) |

---

## 3. Izmjena koda između faza

Mijenjana je samo jedna linija u metodi `Create` (POST) u `AnketaController`:

| Faza | Stanje linije |
|---|---|
| 🔴 RED | `// await _context.SaveChangesAsync();` (zakomentarisano) |
| 🟢 GREEN | `await _context.SaveChangesAsync();` (aktivno) |

---

## 4. 🔴 RED faza

### 4.1 Ispis

```text
[xUnit.net 00:00:04.89]     SakuraWeb.Tests.Controllers.AnketaControllerTests.Create_ValidModel_SavesAndRedirects [FAIL]
[xUnit.net 00:00:04.90]       Assert.Single() Failure: The collection was empty
[xUnit.net 00:00:04.90]       Stack Trace:
[xUnit.net 00:00:04.90]         ...AnketaControllerTests.cs(126,0): at ...Create_ValidModel_SavesAndRedirects()
[xUnit.net 00:00:04.90]         ...AnketaControllerTests.cs(133,0): at ...Create_ValidModel_SavesAndRedirects()

Test summary: total: 1; failed: 1; succeeded: 0; skipped: 0; duration: 8,7s
Build failed with 1 error(s) and 54 warning(s) in 38,5s
```

### 4.2 Rezultat

| Ukupno | Passed | Failed | Skipped | Trajanje |
|:---:|:---:|:---:|:---:|:---:|
| 1 | 0 | **1 ❌** | 0 | 8,7 s |

| Testni slučaj | Ishod | Poruka greške | Lokacija |
|---|:---:|---|---|
| `Create_ValidModel_SavesAndRedirects` | ❌ Failed | `Assert.Single() Failure: The collection was empty` | `AnketaControllerTests.cs`, linija 126 |

### 4.3 Tumačenje

- **Pala je druga provjera.** Poruka `Assert.Single() Failure: The collection was empty` dolazi iz `Assert.Single(db.ankete)`. Pošto se u testu ta linija izvršava nakon `Assert.IsType<RedirectToActionResult>`, a greška je baš na njoj, slijedi da je prva provjera prošla: kontroler je i dalje vraćao preusmjeravanje.
- **Zapis nije sačuvan.** Bez `SaveChangesAsync()` EF Core samo prati entitet u memoriji (`Add`), ali ga ne upisuje u bazu. Upit nad `db.ankete` zato vraća praznu kolekciju.
- **Ovo je opasna vrsta greške.** Korisnik bi vidio da je anketa „kreirana" (preusmjeravanje se desilo), a podaci bi se izgubili. Test koji provjerava samo vraćeni tip rezultata ovo ne bi uhvatio. Zato je provjera `Assert.Single(db.ankete)` ključna.
- **„Build failed with 1 error" ne znači grešku kompajliranja.** Oba projekta su se uspješno izgradila (`SakuraWeb` i `SakuraWeb.Tests` prijavljuju `succeeded`). Jedini „error" je `TESTERROR`, kojim `dotnet test` prijavljuje pali test.

### 4.4 Vremena (RED)

| Korak | Vrijeme |
|---|---|
| Build `SakuraWeb` | 12,2 s (48 upozorenja) |
| Build `SakuraWeb.Tests` | 0,9 s (2 upozorenja) |
| Discovery | 0,70 s → 0,98 s |
| Početak izvršavanja | 1,23 s |
| Pad testa | 4,89 s |
| Završetak | 4,96 s |
| Izvršavanje testa (ukupno) | 8,8 s |
| Cijela naredba | 38,5 s |

---

## 5. 🟢 GREEN faza

### 5.1 Ispis

```text
[xUnit.net 00:00:01.73]   Starting:    SakuraWeb.Tests
[xUnit.net 00:00:06.12]   Finished:    SakuraWeb.Tests
  SakuraWeb.Tests test net10.0 succeeded (10,0s)

Test summary: total: 1; failed: 0; succeeded: 1; skipped: 0; duration: 9,9s
Build succeeded with 54 warning(s) in 30,3s
```

### 5.2 Rezultat

| Ukupno | Passed | Failed | Skipped | Trajanje |
|:---:|:---:|:---:|:---:|:---:|
| 1 | **1 ✅** | 0 | 0 | 9,9 s |

| Testni slučaj | Ishod |
|---|:---:|
| `Create_ValidModel_SavesAndRedirects` | ✅ Passed |

### 5.3 Tumačenje

Nakon vraćanja linije `await _context.SaveChangesAsync();` obje provjere prolaze: kontroler preusmjerava, a u bazi postoji tačno jedan zapis ankete.

### 5.4 Vremena (GREEN)

| Korak | Vrijeme |
|---|---|
| Build `SakuraWeb` | 11,4 s (48 upozorenja) |
| Build `SakuraWeb.Tests` | 1,2 s (2 upozorenja) |
| Discovery | 1,08 s → 1,51 s |
| Početak izvršavanja | 1,73 s |
| Završetak | 6,12 s |
| Izvršavanje testa (ukupno) | 10,0 s |
| Cijela naredba | 30,3 s |

---

## 6. Poređenje RED i GREEN

| Kriterij | 🔴 RED | 🟢 GREEN |
|---|---|---|
| Linija u kontroleru | `// await _context.SaveChangesAsync();` | `await _context.SaveChangesAsync();` |
| `total` | 1 | 1 |
| `failed` | **1** | **0** |
| `succeeded` | 0 | **1** |
| Poruka | `Assert.Single() Failure: The collection was empty` | – |
| Pala provjera | `Assert.Single(db.ankete)` (linija 126) | – |
| Prošla provjera | `Assert.IsType<RedirectToActionResult>` | obje |
| Status builda | Build failed (1 error = pali test) | Build succeeded |
| Upozorenja | 54 | 54 |

**Ključni zaključak:** jedina promjena između faza je jedna linija u kontroleru, a ishod testa se promijenio iz *failed* u *succeeded*. To potvrđuje da test **ispravno otkriva grešku** i da se ponaša deterministički.

---


## 7. 🟡 YELLOW faza (refaktorisanje)

### 7.1 Šta je promijenjeno u `AnketaController`

Refaktorisanje nije mijenjalo ponašanje kontrolera, samo strukturu koda:

| # | Izmjena | Efekat |
|:---:|---|---|
| 1 | Uklonjen dupli `[HttpPost]` i komentar na metodi `Preporuka` | Čitljiviji kod |
| 2 | Izdvojena privatna metoda `DohvatiPitanjaAsync()` | `Index` i `Popuni` više ne dupliraju isti upit |
| 3 | Izdvojena privatna metoda `PopuniKorisnike(...)` | Ista linija `ViewData["korisnikId"]` više se ne ponavlja u `Create` i `Edit` |
| 4 | `Preporuka` pojednostavljena (`NajbliziProfilu(params ...)`, `UPreporuku(...)`) | Manje ponavljanja pri biranju proizvoda i građenju odgovora |

### 7.2 Ispis

```text
Sender Email: sender@test.com; Sender Password: password
Postavke za zadani newsletter nisu pronadene
Postavke za zadani newsletter nisu pronadene
[xUnit.net 00:00:07.88]   Finished:    SakuraWeb.Tests
  SakuraWeb.Tests test net10.0 succeeded (17,6s)

Test summary: total: 99; failed: 0; succeeded: 99; skipped: 0; duration: 12,8s
Build succeeded with 55 warning(s) in 58,0s
```

### 7.3 Rezultat

Pokrenut je **cijeli** skup testova:

```powershell
dotnet test SakuraWeb.Tests/SakuraWeb.Tests.csproj
```

| Ukupno | Passed | Failed | Skipped | Trajanje | Build |
|:---:|:---:|:---:|:---:|:---:|:---:|
| 99 | **99 ✅** | 0 | 0 | 12,8 s | ✅ Succeeded (55 upozorenja, 58,0 s) |

### 7.4 Tumačenje

- Svih 99 testova prolazi, pa refaktorisanje nije promijenilo ponašanje. Među njima su svi testovi `AnketaController` (`Index`, `Details`, `Create`, `Edit`, `Delete`, `Popuni`, `Preporuka`), koji direktno provjeravaju izmijenjenu klasu.
- Ispisi u konzoli (`Sender Email: ...`, `Postavke za zadani newsletter nisu pronadene` i `SmtpException` u `EmailSender.cs`, linija 72) isti su kao u ranijim pokretanjima. Potiču iz testova koji namjerno simuliraju neuspjeh slanja emaila i nedostajuća podešavanja, pa nisu greške.

### 7.5 Broj upozorenja kroz faze

| Pokretanje | Upozorenja | Napomena |
|---|:---:|---|
| Cijeli skup testova (prije TDD ciklusa) | 56 | 4 u testnom projektu (2 NuGet + 2× CS8602) |
| RED i GREEN (pojedinačni test) | 54 | CS8602 u testovima uklonjeni |
| 🟡 YELLOW (cijeli skup testova) | 55 | Jedno upozorenje više nego u GREEN fazi |

Upozorenja su i dalje samo upozorenja i ne utiču na ishod testova. Razlika od 1 u odnosu na GREEN fazu iz ispisa se ne može objasniti, jer je prikazan samo kraj ispisa. Da se utvrdi o kojem je upozorenju riječ, može se pokrenuti:

```powershell
dotnet build SakuraWeb/SakuraWeb.csproj --no-incremental 2>&1 | findstr "AnketaController"
```

Ako se tu pojavi novo upozorenje (npr. nullable), treba ga ispraviti, a testove ponovo pokrenuti.

### 7.6 Status TDD ciklusa

| Faza | Cilj | Očekivano | Dobijeno | Status |
|---|---|:---:|:---:|:---:|
| 🔴 RED | Test pada na neispravnom kodu | `failed: 1` | `failed: 1` | ✅ |
| 🟢 GREEN | Test prolazi na ispravnom kodu | `failed: 0` | `failed: 0` | ✅ |
| 🟡 YELLOW | Refaktorisanje uz zelene testove | `failed: 0` (svi testovi) | `failed: 0; succeeded: 99` | ✅ |

---

## 8. Zaključak

- U **RED** fazi, uz zakomentarisan `SaveChangesAsync()`, test je pao s porukom `Assert.Single() Failure: The collection was empty`. Time je dokazano da test otkriva slučaj u kojem se anketa ne čuva u bazi.
- U **GREEN** fazi, nakon vraćanja linije, test je prošao (`failed: 0; succeeded: 1`).
- Test provjerava i **preusmjeravanje** i **stvarno čuvanje podataka**, pa hvata grešku koju bi provjera samo tipa rezultata propustila.
- U **YELLOW** fazi kod `AnketaController` je refaktorisan (izdvojene pomoćne metode, uklonjeno dupliranje), a cijeli skup testova je prošao: `total: 99; failed: 0; succeeded: 99`. Time je potvrđeno da refaktorisanje nije promijenilo ponašanje.

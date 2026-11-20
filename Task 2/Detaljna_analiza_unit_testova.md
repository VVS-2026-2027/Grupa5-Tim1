# Unit testovi – projekt SakuraWeb.Tests

## Sadržaj

- [5.1 Testovi kontrolera](#51-testovi-kontrolera)
  - [5.1.1 AnketaController](#511-anketacontroller)
  - [5.1.2 NewsletterController](#512-newslettercontroller)
  - [5.1.3 UslugaController](#513-uslugacontroller)
- [6. Ostali unit testovi u projektu](#6-ostali-unit-testovi-u-projektu)
  - [6.1 RezervacijaControllerTests](#61-rezervacijacontrollertests)
  - [6.2 RacunControllerTests](#62-racuncontrollertests)
  - [6.3 ProizvodiControllerTests](#63-proizvodicontrollertests)
  - [6.4 KorisnikTests](#64-korisniktests)
  - [6.5 RezervacijaAvailabilityServiceTests](#65-rezervacijaavailabilityservicetests)
  - [6.6 ProizvodSearchServiceTests](#66-proizvodsearchservicetests)
  - [6.7 EmailSenderTests](#67-emailsendertests)
  - [6.8 Zaključak analize ostalih testova](#68-zaključak-analize-ostalih-testova)

---

## 5.1 Testovi kontrolera

### 5.1.1 AnketaController

Klasa `AnketaControllerTests` testira funkcionalnosti kontrolera `AnketaController`, koji upravlja pitanjima, anketama i preporukama proizvoda. Testovi pokrivaju osnovne CRUD akcije, prikaz podataka, validaciju modela, nepostojeće identifikatore i preporuku proizvoda.

Za testiranje je korištena EF Core InMemory baza podataka kreirana pomoću klase `TestContext`. Svaki test koristi zaseban kontekst baze, tako da promjene izvršene u jednom testu ne utiču na ostale testove.

#### Testiranje metode `Index`

Metoda `Index` vraća sva pitanja sortirana prema njihovom identifikatoru.

Test `Index_ReturnsQuestionsOrderedById` dodaje dva pitanja u InMemory bazu sa ID-jevima 2 i 1. Nakon poziva metode `Index` provjerava se:

- da rezultat predstavlja `ViewResult`,
- da model predstavlja `List<Pitanje>`,
- da su pitanja vraćena redoslijedom 1 i 2.

Ovim testom provjerava se prikaz liste i ispravnost sortiranja.

#### Testiranje metode `Details`

Metoda `Details` testirana je u tri scenarija:

- `Details_ReturnsNotFoundForNullId` provjerava slučaj kada nije proslijeđen ID,
- `Details_ReturnsNotFoundForUnknownId` provjerava nepostojeći ID,
- `Details_ReturnsViewWithAnketa` provjerava postojeću anketu.

Ako je ID `null` ili ne postoji entitet u bazi, očekuje se `NotFoundResult`. Ako entitet postoji, očekuje se `ViewResult` čiji model sadrži odgovarajuću `Anketa`.

Test provjerava i da ID vraćenog modela odgovara proslijeđenom identifikatoru.

#### Testiranje metode `Create` – GET

Test `Create_Get_ReturnsView` provjerava prikaz forme za kreiranje ankete.

Test provjerava:

- da metoda vraća `ViewResult`,
- da `ViewData` sadrži ključno polje `korisnikId`.

#### Testiranje metode `Create` – POST

Za POST varijantu metode `Create` kreirana su dva testna scenarija.

Test `Create_ValidModel_SavesAndRedirects` koristi validan objekat `Anketa`, čuva ga u bazi i provjerava:

- da rezultat predstavlja `RedirectToActionResult`,
- da je u InMemory bazi sačuvan tačno jedan entitet.

Test `Create_InvalidModel_ReturnsView` dodaje grešku u `ModelState`, kreira nevalidan model i provjerava:

- da se vraća `ViewResult`,
- da entitet nije sačuvan,
- da je u `ViewData` dostupno `korisnikId`.

#### Testiranje metode `Edit` – GET

Metoda `Edit` u GET varijanti testirana je sa tri slučaja:

- `Edit_Get_ReturnsNotFoundForNullId`,
- `Edit_Get_ReturnsNotFoundForUnknownId`,
- `Edit_Get_ReturnsViewWithAnketa`.

Prva dva testa provjeravaju `null` i nepostojeći identifikator. Očekivani rezultat je `NotFoundResult`.

Treći test dodaje entitet u bazu, poziva metodu i provjerava:

- da se vraća `ViewResult`,
- da model predstavlja tip `Anketa`,
- da vraćeni ID odgovara traženom,
- da je `korisnikId` dostupno u `ViewData`.

#### Testiranje metode `Edit` – POST

POST varijanta metode `Edit` testira uspješnu izmjenu i različite granične slučajeve.

Test `Edit_Post_MismatchedId_ReturnsNotFound` proslijeđuje različite ID-jeve za URL i model. Očekivani rezultat je `NotFoundResult`.

Test `Edit_Post_InvalidModel_ReturnsView` dodaje grešku u `ModelState` i provjerava da se vraća `ViewResult` sa modelom ankete.

Test `Edit_Post_ValidModel_UpdatesAndRedirects` dodaje entitet u bazu, zatim briše praćenje EF Core-a pomoću:

```csharp
db.ChangeTracker.Clear();
```

Kreira se novi objekat sa istim identifikatorom i izmijenjenom vrijednošću. Nakon poziva metode provjerava se:

- da se vraća `RedirectToActionResult`,
- da je u bazi sačuvana nova vrijednost entiteta.

Test `Edit_Post_DbUpdateConcurrencyException_AnketaDoesNotExist` testira konkurentni izuzetak na nepostojećem entitetu. Entitet se dodaje u `ChangeTracker`, označi se kao izmijenjen i poziva se metoda `Edit`. Očekivani rezultat je `NotFoundResult`.

#### Testiranje metode `Delete` – GET

Metoda `Delete` u GET varijanti testira:

- `Delete_Get_ReturnsNotFoundForNullId`,
- `Delete_Get_ReturnsNotFoundForUnknownId`,
- `Delete_Get_ReturnsViewWithAnketa`.

Ako nije proslijeđen ID ili traženi entitet ne postoji, vraća se `NotFoundResult`. Ako entitet postoji, vraća se `ViewResult` sa ispravnim modelom.

#### Testiranje metode `DeleteConfirmed`

Metoda `DeleteConfirmed` testirana je sa dvije varijante:

- `DeleteConfirmed_RemovesAnketaAndRedirects`,
- `DeleteConfirmed_AnketaNotFound_Redirects`.

Prvi test dodaje entitet u bazu, poziva brisanje i provjerava da se vraća `RedirectToActionResult` i da je baza prazna.

Drugi test pokušava obrisati nepostojeći entitet i provjerava da metoda ne izaziva grešku i da vraća preusmjeravanje.

#### Testiranje metode `Popuni`

Test `Popuni_ReturnsQuestionsOrderedById` dodaje dva pitanja u InMemory bazu i provjerava:

- da rezultat predstavlja `ViewResult`,
- da model predstavlja `List<Pitanje>`,
- da su pitanja vraćena sortirana po ID-u.

#### Testiranje metode `Preporuka`

Metoda `Preporuka` testira generisanje preporučenih proizvoda.

Test `Preporuka_ReturnsClosestProducts` dodaje odgovore i proizvode u bazu, poziva metodu sa jednim odgovorom i provjerava:

- da se vraća `JsonResult`,
- da model predstavlja kolekciju proizvoda,
- da rezultat sadrži tri preporuke.

Data-driven test `Preporuka_UsesJsonData` učitava testne slučajeve iz fajla:

```text
SakuraWeb.Tests/TestData/recommendation-cases.json
```

Testna metoda koristi `[Theory]` i `[MemberData]`:

```csharp
[Theory]
[MemberData(nameof(RecommendationCases))]
public async Task Preporuka_UsesJsonData(
    List<int> answerIds,
    int expectedCount)
```

JSON fajl može sadržavati:

- jednu vrijednost odgovora,
- više vrijednosti odgovora,
- praznu listu odgovora.

Za svako učitano stanje provjerava se da broj vraćenih preporuka odgovara očekivanom broju iz fajla.

---

### 5.1.2 NewsletterController

Klasa `NewsletterControllerTests` testira funkcionalnosti kontrolera `NewsletterController`, uključujući prikaz poruka, upravljanje newsletterima i slanje poruka pretplaćenim korisnicima.

Testovi koriste EF Core InMemory bazu i Moq zamjenske objekte za email zavisnost `IEmailSender`.

#### Testiranje metode `Index`

Test `Index_ReturnsViewWithPoruke` dodaje poruku u bazu i poziva metodu `Index`.

Provjerava se:

- da metoda vraća `ViewResult`,
- da model predstavlja `IEnumerable<Poruka>`,
- da lista sadrži tačno jednu poruku.

#### Testiranje metode `Details`

Metoda `Details` testirana je u tri scenarija:

- `Details_ReturnsNotFoundForNullId`,
- `Details_ReturnsNotFoundForUnknownId`,
- `Details_ReturnsViewWithNewsletter`.

Ako je ID `null` ili ne postoji newsletter, vraća se `NotFoundResult`.

Ako newsletter postoji, provjerava se:

- da se vraća `ViewResult`,
- da model predstavlja `Newsletter`,
- da je vraćen entitet sa ispravnim ID-jem.

#### Testiranje metode `Create` – GET

Test `Create_Get_ReturnsView` provjerava da metoda `Create` u GET varijanti vraća `ViewResult` i prikazuje formu za kreiranje poruke.

#### Testiranje metode `Create` – POST

Metoda `Create` testirana je sa pozitivnim i negativnim scenarijima.

Test `Create_SendsEmailToSubscriber` dodaje:

- newsletter u bazi,
- korisnika sa aktivnom newsletter pretplatom,
- poruku za slanje.

Kreira se Moq objekt:

```csharp
var email = new Mock<IEmailSender>();
```

Kontroleru se proslijeđuje `email.Object`. Nakon poziva metode provjerava se:

- da se vraća `RedirectToActionResult`,
- da je poruka sačuvana u bazi,
- da je `SendEmailAsync` pozvana tačno jednom,
- da su primaocu, naslovu i tekstu proslijeđene ispravne vrijednosti.

Test `Create_WithoutSettings_ReturnsView` provjerava ponašanje kada newsletter podešavanja nisu dostupna. Očekivani rezultat je:

- `ViewResult`,
- poruka nije sačuvana,
- email nije poslat.

Test `Create_Post_NewsletterNull_ReturnsView` provjerava situaciju kada newsletter ne postoji i metoda vraća pogled bez čuvanja poruke.

Test `Create_Post_UsesConcreteEmailSender` demonstrira korištenje zamjenske konkretne klase `EmailSender`. Umjesto stvarnog slanja poruke koristi se Moq objekat:

```csharp
var concreteSenderMock = new Mock<EmailSender>(
    emailOptions,
    logger);
```

Kontroleru se proslijeđuje zamjenski objekat. Test provjerava da poruka ostaje sačuvana i da kontroler vraća `RedirectToActionResult`.

#### Testiranje metode `Edit` – GET

GET varijanta metode `Edit` testira:

- `null` ID,
- nepoznati ID,
- postojeći newsletter.

Ako je ID nevaljan, očekuje se `NotFoundResult`. Ako newsletter postoji, vraća se pogled sa entitetom.

#### Testiranje metode `Edit` – POST

POST varijanta testirana je u sljedećim scenarijima:

- neusklađen ID u URL-u i modelu,
- nevalidan `ModelState`,
- uspješna izmjena,
- konkurentni izuzetak,
- nepostojeći newsletter.

Test `Edit_Post_ReturnsNotFoundForMismatchedId` provjerava da se vraća `NotFoundResult` ako se ID-ovi ne podudaraju.

Test `Edit_Post_ReturnsViewWhenModelStateInvalid` dodaje grešku u `ModelState` i provjerava da se vraća `ViewResult` sa newsletter modelom.

Test `Edit_Post_UpdatesAndRedirects` dodaje stari newsletter u bazu, zatim kreira novi objekat sa izmijenjenim podacima i poziva metodu `Edit`.

Provjerava se:

- da se vraća preusmjeravanje,
- da je email adresa izmijenjena,
- da je nova lozinka sačuvana u bazu.

Test `Edit_Post_DbUpdateConcurrencyException_ReturnsNotFound` kreira nepostojeći newsletter i označava ga kao izmijenjenog entiteta. Metoda treba da obradi situaciju kao nepostojeći entitet i vrati `NotFoundResult`.

#### Testiranje metode `Delete` – GET

Testovi provjeravaju:

- `Delete_ReturnsNotFoundForNullId`,
- `Delete_ReturnsNotFoundForUnknownId`,
- `Delete_ReturnsViewWithNewsletter`.

Nepostojeći ili `null` identifikatori vraćaju `NotFoundResult`, dok postojeći entitet vraća `ViewResult` sa ispravnim modelom.

#### Testiranje metode `DeleteConfirmed`

Metoda `DeleteConfirmed` testira:

- uspješno brisanje newslettera,
- brisanje nepostojećeg ID-a,
- vraćanje `RedirectToActionResult`,
- odsustvo dodatnih entiteta nakon brisanja.

Test `DeleteConfirmed_NewsletterNotFound_Redirects` provjerava da se nepostojeći ID obrađuje bez izuzetka.

---

### 5.1.3 UslugaController

Klasa `UslugaControllerTests` testira sve osnovne CRUD operacije kontrolera `UslugaController`.

Za testiranje koristi se EF Core InMemory baza bez vanjskih zavisnosti.

#### Testiranje metode `Index`

Test `Index_ReturnsServices` dodaje dvije usluge u bazu:

- uslugu sa ID-jem 2,
- uslugu sa ID-jem 1.

Nakon poziva metode provjerava se:

- da rezultat predstavlja `ViewResult`,
- da model predstavlja `List<Usluga>`,
- da su usluge sortirane po ID-u.

#### Testiranje metode `Details`

Testovi metode `Details` pokrivaju:

- `Details_ReturnsNotFoundForNullId`,
- `Details_ReturnsNotFoundForUnknownId`,
- `Details_ReturnsViewForExistingId`.

Ako je ID `null` ili nepostojeći, vraća se `NotFoundResult`.

Ako usluga postoji, provjerava se da:

- rezultat predstavlja `ViewResult`,
- model predstavlja `Usluga`,
- vraćeni entitet ima ispravnu vrijednost ID-a.

#### Testiranje metode `Create` – GET

Test `Create_Get_ReturnsView` provjerava da metoda `Create` u GET varijanti vraća `ViewResult`.

#### Testiranje metode `Create` – POST

POST metoda testirana je sa validnim i nevalidnim modelom.

Test `Create_ValidService_SavesAndRedirects` kreira novu uslugu i provjerava:

- da se vraća `RedirectToActionResult`,
- da je usluga sačuvana u InMemory bazi,
- da baza sadrži tačno jednu uslugu.

Test `Create_InvalidModel_ReturnsView` dodaje grešku u `ModelState`. Provjerava se:

- da se vraća `ViewResult`,
- da je model tipa `Usluga`,
- da usluga nije sačuvana.

#### Testiranje metode `Edit` – GET

Testovi GET varijante provjeravaju:

- `null` ID,
- nepoznat ID,
- postojeću uslugu.

Ako ID ne postoji, vraća se `NotFoundResult`. Ako usluga postoji, vraća se `ViewResult` sa ispravnim modelom.

#### Testiranje metode `Edit` – POST

POST varijanta metode testira:

- neusklađenost ID-a,
- nevalidan `ModelState`,
- uspješnu izmjenu,
- konkurentni izuzetak,
- nepostojeći entitet.

Test `Edit_MismatchedId_ReturnsNotFound` provjerava da se ne dozvoljava izmjena ako se ID u URL-u i modelu ne podudaraju.

Test `Edit_Post_InvalidModel_ReturnsView` dodaje grešku u `ModelState` i provjerava da se vraća ispravan pogled sa entitetom.

Test `Edit_Post_ValidModel_UpdatesAndRedirects` kreira postojeću uslugu u bazi, čisti `ChangeTracker` i proslijeđuje izmijenjeni model. Provjerava se:

- da se vraća `RedirectToActionResult`,
- da je novi naziv sačuvan,
- da su opis, cijena i trajanje izmijenjeni.

Test `Edit_Post_ConcurrencyException_WhenNotExists_ReturnsNotFound` provjerava situaciju sa nepostojećom uslugom označenom kao izmijenjen entitet.

#### Testiranje metode `Delete` – GET

Testovi provjeravaju:

- `Delete_Get_ReturnsNotFoundForNullId`,
- `Delete_Get_ReturnsNotFoundForUnknownId`,
- `Delete_Get_ReturnsViewForExistingId`.

Nevaljani i nepostojeći ID-jevi vraćaju `NotFoundResult`. Postojeća usluga vraća pogled sa ispravnim modelom.

#### Testiranje metode `DeleteConfirmed`

Test `DeleteConfirmed_RemovesService` dodaje uslugu u bazu, poziva potvrđeno brisanje i provjerava:

- da se vraća `RedirectToActionResult`,
- da je usluga uklonjena iz baze,
- da baza ne sadrži entitete.

Test `DeleteConfirmed_WhenNotFound_Redirects` provjerava da se pokušaj brisanja nepostojećeg ID-a obrađuje preusmjeravanjem bez izuzetka.

---

## 6. Ostali unit testovi u projektu

Pored testova tri navedena kontrolera, projekt `SakuraWeb.Tests` sadrži dodatne testne klase za ostale kontrolere, modele i servisne komponente.

### 6.1 RezervacijaControllerTests

Klasa `RezervacijaControllerTests` testira operacije povezane sa kreiranjem, ocjenjivanjem i otkazivanjem rezervacija.

Test `Create_ValidReservation_Saves` provjerava uspješno kreiranje rezervacije.

Kreiraju se:

- klijent,
- frizer,
- usluga,
- zamjenski `IEmailSender`,
- korisnički kontekst klijenta.

Test provjerava:

- da se vraća `RedirectToActionResult`,
- da je tačno jedna rezervacija sačuvana u bazi,
- da je email poslat klijentu tačno jednom.

Test `Create_UnknownService_ReturnsView` provjerava pokušaj kreiranja rezervacije sa nepostojećom uslugom.

Test provjerava:

- da se vraća `ViewResult`,
- da `ModelState` nije validan,
- da rezervacija nije sačuvana.

Test `Create_TermOutsideWorkingHours_ReturnsView` provjerava termin izvan radnog vremena.

Test provjerava:

- da se vraća `ViewResult`,
- da rezervacija nije sačuvana u bazu.

Test `Ocijeni_ValidRating_IsSaved` dodaje rezervaciju i prosljeđuje validnu ocjenu.

Provjerava se:

- da se vraća `RedirectToActionResult`,
- da je ocjena sačuvana u bazi.

Test `Ocijeni_InvalidRating_DoesNotSave` prosljeđuje neispravnu ocjenu veću od dozvoljenog opsega i provjerava da stara vrijednost ocjene ostaje neizmijenjena.

Test `Otkazi_OtherUser_ReturnsForbid` kreira rezervaciju jednog korisnika i pokušava otkazivanje koristeći drugog korisnika.

Provjerava se da metoda vraća `ForbidResult` i da korisnik ne može otkazati tuđu rezervaciju.

Test `ZauzetiTermini_ReturnsAllSlots` provjerava vraćanje zauzetih termina u formatu `HashSet<string>`.

Provjerava se da JSON rezultat sadrži ispravne vremenske termine, uključujući:

```text
10:00
10:30
```

Ovi testovi također koriste `ControllerContextFactory` za simulaciju prijavljenog korisnika i Moq `IEmailSender` za sprječavanje slanja stvarnih email poruka.

### 6.2 RacunControllerTests

Klasa `RacunControllerTests` testira kontroler računa i korisničke operacije.

U testovima se koristi Moq zamjena za `UserManager<Korisnik>` i `IUserStore<Korisnik>`.

Pomoćna metoda `CreateManager` kreira zamjenski `UserManager` objekat sa potrebnim konstruktorskim parametrima.

Testovi pokrivaju:

- prikaz računa,
- korisničke akcije,
- nevalidne identifikatore,
- nedostajuće korisnike,
- slanje email poruka,
- validan i nevalidan korisnički kontekst.

Zamjenske zavisnosti omogućavaju testiranje Identity funkcionalnosti bez stvarne autentikacione baze.

### 6.3 ProizvodiControllerTests

Klasa `ProizvodiControllerTests` testira upravljanje proizvodima u aplikaciji.

Testovi pokrivaju:

- prikaz proizvoda,
- prikaz detalja proizvoda,
- kreiranje proizvoda,
- izmjenu proizvoda,
- brisanje proizvoda,
- rad sa fajlovima i slikama,
- rad sa `IWebHostEnvironment`,
- validne i nevalidne ulazne podatke.

Testovi koriste atribute `[Fact]`, `[Theory]` i `[InlineData]`.

Inline podaci koriste se za provjeru različitih kategorija i vrijednosti proizvoda bez kreiranja novih zasebnih testnih metoda.

Moq objekat `IWebHostEnvironment` koristi se za simulaciju okruženja aplikacije i omogućava testiranje radnji sa slikama bez stvarnog fajl sistema.

### 6.4 KorisnikTests

Klasa `KorisnikTests` testira model `Korisnik`, posebno metodu za promjenu stanja newsletter pretplate.

Testna metoda ima naziv:

```csharp
PromjeniStanjePretplate_ChangesSubscription
```

Koristi `[Theory]` i `[MemberData]` za učitavanje podataka iz CSV fajla.

Test provjerava:

- aktiviranje pretplate,
- deaktiviranje pretplate,
- ostavljanje neaktivne pretplate neizmijenjenom,
- ostavljanje aktivne pretplate neizmijenjenom.

CSV podaci omogućavaju da se ista testna metoda izvrši sa više ulaznih kombinacija.

### 6.5 RezervacijaAvailabilityServiceTests

Klasa `RezervacijaAvailabilityServiceTests` testira servis `RezervacijaAvailabilityService`.

Test `Check_ReturnsUnavailable_WhenSameFrizerHasOverlappingReservation` provjerava slučaj kada isti frizer već ima rezervaciju koja se preklapa sa traženim terminom.

Očekivani rezultat je:

- `IsAvailable` ima vrijednost `false`,
- vraća se poruka da je termin zauzet,
- korisnik mora odabrati drugi termin.

Test `Check_AllowsSameTime_WhenReservationBelongsToAnotherFrizer` provjerava termin koji je zauzet kod drugog frizera.

Očekivani rezultat je:

- rezervacija je dostupna,
- `IsAvailable` ima vrijednost `true`,
- `ErrorMessage` ima vrijednost `null`.

Test `Check_ReturnsUnavailable_WhenServiceDoesNotFitWorkingHours` provjerava uslugu čije trajanje ne stane u radno vrijeme.

Očekivani rezultat je:

- `IsAvailable` ima vrijednost `false`,
- vraća se poruka da odabrana usluga ne staje u radno vrijeme.

### 6.6 ProizvodSearchServiceTests

Klasa `ProizvodSearchServiceTests` testira servis pretrage proizvoda.

Testovi provjeravaju:

- vraćanje tačnih proizvoda,
- filtriranje proizvoda prema kriterijima,
- slučaj kada nema pronađenih proizvoda,
- konzistentnost vraćenih rezultata.

Servis se testira kao posebna jedinica, bez potrebe za povezivanjem sa kontrolerom ili prikazom.

### 6.7 EmailSenderTests

Klasa `EmailSenderTests` testira email servis.

Testovi provjeravaju:

- ispravnu konfiguraciju email sendera,
- pravilno formiranje email poruke,
- validan primaoca,
- validan naslov i sadržaj poruke,
- ponašanje servisa u različitim konfiguracijama.

Ovi testovi dopunjuju testove kontrolera u kojima se email servis zamjenjuje Moq objektom.

### 6.8 Zaključak analize ostalih testova

Ostale testne klase proširuju pokrivenost izvan kontrolera i obuhvataju:

- Identity i korisničke modele,
- rezervacioni servis,
- pretragu proizvoda,
- email servis,
- data-driven modelne podatke,
- korisnički kontekst,
- zamjenske email zavisnosti.

Ovi testovi doprinose ukupnoj pokrivenosti i demonstriraju primjenu unit testiranja na kontrolere, modele i servisne klase.

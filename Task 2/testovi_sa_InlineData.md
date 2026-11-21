### Testovi sa `[InlineData]`
 
| Klasa | Test | Šta se mijenja po redovima | Redova |
|---|---|---|:---:|
| `ProizvodiController` | `Index_SortsProducts` | Način sortiranja i očekivani redoslijed ID-jeva | 4 |
| `RezervacijaController` | `Index_ReturnsReservationsForCurrentRole` | Korisnik, uloga i očekivani broj rezervacija | 3 |
| `RezervacijaController` | `Create_CancelledOrDifferentDayReservation_DoesNotBlockSlot` | Da li je rezervacija otkazana i pomak dana | 2 |
| `Proizvod` | `DataAnnotations_ValidateVolumen` / `ValidateCijena` / `ValidatePoeni` | Vrijednost i očekivana ispravnost | po 3 |
| `Rezervacija` | `DataAnnotations_ValidateOcjena` | Ocjena (0, 1, 5, 6) i ispravnost | 4 |
| `Odgovor` | `PoeniProperty_ValueInRange_IsValid` / `OutsideRange_IsInvalid` | Poeni u dozvoljenom i van dozvoljenog opsega | 3 i 2 |
| `EmailSender` | `SendEmailAsync_UsesFallbackOrExplicitCredentials` | Kombinacije eksplicitnih i podrazumijevanih kredencijala | 4 |
 
---

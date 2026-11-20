# Rezultati izvršavanja unit testova – SakuraWeb.Tests

## Sažetak

| Ukupno testova | Uspješni (Passed) | Neuspješni (Failed) | Uspješnost |
|:---:|:---:|:---:|:---:|
| 296 | 292  | 4  | 98.6% |

## Pregled po klasama

| Klasa | Ukupno | Passed | Failed |
|---|:---:|:---:|:---:|
| Anketa | 8 | 8 | 0 |
| AnketaController | 24 | 24 | 0 |
| AuthMessageSenderOptions | 2 | 2 | 0 |
| Benefit | 13 | 13 | 0 |
| CustomEmailConfirmationTokenProvider | 1 | 1 | 0 |
| EmailConfirmationTokenProviderOptions | 1 | 1 | 0 |
| EmailSender | 15 | 15 | 0 |
| HomeController | 4 | 4 | 0 |
| Korisnik | 11 | 11 | 0 |
| Newsletter | 7 | 7 | 0 |
| NewsletterController | 20 | 20 | 0 |
| Odgovor | 10 | 10 | 0 |
| Pitanje | 7 | 7 | 0 |
| Poruka | 8 | 8 | 0 |
| Proizvod | 13 | 13 | 0 |
| ProizvodBenefit | 4 | 4 | 0 |
| ProizvodiController | 29 | 29 | 0 |
| ProizvodSastojak | 4 | 4 | 0 |
| RacunController | 22 | 22 | 0 |
| Rezervacija | 7 | 7 | 0 |
| RezervacijaController | 32 | 32 | 0 |
| RezervacijaControllerAdditional | 25 | 21 | 4 |
| Sastojak | 10 | 10 | 0 |
| UslugaController | 19 | 19 | 0 |
| **UKUPNO** | **296** | **292** | **4** |

## Neuspješni testovi

| Klasa | Testni slučaj | Detalj greške |
|---|---|---|
| RezervacijaControllerAdditional | `Delete_KnownReservationReturnsView` | Assert.IsType() Failure: Value is not the exact type. Expected: typeof(Microsoft.AspNetCore.Mvc.ViewResult), Actual: typeof(Microsoft.AspNetCore.Mvc.NotFoundResult) |
| RezervacijaControllerAdditional | `Index_RoleReturnsVisibleReservations(role: "Administrator", expectedCount: 2)` | Assert.Equal() Failure: Values differ. Expected: 2, Actual: 0 |
| RezervacijaControllerAdditional | `Index_RoleReturnsVisibleReservations(role: "Klijent", expectedCount: 1)` | Assert.Equal() Failure: Values differ. Expected: 1, Actual: 0 |
| RezervacijaControllerAdditional | `Index_RoleReturnsVisibleReservations(role: "Frizer", expectedCount: 1)` | Assert.Equal() Failure: Values differ. Expected: 1, Actual: 0 |

## Svi testovi (grupisano po klasama)

> Napomena: kod parametara `testCase: ModelConstructionCase { ... }` izostavljen je ostatak polja radi čitljivosti (označeno sa `…`).

### Anketa

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `IdConstructor_SetsId(id: 42)` |  Passed |  |
| 2 | `IdConstructor_SetsId(id: -1)` |  Passed |  |
| 3 | `PoeniProperty_AssignedValue_IsRetained(poeni: 0)` |  Passed |  |
| 4 | `PoeniProperty_AssignedValue_IsRetained(poeni: 15)` |  Passed |  |
| 5 | `PopunjavanjeProperty_AssignedState_IsRetained` |  Passed |  |
| 6 | `PoeniProperty_AssignedValue_IsRetained(poeni: 100)` |  Passed |  |
| 7 | `IdConstructor_SetsId(id: 0)` |  Passed |  |
| 8 | `ParameterlessConstructor_SetsSentinelId` |  Passed |  |

### AnketaController

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `Edit_Post_MismatchedId_ReturnsNotFound` |  Passed |  |
| 2 | `Create_Get_ReturnsView` |  Passed |  |
| 3 | `Create_InvalidModel_ReturnsView` |  Passed |  |
| 4 | `Details_ReturnsNotFoundForUnknownId` |  Passed |  |
| 5 | `Create_ValidModel_SavesAndRedirects` |  Passed |  |
| 6 | `Details_ReturnsNotFoundForNullId` |  Passed |  |
| 7 | `Preporuka_ReturnsClosestProducts` |  Passed |  |
| 8 | `Edit_Get_ReturnsViewWithAnketa` |  Passed |  |
| 9 | `Delete_Get_ReturnsViewWithAnketa` |  Passed |  |
| 10 | `Details_ReturnsViewWithAnketa` |  Passed |  |
| 11 | `Edit_Post_ValidModel_UpdatesAndRedirects` |  Passed |  |
| 12 | `DeleteConfirmed_AnketaNotFound_Redirects` |  Passed |  |
| 13 | `Preporuka_UsesJsonData(answerIds: [1], expectedCount: 3)` |  Passed |  |
| 14 | `Preporuka_UsesJsonData(answerIds: [1, 2], expectedCount: 3)` |  Passed |  |
| 15 | `Edit_Post_DbUpdateConcurrencyException_AnketaDoesNotExist` |  Passed |  |
| 16 | `Edit_Get_ReturnsNotFoundForUnknownId` |  Passed |  |
| 17 | `Delete_Get_ReturnsNotFoundForNullId` |  Passed |  |
| 18 | `Preporuka_UsesJsonData(answerIds: [], expectedCount: 3)` |  Passed |  |
| 19 | `Delete_Get_ReturnsNotFoundForUnknownId` |  Passed |  |
| 20 | `Edit_Get_ReturnsNotFoundForNullId` |  Passed |  |
| 21 | `Edit_Post_InvalidModel_ReturnsView` |  Passed |  |
| 22 | `DeleteConfirmed_RemovesAnketaAndRedirects` |  Passed |  |
| 23 | `Index_ReturnsQuestionsOrderedById` |  Passed |  |
| 24 | `Popuni_ReturnsQuestionsOrderedById` |  Passed |  |

### AuthMessageSenderOptions

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `Properties_DefaultToNull` |  Passed |  |
| 2 | `Properties_StoreConfiguredEmailAndPassword` |  Passed |  |

### Benefit

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `OpisProperty_ValidText_IsValid(opis: "Sjajna kosa")` |  Passed |  |
| 2 | `ParameterizedConstructor_SetsProperties(id: 42, opis: "Jaca kosu", urlSlike: "jaca.png")` |  Passed |  |
| 3 | `ParameterizedConstructor_SetsProperties(id: 0, opis: "Sjaj", urlSlike: "sjaj.png")` |  Passed |  |
| 4 | `ParameterizedConstructor_SetsProperties(id: -1, opis: "", urlSlike: "")` |  Passed |  |
| 5 | `Properties_AssignedValues_AreRetained(opis: "Sjajna kosa", urlSlike: "sjajna-kosa.svg")` |  Passed |  |
| 6 | `OpisProperty_ValidText_IsValid(opis: "Jaca kosu")` |  Passed |  |
| 7 | `URLSlikeProperty_EmptyValue_IsInvalid` |  Passed |  |
| 8 | `Properties_AssignedValues_AreRetained(opis: "Jaca kosu", urlSlike: "jaca-kosu.svg")` |  Passed |  |
| 9 | `OpisProperty_EmptyValue_IsInvalid` |  Passed |  |
| 10 | `Properties_AssignedValues_AreRetained(opis: "Povecana gustoca", urlSlike: "gustoca.svg")` |  Passed |  |
| 11 | `ParameterlessConstructor_SetsSentinelIdAndEmptyProperties` |  Passed |  |
| 12 | `OpisProperty_ValidText_IsValid(opis: "Povecana gustoca")` |  Passed |  |
| 13 | `ProizvodBenefitiProperty_AddingLink_RetainsCollection` |  Passed |  |

### CustomEmailConfirmationTokenProvider

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `Constructor_CreatesDataProtectorTokenProviderForUserType` |  Passed |  |

### EmailConfirmationTokenProviderOptions

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `Constructor_UsesEmailConfirmationTokenSettings` |  Passed |  |

### EmailSender

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `SendEmailAsync_WithConfiguredCredentials_ValidatesRecipientBeforeSending` |  Passed |  |
| 2 | `Execute_InvalidSenderAddress_ThrowsFormatException` |  Passed |  |
| 3 | `Execute_MalformedRecipient_ThrowsFormatException` |  Passed |  |
| 4 | `Execute_EmptyRecipient_ThrowsArgumentException` |  Passed |  |
| 5 | `SendEmailAsync_WithExplicitCredentials_ThrowsWhenCredentialIsMissing(fromEmail: "from@test.com", appPassword: "", expectedMessage: "Null AppPassword")` |  Passed |  |
| 6 | `SendEmailAsync_WithExplicitCredentials_ThrowsWhenCredentialIsMissing(fromEmail: "", appPassword: "password", expectedMessage: "Null FromEmail")` |  Passed |  |
| 7 | `SendEmailAsync_UsesFallbackOrExplicitCredentials(fromEmail: null, appPassword: "override-password", expectedFromEmail: "configured@example.com", expectedPassword: "override-password")` |  Passed |  |
| 8 | `SendEmailAsync_ThrowsWhenFromEmailIsMissing` |  Passed |  |
| 9 | `SendEmailAsync_UsesConfiguredCredentialsAndSendsMessage` |  Passed |  |
| 10 | `SendEmailAsync_ThrowsWhenAppPasswordIsMissing` |  Passed |  |
| 11 | `Execute_WhenSmtpSendSucceeds_CompletesSending` |  Passed |  |
| 12 | `SendEmailAsync_UsesFallbackOrExplicitCredentials(fromEmail: "override@example.com", appPassword: null, expectedFromEmail: "override@example.com", expectedPassword: "configured-password")` |  Passed |  |
| 13 | `SendEmailAsync_UsesFallbackOrExplicitCredentials(fromEmail: "override@example.com", appPassword: "override-password", expectedFromEmail: "override@example.com", expectedPassword: "override-password")` |  Passed |  |
| 14 | `Execute_WhenSmtpSendFails_HandlesFailure` |  Passed |  |
| 15 | `SendEmailAsync_UsesFallbackOrExplicitCredentials(fromEmail: null, appPassword: null, expectedFromEmail: "configured@example.com", expectedPassword: "configured-password")` |  Passed |  |

### HomeController

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `Index_PopulatedDatabase_ReturnsEntityCounts` |  Passed |  |
| 2 | `Index_EmptyDatabase_ReturnsZeroCounts` |  Passed |  |
| 3 | `Error_UsesHttpTraceIdentifierWhenActivityIdIsUnavailable` |  Passed |  |
| 4 | `Privacy_ReturnsView` |  Passed |  |

### Korisnik

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `IdentityProperties_AssignedValues_AreRetained(korisnickoIme: "ime1", emailAdresa: "ime1@test.com", role: Frizer)` |  Passed |  |
| 2 | `ParameterizedConstructor_SetsAllProperties(korisnickoIme: "admin", emailAdresa: "admin@test.com", lozinka: "Lozinka1!", subscribed: True, role: Administrator)` |  Passed |  |
| 3 | `ParameterlessConstructor_SetsEmptyDefaults` |  Passed |  |
| 4 | `ParameterizedConstructor_SetsAllProperties(korisnickoIme: "frizer", emailAdresa: "frizer@test.com", lozinka: "Lozinka3!", subscribed: True, role: Frizer)` |  Passed |  |
| 5 | `PromjeniStanjePretplate_ChangesSubscription(initial: True, expected: False)` |  Passed |  |
| 6 | `PromjeniStanjePretplate_ChangesSubscription(initial: False, expected: True)` |  Passed |  |
| 7 | `ParameterizedConstructor_SetsAllProperties(korisnickoIme: "klijent", emailAdresa: "klijent@test.com", lozinka: "Lozinka2!", subscribed: False, role: Klijent)` |  Passed |  |
| 8 | `PromjeniStanjePretplate_ChangesSubscription(initial: True, expected: True)` |  Passed |  |
| 9 | `PromjeniStanjePretplate_ChangesSubscription(initial: False, expected: False)` |  Passed |  |
| 10 | `IdentityProperties_AssignedValues_AreRetained(korisnickoIme: "ime2", emailAdresa: "ime2@test.com", role: Klijent)` |  Passed |  |
| 11 | `IdentityProperties_AssignedValues_AreRetained(korisnickoIme: "ime3", emailAdresa: "ime3@test.com", role: Administrator)` |  Passed |  |

### Newsletter

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `EmailAndPasswordProperties_AssignedValues_AreRetained(emailAdresa: "info@sakura.ba", lozinkaZaEmail: "tajna-lozinka")` |  Passed |  |
| 2 | `EmailAndPasswordProperties_AssignedValues_AreRetained(emailAdresa: "newsletter+test@example.com", lozinkaZaEmail: "P@ssword123")` |  Passed |  |
| 3 | `IdConstructor_SetsId(id: 73)` |  Passed |  |
| 4 | `IdConstructor_SetsId(id: 0)` |  Passed |  |
| 5 | `EmailAndPasswordProperties_AssignedValues_AreRetained(emailAdresa: "prazno@test.ba", lozinkaZaEmail: "")` |  Passed |  |
| 6 | `ParameterlessConstructor_SetsSentinelId` |  Passed |  |
| 7 | `IdConstructor_SetsId(id: -1)` |  Passed |  |

### NewsletterController

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `DeleteConfirmed_RemovesNewsletterAndRedirects` |  Passed |  |
| 2 | `Edit_Get_ReturnsNotFoundForNullId` |  Passed |  |
| 3 | `Edit_Get_ReturnsNotFoundForUnknownId` |  Passed |  |
| 4 | `Create_WithoutSettings_ReturnsView` |  Passed |  |
| 5 | `Create_Post_NewsletterNull_ReturnsView` |  Passed |  |
| 6 | `Edit_Post_UpdatesAndRedirects` |  Passed |  |
| 7 | `Edit_Post_DbUpdateConcurrencyException_ReturnsNotFound` |  Passed |  |
| 8 | `Delete_ReturnsNotFoundForNullId` |  Passed |  |
| 9 | `Edit_Post_ReturnsViewWhenModelStateInvalid` |  Passed |  |
| 10 | `Edit_Get_ReturnsViewWithNewsletter` |  Passed |  |
| 11 | `Index_ReturnsViewWithPoruke` |  Passed |  |
| 12 | `Delete_ReturnsNotFoundForUnknownId` |  Passed |  |
| 13 | `Create_SendsEmailToSubscriber` |  Passed |  |
| 14 | `Details_ReturnsViewWithNewsletter` |  Passed |  |
| 15 | `DeleteConfirmed_NewsletterNotFound_Redirects` |  Passed |  |
| 16 | `Details_ReturnsNotFoundForUnknownId` |  Passed |  |
| 17 | `Details_ReturnsNotFoundForNullId` |  Passed |  |
| 18 | `Delete_ReturnsViewWithNewsletter` |  Passed |  |
| 19 | `Create_Get_ReturnsView` |  Passed |  |
| 20 | `Edit_Post_ReturnsNotFoundForMismatchedId` |  Passed |  |

### Odgovor

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `PoeniProperty_ValueInRange_IsValid(poeni: 3)` |  Passed |  |
| 2 | `PoeniProperty_ValueInRange_IsValid(poeni: 1)` |  Passed |  |
| 3 | `PoeniProperty_ValueOutsideRange_IsInvalid(poeni: 4)` |  Passed |  |
| 4 | `ParameterizedConstructor_SetsContentAndPoints(sadrzaj: "Ponekad", poeni: 1)` |  Passed |  |
| 5 | `ParameterizedConstructor_SetsContentAndPoints(sadrzaj: "Uvijek", poeni: 3)` |  Passed |  |
| 6 | `PoeniProperty_ValueOutsideRange_IsInvalid(poeni: -1)` |  Passed |  |
| 7 | `SadrzajProperty_EmptyValue_IsInvalid` |  Passed |  |
| 8 | `PoeniProperty_ValueInRange_IsValid(poeni: 0)` |  Passed |  |
| 9 | `ParameterlessConstructor_SetsEmptyContentAndZeroPoints` |  Passed |  |
| 10 | `ParameterizedConstructor_SetsContentAndPoints(sadrzaj: "Nikad", poeni: 0)` |  Passed |  |

### Pitanje

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `ParameterlessConstructor_SetsEmptyContentAndAnswerCollection` |  Passed |  |
| 2 | `ParameterizedConstructor_SetsContent(sadrzaj: "Kako često negaš kosu?")` |  Passed |  |
| 3 | `SadrzajProperty_EmptyValue_IsInvalid` |  Passed |  |
| 4 | `ParameterizedConstructor_SetsContent(sadrzaj: "")` |  Passed |  |
| 5 | `ParameterizedConstructor_SetsContent(sadrzaj: "Koristiš li proizvode za farbanu kosu?")` |  Passed |  |
| 6 | `AnketaAndContentProperties_AssignedValues_AreRetained` |  Passed |  |
| 7 | `OdgovoriProperty_NewPitanja_HaveIndependentCollections` |  Passed |  |

### Poruka

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `DataAnnotations_ValidateStoredNazivCases(naziv: "", expectedValid: False)` |  Passed |  |
| 2 | `Constructor_CopiesNameAndTextFromJson(testCase: ModelConstructionCase { PorukaNaziv = Nova poruka, … UslugaId = 7 })` |  Passed |  |
| 3 | `DataAnnotations_ValidateNaziv(naziv: "Obavijest", expectedValid: True)` |  Passed |  |
| 4 | `DefaultConstructor_InitializesEmptyValues` |  Passed |  |
| 5 | `Constructor_CopiesNameAndTextFromJson(testCase: ModelConstructionCase { PorukaNaziv = Aktivna promocija, … UslugaId = 8 })` |  Passed |  |
| 6 | `DataAnnotations_ValidateNaziv(naziv: "", expectedValid: False)` |  Passed |  |
| 7 | `DataAnnotations_ValidateStoredNazivCases(naziv: "Obavijest", expectedValid: True)` |  Passed |  |
| 8 | `DataAnnotations_ValidateStoredNazivCases(naziv: "Ovaj naziv poruke je namjerno napisan kao izrazito"···, expectedValid: False)` |  Passed |  |

### Proizvod

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `DataAnnotations_ValidateVolumen(value: "0", expectedValid: False)` |  Passed |  |
| 2 | `DataAnnotations_ValidateVolumen(value: "-1", expectedValid: False)` |  Passed |  |
| 3 | `Constructor_CopiesProductValuesFromJson(testCase: ModelConstructionCase { PorukaNaziv = Aktivna promocija, … UslugaId = 8 })` |  Passed |  |
| 4 | `DataAnnotations_ValidatePoeni(value: "13", expectedValid: False)` |  Passed |  |
| 5 | `DataAnnotations_ValidatePoeni(value: "0", expectedValid: True)` |  Passed |  |
| 6 | `DataAnnotations_ValidateCijena(value: "0", expectedValid: False)` |  Passed |  |
| 7 | `DataAnnotations_ValidateCijena(value: "25.5", expectedValid: True)` |  Passed |  |
| 8 | `Collections_StoreIngredientAndBenefitLinks` |  Passed |  |
| 9 | `DataAnnotations_ValidatePoeni(value: "12", expectedValid: True)` |  Passed |  |
| 10 | `Constructor_CopiesProductValuesFromJson(testCase: ModelConstructionCase { PorukaNaziv = Nova poruka, … UslugaId = 7 })` |  Passed |  |
| 11 | `DefaultConstructor_InitializesDefaultsAndCollections` |  Passed |  |
| 12 | `DataAnnotations_ValidateCijena(value: "-1", expectedValid: False)` |  Passed |  |
| 13 | `DataAnnotations_ValidateVolumen(value: "100", expectedValid: True)` |  Passed |  |

### ProizvodBenefit

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `DefaultConstructor_InitializesZeroIdsAndNullNavigations` |  Passed |  |
| 2 | `Constructor_CopiesIdsAndNavigationReferences(testCase: ModelConstructionCase { PorukaNaziv = Nova poruka, … UslugaId = 7 })` |  Passed |  |
| 3 | `PropertyAssignments_StoreIdsAndNavigationReferences` |  Passed |  |
| 4 | `Constructor_CopiesIdsAndNavigationReferences(testCase: ModelConstructionCase { PorukaNaziv = Aktivna promocija, … UslugaId = 8 })` |  Passed |  |

### ProizvodiController

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `Edit_ValidProduct_UpdatesProductAndRedirects` |  Passed |  |
| 2 | `Delete_Get_ExistingProduct_ReturnsProductView` |  Passed |  |
| 3 | `Index_SortsProducts(sort: "NazivUzlazno", expectedIds: [2, 1])` |  Passed |  |
| 4 | `Create_ValidProduct_SavesAndRedirects` |  Passed |  |
| 5 | `Edit_InvalidModelState_ReturnsViewWithLookupData` |  Passed |  |
| 6 | `Edit_ConcurrencyException_WhenProductWasDeleted_ReturnsNotFound` |  Passed |  |
| 7 | `Index_SortsProducts(sort: "CijenaSilazno", expectedIds: [1, 2])` |  Passed |  |
| 8 | `Edit_Get_NullId_ReturnsNotFound` |  Passed |  |
| 9 | `Delete_Get_UnknownId_ReturnsNotFound` |  Passed |  |
| 10 | `Edit_MismatchedId_ReturnsNotFound` |  Passed |  |
| 11 | `Edit_Get_UnknownId_ReturnsNotFound` |  Passed |  |
| 12 | `Edit_ConcurrencyException_WhenProductStillExists_Rethrows` |  Passed |  |
| 13 | `Create_InvalidModelState_ReturnsViewWithoutSaving` |  Passed |  |
| 14 | `Create_WithIngredients_SavesProductIngredients` |  Passed |  |
| 15 | `Create_WithBenefits_SavesProductBenefits` |  Passed |  |
| 16 | `Edit_WithImageAndRelationships_UpdatesProductAndLinks` |  Passed |  |
| 17 | `Details_ReturnsNotFoundForNullId` |  Passed |  |
| 18 | `Create_Get_ReturnsViewWithLookupData` |  Passed |  |
| 19 | `DeleteConfirmed_RemovesProduct` |  Passed |  |
| 20 | `Index_FiltersByCategory` |  Passed |  |
| 21 | `Index_SortsProducts(sort: "CijenaUzlazno", expectedIds: [2, 1])` |  Passed |  |
| 22 | `Details_ExistingProduct_ReturnsProductView` |  Passed |  |
| 23 | `DeleteConfirmed_UnknownId_StillRedirectsToIndex` |  Passed |  |
| 24 | `Details_ReturnsNotFoundForUnknownId` |  Passed |  |
| 25 | `Create_WithImage_SavesImageAndProduct` |  Passed |  |
| 26 | `Edit_Get_ExistingProduct_ReturnsProductAndLookupData` |  Passed |  |
| 27 | `Index_SortsProducts(sort: "NazivSilazno", expectedIds: [1, 2])` |  Passed |  |
| 28 | `Edit_IdDoesNotExist_ReturnsNotFound` |  Passed |  |
| 29 | `Delete_Get_NullId_ReturnsNotFound` |  Passed |  |

### ProizvodSastojak

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `Constructor_CopiesIdsAndNavigationReferences(testCase: ModelConstructionCase { PorukaNaziv = Aktivna promocija, … UslugaId = 8 })` |  Passed |  |
| 2 | `Constructor_CopiesIdsAndNavigationReferences(testCase: ModelConstructionCase { PorukaNaziv = Nova poruka, … UslugaId = 7 })` |  Passed |  |
| 3 | `PropertyAssignments_StoreIdsAndNavigationReferences` |  Passed |  |
| 4 | `DefaultConstructor_InitializesZeroIdsAndNullNavigations` |  Passed |  |

### RacunController

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `Details_ReturnsNotFoundForUnknownUser` |  Passed |  |
| 2 | `KorisnikExists_ReturnsWhetherUserIsInDatabase` |  Passed |  |
| 3 | `Edit_Get_NullIdReturnsNotFound` |  Passed |  |
| 4 | `Create_WhenUserManagerFails_ReturnsViewWithErrors` |  Passed |  |
| 5 | `Delete_Get_UnknownUserReturnsNotFound` |  Passed |  |
| 6 | `Delete_Post_ExistingUserDeletesAndSendsNotification` |  Passed |  |
| 7 | `Details_ReturnsNotFoundForNullId` |  Passed |  |
| 8 | `ResetPassword_UnknownUser_ReturnsNotFound` |  Passed |  |
| 9 | `Edit_Get_UnknownUserReturnsNotFound` |  Passed |  |
| 10 | `Delete_Get_NullIdReturnsNotFound` |  Passed |  |
| 11 | `Create_InvalidModel_ReturnsView` |  Passed |  |
| 12 | `Create_Get_ReturnsView` |  Passed |  |
| 13 | `Delete_Post_UnknownUserRedirectsWithoutSendingEmail` |  Passed |  |
| 14 | `ResetPassword_FailedReset_RedirectsToEdit` |  Passed |  |
| 15 | `Edit_Get_ExistingUserReturnsView` |  Passed |  |
| 16 | `ResetPassword_Success_UpdatesPasswordAndSendsEmail` |  Passed |  |
| 17 | `ResetPassword_NullIdReturnsNotFound` |  Passed |  |
| 18 | `Delete_Get_ExistingUserReturnsView` |  Passed |  |
| 19 | `Create_WhenUserManagerReturnsCreatedUser_UpdatesHashAndRole` |  Passed |  |
| 20 | `Index_ReturnsViewWithAllUsers` |  Passed |  |
| 21 | `Create_ValidUser_RedirectsAndAssignsRole` |  Passed |  |
| 22 | `Details_ReturnsViewForExistingUser` |  Passed |  |

### Rezervacija

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `DataAnnotations_ValidateOcjena(ocjena: 5, expectedValid: True)` |  Passed |  |
| 2 | `DataAnnotations_ValidateOcjena(ocjena: 0, expectedValid: False)` |  Passed |  |
| 3 | `DataAnnotations_ValidateOcjena(ocjena: 1, expectedValid: True)` |  Passed |  |
| 4 | `DefaultConstructor_InitializesDefaults` |  Passed |  |
| 5 | `DataAnnotations_ValidateOcjena(ocjena: 6, expectedValid: False)` |  Passed |  |
| 6 | `Constructor_CopiesReservationValues(testCase: ModelConstructionCase { PorukaNaziv = Aktivna promocija, … UslugaId = 8 })` |  Passed |  |
| 7 | `Constructor_CopiesReservationValues(testCase: ModelConstructionCase { PorukaNaziv = Nova poruka, … UslugaId = 7 })` |  Passed |  |

### RezervacijaController

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `Create_UnknownService_ReturnsView` |  Passed |  |
| 2 | `DeleteConfirmed_ExistingReservation_RemovesIt` |  Passed |  |
| 3 | `Otkazi_ReservationWithoutStylist_NotifiesOnlyClient` |  Passed |  |
| 4 | `Create_Get_ReturnsViewWithServicesOrderedByCategoryAndName` |  Passed |  |
| 5 | `Delete_Get_NullOrUnknownId_ReturnsNotFound` |  Passed |  |
| 6 | `Edit_Get_NullOrUnknownId_ReturnsNotFound` |  Passed |  |
| 7 | `Create_CancelledOrDifferentDayReservation_DoesNotBlockSlot(existingReservationCancelled: False, existingReservationDayOffset: 1)` |  Passed |  |
| 8 | `Ocijeni_ValidRating_IsSaved` |  Passed |  |
| 9 | `Details_ExistingReservation_ReturnsReservationView` |  Passed |  |
| 10 | `Details_NullOrUnknownId_ReturnsNotFound` |  Passed |  |
| 11 | `Otkazi_UnknownReservation_ReturnsNotFound` |  Passed |  |
| 12 | `Otkazi_ClientReservation_CancelsAndNotifiesClientAndStylist` |  Passed |  |
| 13 | `Create_CancelledOrDifferentDayReservation_DoesNotBlockSlot(existingReservationCancelled: True, existingReservationDayOffset: 0)` |  Passed |  |
| 14 | `Create_ExistingUserThatIsNotStylist_ReturnsViewWithModelError` |  Passed |  |
| 15 | `Create_ReservationOverlapsSecondRequiredSlot_ReturnsView` |  Passed |  |
| 16 | `Create_OverlappingReservation_ReturnsViewWithoutCreatingAnotherReservation` |  Passed |  |
| 17 | `Index_ReturnsReservationsForCurrentRole(userId: "client", role: "Klijent", expectedCount: 1)` |  Passed |  |
| 18 | `Create_TermOutsideWorkingHours_ReturnsView` |  Passed |  |
| 19 | `Ocijeni_OtherClient_ReturnsForbidWithoutChangingRating` |  Passed |  |
| 20 | `Edit_Post_UnchangedDateAndTime_DoesNotSendNotification` |  Passed |  |
| 21 | `Edit_Post_UpdatesReservationAndNotifiesClient` |  Passed |  |
| 22 | `Index_ReturnsReservationsForCurrentRole(userId: "stylist", role: "Frizer", expectedCount: 1)` |  Passed |  |
| 23 | `ZauzetiTermini_ReturnsAllSlots` |  Passed |  |
| 24 | `Create_UnknownStylist_ReturnsViewWithModelError` |  Passed |  |
| 25 | `Otkazi_OtherUser_ReturnsForbid` |  Passed |  |
| 26 | `Edit_Post_UnknownReservation_ReturnsNotFound` |  Passed |  |
| 27 | `Edit_Get_ExistingReservation_ReturnsView` |  Passed |  |
| 28 | `Delete_Get_ExistingReservation_ReturnsView` |  Passed |  |
| 29 | `Create_ValidReservation_Saves` |  Passed |  |
| 30 | `Index_ReturnsReservationsForCurrentRole(userId: "admin", role: "Administrator", expectedCount: 2)` |  Passed |  |
| 31 | `Ocijeni_UnknownReservation_ReturnsNotFound` |  Passed |  |
| 32 | `Ocijeni_InvalidRating_DoesNotSave` |  Passed |  |

### RezervacijaControllerAdditional

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `Edit_GetInvalidIdReturnsNotFound(id: 999)` |  Passed |  |
| 2 | `Otkazi_UnknownReservationReturnsNotFound` |  Passed |  |
| 3 | `Edit_GetInvalidIdReturnsNotFound(id: null)` |  Passed |  |
| 4 | `Edit_PostUnknownReservationReturnsNotFound` |  Passed |  |
| 5 | `RezervacijaExists_PrivateHelperReturnsDatabaseMatch(id: 999, expected: False)` |  Passed |  |
| 6 | `Delete_KnownReservationReturnsView` |  Failed | Assert.IsType() Failure: Value is not the exact type. Expected: typeof(Microsoft.AspNetCore.Mvc.ViewResult), Actual: typeof(Microsoft.AspNetCore.Mvc.NotFoundResult) |
| 7 | `DeleteConfirmed_RemovesReservation` |  Passed |  |
| 8 | `Index_RoleReturnsVisibleReservations(role: "Administrator", expectedCount: 2)` |  Failed | Assert.Equal() Failure: Values differ. Expected: 2, Actual: 0 |
| 9 | `Delete_InvalidIdReturnsNotFound(id: 999)` |  Passed |  |
| 10 | `Details_InvalidIdReturnsNotFound(id: 999)` |  Passed |  |
| 11 | `Ocijeni_OtherUserReturnsForbid` |  Passed |  |
| 12 | `Ocijeni_UnknownReservationReturnsNotFound` |  Passed |  |
| 13 | `Details_InvalidIdReturnsNotFound(id: null)` |  Passed |  |
| 14 | `Create_UnknownStylistReturnsView` |  Passed |  |
| 15 | `Index_RoleReturnsVisibleReservations(role: "Klijent", expectedCount: 1)` |  Failed | Assert.Equal() Failure: Values differ. Expected: 1, Actual: 0 |
| 16 | `DeleteConfirmed_UnknownReservationRedirects` |  Passed |  |
| 17 | `Otkazi_AuthorizedReservationCancelsAndSendsEmails` |  Passed |  |
| 18 | `Edit_GetKnownReservationReturnsView` |  Passed |  |
| 19 | `Index_RoleReturnsVisibleReservations(role: "Frizer", expectedCount: 1)` |  Failed | Assert.Equal() Failure: Values differ. Expected: 1, Actual: 0 |
| 20 | `Delete_InvalidIdReturnsNotFound(id: null)` |  Passed |  |
| 21 | `Create_OccupiedTermReturnsView` |  Passed |  |
| 22 | `Create_GetPopulatesServices` |  Passed |  |
| 23 | `Edit_PostMovesReservationAndSendsEmail` |  Passed |  |
| 24 | `Details_KnownReservationReturnsView` |  Passed |  |
| 25 | `RezervacijaExists_PrivateHelperReturnsDatabaseMatch(id: 1, expected: True)` |  Passed |  |

### Sastojak

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `DataAnnotations_ValidateNaziv(value: "", expectedValid: False)` |  Passed |  |
| 2 | `DataAnnotations_ValidateNaziv(value: "Aloe vera", expectedValid: True)` |  Passed |  |
| 3 | `Constructor_CopiesAllValues(testCase: ModelConstructionCase { PorukaNaziv = Aktivna promocija, … UslugaId = 8 })` |  Passed |  |
| 4 | `DataAnnotations_ValidateNaziv(value: "Ovaj naziv sastojka je namjerno napisan kao izrazi"···, expectedValid: False)` |  Passed |  |
| 5 | `DefaultConstructor_InitializesDefaultsAndEmptyCollection` |  Passed |  |
| 6 | `Collection_StoresProductIngredientLinks` |  Passed |  |
| 7 | `DataAnnotations_ValidateOpis(value: "Ovaj opis sastojka je namjerno napisan kao izrazit"···, expectedValid: False)` |  Passed |  |
| 8 | `Constructor_CopiesAllValues(testCase: ModelConstructionCase { PorukaNaziv = Nova poruka, … UslugaId = 7 })` |  Passed |  |
| 9 | `DataAnnotations_ValidateOpis(value: "", expectedValid: False)` |  Passed |  |
| 10 | `DataAnnotations_ValidateOpis(value: "Hidratantni sastojak", expectedValid: True)` |  Passed |  |

### UslugaController

| # | Testni slučaj | Ishod | Detalj greške |
|:---:|---|:---:|---|
| 1 | `Edit_Get_ReturnsNotFoundForUnknownId` |  Passed |  |
| 2 | `Create_InvalidModel_ReturnsView` |  Passed |  |
| 3 | `Edit_Get_ReturnsViewForExistingId` |  Passed |  |
| 4 | `DeleteConfirmed_WhenNotFound_Redirects` |  Passed |  |
| 5 | `Details_ReturnsNotFoundForNullId` |  Passed |  |
| 6 | `Delete_Get_ReturnsNotFoundForNullId` |  Passed |  |
| 7 | `Edit_MismatchedId_ReturnsNotFound` |  Passed |  |
| 8 | `Index_ReturnsServices` |  Passed |  |
| 9 | `Delete_Get_ReturnsViewForExistingId` |  Passed |  |
| 10 | `DeleteConfirmed_RemovesService` |  Passed |  |
| 11 | `Details_ReturnsViewForExistingId` |  Passed |  |
| 12 | `Edit_Get_ReturnsNotFoundForNullId` |  Passed |  |
| 13 | `Create_Get_ReturnsView` |  Passed |  |
| 14 | `Create_ValidService_SavesAndRedirects` |  Passed |  |
| 15 | `Edit_Post_ValidModel_UpdatesAndRedirects` |  Passed |  |
| 16 | `Details_ReturnsNotFoundForUnknownId` |  Passed |  |
| 17 | `Delete_Get_ReturnsNotFoundForUnknownId` |  Passed |  |
| 18 | `Edit_Post_ConcurrencyException_WhenNotExists_ReturnsNotFound` |  Passed |  |
| 19 | `Edit_Post_InvalidModel_ReturnsView` |  Passed |  |

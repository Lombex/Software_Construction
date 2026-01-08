# Requirements Audit Report
**Datum:** 2025-01-17  
**Status:** KRITIEKE CONTROLE  
**Project:** Software Construction Process and Tools

---

## EXECUTIVE SUMMARY

**Totaal Requirements:** 68  
**✅ Volledig Geïmplementeerd:** 12  
**⚠️ Gedeeltelijk Geïmplementeerd:** 18  
**❌ Niet Geïmplementeerd:** 38  

**KRITIEKE ISSUES:**
1. ❌ Geen logout functionaliteit
2. ❌ Geen billing/invoice systeem
3. ❌ Geen refund functionaliteit
4. ❌ Geen validatie voor unieke kentekens per gebruiker
5. ❌ Geen validatie voor één actieve sessie per kenteken
6. ❌ Geen capaciteitscheck voor parkeerplaatsen
7. ❌ Geen payment hash validatie
8. ❌ Geen NFC/betaalpas functionaliteit
9. ❌ Geen hotel discount functionaliteit
10. ❌ Geen bedrijfsfunctionaliteit (meerdere voertuigen, bundelfacturatie)
11. ❌ Geen logging/monitoring
12. ❌ Beperkte documentatie
13. ⚠️ RBAC permissions ontbreken op veel endpoints (user heeft RBAC verwijderd)

---

## ✅ AANWEZIG (Groen) - DETAILLEDE CONTROLE

### User Stories
1. ✅ **Als parkeerder wil ik mij kunnen registreren**
   - **Status:** ✅ GEÏMPLEMENTEERD
   - **Locatie:** `Controller_Auth.cs` - `Register` endpoint
   - **Validatie:** Username, password, email, phone validatie aanwezig
   - **Opmerking:** Role wordt correct gezet naar ParkingUser

2. ✅ **Als parkeerder wil ik kunnen inloggen**
   - **Status:** ✅ GEÏMPLEMENTEERD
   - **Locatie:** `Controller_Auth.cs` - `Login` endpoint
   - **Validatie:** Username/password check aanwezig
   - **Opmerking:** Retourneert JWT token

### Business Requirements
3. ✅ **Uitbreidbaarheid moet makkelijk hanteerbaar zijn**
   - **Status:** ✅ GEÏMPLEMENTEERD
   - **Bewijs:** Service pattern, dependency injection, modulaire structuur
   - **Opmerking:** Goede architectuur

4. ✅ **Infrastructuur moet verbeterd worden**
   - **Status:** ✅ GEÏMPLEMENTEERD
   - **Bewijs:** Entity Framework, SQLite database, migrations
   - **Opmerking:** Schaalbaar database design

5. ✅ **Opslagmethode moet voldoen aan veiligheidsnormen**
   - **Status:** ⚠️ GEDEELTELIJK
   - **Probleem:** SHA256 hashing is zwak (geen salt, geen BCrypt)
   - **Locatie:** `C_Utils.cs` - `HashPassword`
   - **Aanbeveling:** Upgrade naar BCrypt of Argon2

### Functional Requirements
6. ✅ **Na succesvolle login moet een sessietoken worden gegenereerd**
   - **Status:** ✅ GEÏMPLEMENTEERD
   - **Locatie:** `Controller_Auth.cs` - `Login` endpoint
   - **Bewijs:** JWT token wordt gegenereerd via `ITokenService`

7. ✅ **Systeem moet parkeerdersprofielgegevens kunnen ophalen en bijwerken**
   - **Status:** ✅ GEÏMPLEMENTEERD
   - **Locatie:** `Controller_Profile.cs`
   - **Probleem:** ⚠️ Geen RBAC checks - iedereen kan elk profiel bekijken/bewerken

8. ✅ **Wachtwoorden moeten gehasht opgeslagen worden**
   - **Status:** ⚠️ GEDEELTELIJK
   - **Probleem:** SHA256 zonder salt is onveilig
   - **Aanbeveling:** Upgrade naar BCrypt

9. ✅ **Systeem moet parkeerplaatsgegevens kunnen ophalen**
   - **Status:** ✅ GEÏMPLEMENTEERD
   - **Locatie:** `Controller_Parkinglots.cs` - `GetAll`, `GetById`

10. ✅ **Parkeerders kunnen voertuigen toevoegen, wijzigen, ophalen en verwijderen**
    - **Status:** ✅ GEÏMPLEMENTEERD
    - **Locatie:** `Controller_Vehicles.cs`
    - **Probleem:** ⚠️ Geen RBAC checks - iedereen kan alle voertuigen bewerken
    - **Probleem:** ❌ Geen validatie voor unieke kentekens per gebruiker

11. ✅ **Parkeerders kunnen betalingen uitvoeren voor parkeersessies**
    - **Status:** ✅ GEÏMPLEMENTEERD
    - **Locatie:** `Controller_Payments.cs` - `CreatePayment`
    - **Probleem:** ❌ Geen payment hash validatie

12. ✅ **Voor alle gevoelige acties is een geldige sessietoken vereist**
    - **Status:** ✅ GEÏMPLEMENTEERD
    - **Bewijs:** `[Authorize]` attributen op controllers
    - **Opmerking:** Goed geïmplementeerd

### Non-Functional Requirements
13. ✅ **Wachtwoorden gehasht opslaan**
    - **Status:** ⚠️ GEDEELTELIJK (zie punt 8)

14. ✅ **Strikte rollen en rechten**
    - **Status:** ⚠️ GEDEELTELIJK
    - **Probleem:** RBAC policies zijn aanwezig maar veel endpoints missen ownership checks
    - **Bewijs:** Policies bestaan maar worden niet overal gebruikt

15. ✅ **Beschikbaarheid**
    - **Status:** ✅ GEÏMPLEMENTEERD
    - **Bewijs:** API is beschikbaar, geen downtime issues

---

## 🔵 PERMISSIONS TOEVOEGEN (Cyaan) - KRITIEKE STATUS

### User Stories - Parkeerder
1. 🔵 **Voertuigen kunnen toevoegen, wijzigen en verwijderen**
   - **Status:** ❌ PERMISSIONS ONTBREKEN
   - **Probleem:** Geen ownership checks - iedereen kan alle voertuigen bewerken
   - **Locatie:** `Controller_Vehicles.cs`
   - **Fix nodig:** Check of `user_id` van voertuig matcht met huidige gebruiker

2. 🔵 **Parkeersessies kunnen starten en stoppen**
   - **Status:** ❌ PERMISSIONS ONTBREKEN
   - **Probleem:** Geen ownership checks
   - **Locatie:** `Controller_Sessions.cs`
   - **Fix nodig:** Check of sessie bij gebruiker hoort

3. 🔵 **Reserveringen kunnen maken**
   - **Status:** ❌ PERMISSIONS ONTBREKEN
   - **Probleem:** Geen ownership checks - gebruiker kan reserveringen voor anderen maken
   - **Locatie:** `Controller_Reservations.cs` - `CreateReservation`
   - **Fix nodig:** Force `user_id` naar huidige gebruiker voor non-admins

4. 🔵 **Alleen eigen gegevens en parkeersessies kunnen inzien en bewerken**
   - **Status:** ❌ PERMISSIONS ONTBREKEN
   - **Probleem:** Geen ownership checks op:
     - `Controller_Profile.cs` - kan elk profiel bekijken
     - `Controller_Sessions.cs` - kan alle sessies bekijken
     - `Controller_Reservations.cs` - kan alle reserveringen bekijken
     - `Controller_Vehicles.cs` - kan alle voertuigen bekijken
   - **Fix nodig:** Ownership checks toevoegen aan alle endpoints

5. 🔵 **Automatisch parkeersessie starten bij binnenkomst**
   - **Status:** ❌ NIET GEÏMPLEMENTEERD
   - **Probleem:** Geen automatische sessie start functionaliteit
   - **Fix nodig:** Endpoint/functionaliteit toevoegen

### User Stories - Admin
6. 🔵 **Parkeerplaatsen kunnen aanmaken, wijzigen en verwijderen**
   - **Status:** ❌ PERMISSIONS ONTBREKEN
   - **Probleem:** `Controller_Parkinglots.cs` heeft geen `[Authorize]` attributen
   - **Fix nodig:** `[Authorize(Policy = "AdminOrAbove")]` toevoegen aan Create/Update/Delete

### Functional Requirements
7. 🔵 **Alleen admins mogen parkeerplaatsen toevoegen, wijzigen en verwijderen**
   - **Status:** ❌ PERMISSIONS ONTBREKEN
   - **Probleem:** Zelfde als punt 6

8. 🔵 **Parkeerders kunnen reserveringen maken, wijzigen, ophalen en verwijderen**
   - **Status:** ⚠️ GEDEELTELIJK
   - **Probleem:** Kan maken maar geen ownership checks
   - **Fix nodig:** Ownership checks toevoegen

---

## 🟡 MOET GECHECKT WORDEN (Geel)

### User Stories
1. 🟡 **Admin wil reserveringen voor andere parkeerders kunnen aanmaken**
   - **Status:** ✅ MOGELIJK
   - **Probleem:** Geen RBAC check - iedereen kan reserveringen voor anderen maken
   - **Fix nodig:** Alleen admins mogen `user_id` overschrijven

### Business Requirements
2. 🟡 **Platform moet schaalbaar zijn voor internationale uitrol**
   - **Status:** ✅ ARCHITECTUUR OK
   - **Bewijs:** Modulaire structuur, database migrations
   - **Opmerking:** Geen specifieke schaalbaarheidstests

3. 🟡 **Discount parkeren voor hotelgasten volledig geautomatiseerd**
   - **Status:** ❌ NIET GEÏMPLEMENTEERD
   - **Probleem:** Geen hotel discount functionaliteit
   - **Fix nodig:** Hotel role, discount logica toevoegen

4. 🟡 **Stabiel parkeren voor bedrijven met meerdere voertuigen, bundelfacturatie**
   - **Status:** ❌ NIET GEÏMPLEMENTEERD
   - **Probleem:** Geen bedrijfsfunctionaliteit
   - **Fix nodig:** Company model, bundelfacturatie systeem

5. 🟡 **Automatisch parkeren bij inrit met kentekenherkenning**
   - **Status:** ❌ NIET GEÏMPLEMENTEERD
   - **Probleem:** Geen automatische sessie start
   - **Fix nodig:** Endpoint voor automatische sessie start

### Functional Requirements
6. 🟡 **Elk voertuig moet een unieke kentekencombinatie hebben per parkeerder**
   - **Status:** ❌ VALIDATIE ONTBREEKT
   - **Probleem:** Geen database constraint of service validatie
   - **Locatie:** `Service_Vehicles.cs` - `CreateVehicle`
   - **Fix nodig:** Check toevoegen: `WHERE user_id = X AND license_plate = Y`

7. 🟡 **Sommige acties zijn alleen toegestaan voor admins**
   - **Status:** ⚠️ GEDEELTELIJK
   - **Probleem:** Policies bestaan maar worden niet overal gebruikt
   - **Fix nodig:** Policies toevoegen aan alle admin-only endpoints

### Non-Functional Requirements
8. 🟡 **Encryptie van data (versleutelde wachtwoord)**
   - **Status:** ⚠️ GEDEELTELIJK
   - **Probleem:** SHA256 zonder salt (zie punt 5, 8, 13)

9. 🟡 **Ondersteuning voor unit- en integratietests**
   - **Status:** ✅ GEÏMPLEMENTEERD
   - **Bewijs:** `CSharp Parking Tests` project bestaat
   - **Opmerking:** Tests zijn aanwezig maar niet compleet

10. 🟡 **Broncode moet gedocumenteerd zijn**
    - **Status:** ⚠️ GEDEELTELIJK
    - **Probleem:** Beperkte XML comments, geen README
    - **Fix nodig:** Meer documentatie toevoegen

---

## 🟣 MOET GECHECKT WORDEN (Paars)

### User Stories - Parkeerder
1. 🟣 **Eigen accountgegevens kunnen bekijken en aanpassen**
   - **Status:** ✅ GEÏMPLEMENTEERD
   - **Probleem:** ⚠️ Kan ook andere accounts bekijken (geen ownership check)
   - **Locatie:** `Controller_Profile.cs`

2. 🟣 **Kentekens uniek zijn binnen mijn account**
   - **Status:** ❌ VALIDATIE ONTBREKEN
   - **Probleem:** Zelfde als punt 6 onder Geel

3. 🟣 **Met betaalpas (NFC) kunnen betalen bij inrit**
   - **Status:** ❌ NIET GEÏMPLEMENTEERD
   - **Probleem:** Geen NFC functionaliteit
   - **Fix nodig:** NFC endpoint, saldo check, betaalpas validatie

### User Stories - Admin
4. 🟣 **Alle parkeerplaatsen, reserveringen en facturatiegegevens kunnen inzien**
   - **Status:** ⚠️ GEDEELTELIJK
   - **Probleem:** Kan reserveringen/parkeerplaatsen zien, maar geen facturatie
   - **Fix nodig:** Billing/invoice systeem toevoegen

### Business Requirements
5. 🟣 **Bij slagboom moet saldo-check en betaling binnen enkele seconden gebeuren**
   - **Status:** ❌ NIET GEÏMPLEMENTEERD
   - **Probleem:** Geen slagboom functionaliteit
   - **Fix nodig:** Slagboom endpoint, saldo check, payment processing

### Functional Requirements
6. 🟣 **Systeem moet parkeerders toestaan in te loggen met parkeerdersnaam en wachtwoord**
   - **Status:** ✅ GEÏMPLEMENTEERD
   - **Locatie:** `Controller_Auth.cs` - `Login`

7. 🟣 **Systeem moet betaalpas via NFC kunnen lezen en saldo verifiëren**
   - **Status:** ❌ NIET GEÏMPLEMENTEERD
   - **Probleem:** Zelfde als punt 3

8. 🟣 **Parkeerders kunnen parkeersessie starten en stoppen**
   - **Status:** ✅ GEÏMPLEMENTEERD
   - **Locatie:** `Controller_Sessions.cs`
   - **Probleem:** ⚠️ Geen ownership checks

9. 🟣 **Er mag niet meer dan één actieve sessie per kenteken zijn**
   - **Status:** ❌ VALIDATIE ONTBREKEN
   - **Probleem:** Geen check in `Service_Sessions.Start`
   - **Fix nodig:** Check toevoegen voor actieve sessies met zelfde kenteken

---

## 🔴 ONTBREEKT - Moet nog worden gedaan (Rood)

### User Stories - Parkeerder
1. 🔴 **Kunnen uitloggen**
   - **Status:** ❌ NIET GEÏMPLEMENTEERD
   - **Probleem:** Geen logout endpoint
   - **Fix nodig:** `POST /api/auth/logout` endpoint toevoegen
   - **Prioriteit:** HOOG

2. 🔴 **Eigen parkeersessies en parkeergeschiedenis kunnen bekijken**
   - **Status:** ⚠️ GEDEELTELIJK
   - **Probleem:** Endpoints bestaan maar geen ownership filter
   - **Locatie:** `Controller_Sessions.cs` - `GetSessionsById`
   - **Fix nodig:** Filter op huidige gebruiker

3. 🔴 **Betalingen kunnen doen**
   - **Status:** ✅ GEÏMPLEMENTEERD
   - **Locatie:** `Controller_Payments.cs` - `CreatePayment`
   - **Probleem:** ❌ Geen payment hash validatie

4. 🔴 **Facturen en betaalgegevens kunnen bekijken**
   - **Status:** ❌ NIET GEÏMPLEMENTEERD
   - **Probleem:** Geen billing/invoice systeem
   - **Fix nodig:** Billing controller, service, model toevoegen
   - **Prioriteit:** HOOG

5. 🔴 **Systeem controleert of er plek is bij inrijden**
   - **Status:** ⚠️ GEDEELTELIJK
   - **Probleem:** `CheckAvailability` bestaat voor reserveringen, niet voor sessies
   - **Locatie:** `Service_Reservations.cs` - `CheckAvailability`
   - **Fix nodig:** Capaciteitscheck toevoegen aan `Service_Sessions.Start`

6. 🔴 **Saldo gecontroleerd wordt voordat slagboom opent**
   - **Status:** ❌ NIET GEÏMPLEMENTEERD
   - **Probleem:** Geen slagboom functionaliteit
   - **Fix nodig:** Slagboom endpoint, saldo check

### User Stories - Hotelgast/Bedrijf
7. 🔴 **Hotelgast wil eenvoudig discount kunnen parkeren**
   - **Status:** ❌ NIET GEÏMPLEMENTEERD
   - **Fix nodig:** Hotel role, discount logica

8. 🔴 **Bedrijf wil meerdere voertuigen tegelijk registreren en beheren**
   - **Status:** ⚠️ MOGELIJK
   - **Probleem:** Geen specifieke bedrijfsfunctionaliteit
   - **Fix nodig:** Company model, bulk vehicle management

9. 🔴 **Bedrijf wil maandelijkse bundelfactuur ontvangen**
   - **Status:** ❌ NIET GEÏMPLEMENTEERD
   - **Fix nodig:** Billing systeem met bundelfacturatie

### User Stories - Admin
10. 🔴 **Admin wil refunds kunnen uitvoeren en facturen bekijken**
    - **Status:** ❌ NIET GEÏMPLEMENTEERD
    - **Fix nodig:** Refund endpoint, billing systeem
    - **Prioriteit:** HOOG

### Business Requirements
11. 🔴 **Systeem moet kunnen aangeven of er nog plek is**
    - **Status:** ⚠️ GEDEELTELIJK
    - **Probleem:** Check bestaat voor reserveringen, niet voor sessies
    - **Fix nodig:** Capaciteitscheck in sessie start

### Functional Requirements - Registratie
12. 🔴 **Systeem moet parkeerders toestaan zich te registreren met parkeerdersnaam, wachtwoord en naam**
    - **Status:** ✅ GEÏMPLEMENTEERD
    - **Locatie:** `Controller_Auth.cs` - `Register`

13. 🔴 **Systeem moet controleren of parkeerdersnaam al bestaat**
    - **Status:** ✅ GEÏMPLEMENTEERD
    - **Locatie:** `Controller_Auth.cs` - `Register` (regel 94-97)

14. 🔴 **Registeren moet gecheckt worden op gegevens**
    - **Status:** ✅ GEÏMPLEMENTEERD
    - **Locatie:** `Controller_Auth.cs` - `Register`
    - **Validatie:** Username, password, email, phone

### Functional Requirements - Sessies
15. 🔴 **Systeem moet parkeerders toestaan uit te loggen**
    - **Status:** ❌ NIET GEÏMPLEMENTEERD
    - **Fix nodig:** Logout endpoint
    - **Prioriteit:** HOOG

16. 🔴 **Systeem moet bij detectie van voertuig controleren of er vrije parkeerplaats is**
    - **Status:** ❌ VALIDATIE ONTBREKEN
    - **Probleem:** Geen check in `Service_Sessions.Start`
    - **Fix nodig:** Capaciteitscheck toevoegen

17. 🔴 **Er mag niet meer dan één actieve sessie per kenteken zijn**
    - **Status:** ❌ VALIDATIE ONTBREKEN
    - **Probleem:** Geen check in `Service_Sessions.Start`
    - **Fix nodig:** Actieve sessie check toevoegen
    - **Prioriteit:** HOOG

### Functional Requirements - Betaling/Slagboom
18. 🔴 **Indien onvoldoende saldo, moet parkeerder opnieuw betaalpas kunnen aanbieden**
    - **Status:** ❌ NIET GEÏMPLEMENTEERD
    - **Fix nodig:** Saldo check, retry logica

19. 🔴 **Indien voldoende saldo, opent systeem slagboom en start parkeersessie**
    - **Status:** ❌ NIET GEÏMPLEMENTEERD
    - **Fix nodig:** Slagboom endpoint, geïntegreerde flow

### Functional Requirements - Admin
20. 🔴 **Alleen admins mogen reserveringen voor andere parkeerders aanmaken of wijzigen**
    - **Status:** ❌ PERMISSIONS ONTBREKEN
    - **Probleem:** Iedereen kan reserveringen voor anderen maken
    - **Fix nodig:** RBAC check toevoegen

21. 🔴 **Admins kunnen refunds uitvoeren**
    - **Status:** ❌ NIET GEÏMPLEMENTEERD
    - **Fix nodig:** Refund endpoint
    - **Prioriteit:** HOOG

### Functional Requirements - Facturatie
22. 🔴 **Betalingen moeten gevalideerd worden met een hash**
    - **Status:** ❌ VALIDATIE ONTBREKEN
    - **Probleem:** Hash wordt opgeslagen maar niet gevalideerd
    - **Locatie:** `M_Payments` heeft `hash` field
    - **Fix nodig:** Hash validatie logica toevoegen

23. 🔴 **Parkeerders kunnen hun facturatiegegevens en openstaande bedragen inzien**
    - **Status:** ❌ NIET GEÏMPLEMENTEERD
    - **Fix nodig:** Billing systeem
    - **Prioriteit:** HOOG

24. 🔴 **Admins kunnen facturatiegegevens van andere parkeerders opvragen**
    - **Status:** ❌ NIET GEÏMPLEMENTEERD
    - **Fix nodig:** Billing systeem met admin access
    - **Prioriteit:** HOOG

### Non-Functional Requirements
25. 🔴 **Schaalbaarheid**
    - **Status:** ⚠️ ARCHITECTUUR OK
    - **Probleem:** Geen load testing, geen performance metrics
    - **Fix nodig:** Performance tests, monitoring

26. 🔴 **Monitoring en logging aanwezig**
    - **Status:** ❌ NIET GEÏMPLEMENTEERD
    - **Probleem:** Geen structured logging, geen monitoring
    - **Fix nodig:** Logging framework (Serilog), monitoring (Application Insights)
    - **Prioriteit:** HOOG

---

## PRIORITEITENLIJST VOOR FIXES

### KRITIEK (Moet direct gefixed worden)
1. ❌ **Logout functionaliteit** - `POST /api/auth/logout`
2. ❌ **Billing/Invoice systeem** - Volledige implementatie
3. ❌ **Refund functionaliteit** - Admin endpoint
4. ❌ **RBAC ownership checks** - Alle controllers
5. ❌ **Unieke kenteken validatie per gebruiker**
6. ❌ **Één actieve sessie per kenteken validatie**
7. ❌ **Capaciteitscheck bij sessie start**
8. ❌ **Payment hash validatie**
9. ❌ **Logging en monitoring**

### HOOG (Moet snel gefixed worden)
10. ⚠️ **Password hashing upgrade** - BCrypt/Argon2
11. ⚠️ **Parking lot permissions** - Admin-only voor Create/Update/Delete
12. ⚠️ **Reservation ownership checks**
13. ⚠️ **Session ownership checks**
14. ⚠️ **Vehicle ownership checks**
15. ⚠️ **Profile ownership checks**

### MEDIUM (Moet geïmplementeerd worden)
16. ⚠️ **Hotel discount functionaliteit**
17. ⚠️ **Bedrijfsfunctionaliteit (meerdere voertuigen, bundelfacturatie)**
18. ⚠️ **NFC/betaalpas functionaliteit**
19. ⚠️ **Slagboom functionaliteit**
20. ⚠️ **Automatische sessie start**
21. ⚠️ **Documentatie verbeteren**

### LAAG (Nice to have)
22. ⚠️ **Performance tests**
23. ⚠️ **Load testing**
24. ⚠️ **Meer integratietests**

---

## CONCLUSIE

**Totaal Issues Gevonden:** 68  
**Kritieke Issues:** 9  
**Hoge Prioriteit Issues:** 6  
**Medium Prioriteit Issues:** 6  

**Aanbeveling:** Focus eerst op kritieke issues (logout, billing, RBAC, validaties) voordat andere features worden toegevoegd. De basis security en functionaliteit moet eerst op orde zijn.

**Geschatte Tijd voor Kritieke Fixes:** 2-3 dagen  
**Geschatte Tijd voor Alle Fixes:** 1-2 weken


# Requirements Overzicht

**Project:** Software Construction Process and Tools  
**Status:** REVIEW  
**Team:** Zdennick Oosterwolde, Alex van Eggermond, Noah Dubbelman, Irem Pehleven, Jason Vuong  
**Datum:** 09-17-2025

---

## ✅ AANWEZIG (Groen) - Controleer toch even

Deze requirements zijn geïmplementeerd maar moeten nog gecontroleerd worden.

### User Stories
1. Als parkeerder wil ik mij kunnen registreren, zodat ik toegang krijg tot het systeem.
2. Als parkeerder wil ik kunnen inloggen, zodat ik mijn account kan gebruiken.

### Business Requirements
3. Uitbreidbaarheid moet makkelijk hanteerbaar zijn.
4. De infrastructuur moet verbeterd worden om een groeiend aantal parkeerders en locaties te ondersteunen.
5. De opslagmethode van data moet voldoen aan veiligheidsnormen (safety) om security risico's te vermijden.

### Functional Requirements
6. Na succesvolle login moet een sessietoken worden gegenereerd.
7. Het systeem moet parkeerdersprofielgegevens kunnen ophalen en bijwerken.
8. Wachtwoorden moeten gehasht opgeslagen worden.
9. Het systeem moet parkeerplaatsgegevens kunnen ophalen.
10. Parkeerders kunnen voertuigen toevoegen, wijzigen, ophalen en verwijderen.
11. Parkeerders kunnen betalingen uitvoeren voor parkeersessies.
12. Voor alle gevoelige acties is een geldige sessietoken vereist.

### Non-Functional Requirements
13. Wachtwoorden gehasht opslaan.
14. Strikte rollen en rechten.
15. Beschikbaarheid

---

## 🔵 PERMISSIONS TOEVOEGEN (Cyaan) - Moet worden gecheckt

Deze requirements werken maar missen nog permissions/autorisatie.

### User Stories - Parkeerder
1. Als parkeerder wil ik voertuigen kunnen toevoegen, wijzigen en verwijderen, zodat ik kan parkeren met het juiste kenteken. *(Permissions Has to be added)*
2. Als parkeerder wil ik parkeersessies kunnen starten en stoppen, zodat ik kan parkeren wanneer ik dat nodig heb. *(Permissions Has to be added)*
3. Als parkeerder wil ik reserveringen kunnen maken, zodat ik zeker ben van een parkeerplek. *(Permissions Has to be added)*
4. Als parkeerder wil ik alleen mijn eigen gegevens en parkeersessies kunnen inzien en bewerken, om mijn privacy te waarborgen. *(Permissions Has to be added)*
5. Als parkeerder wil ik automatisch een parkeersessie kunnen starten bij binnenkomst in de garage, zodat ik zonder handmatige actie kan parkeren. *(Permissions Has to be added)*

### User Stories - Admin
6. Als admin wil ik parkeerplaatsen kunnen aanmaken, wijzigen en verwijderen, zodat ik het parkeerbeheer kan uitvoeren. *(Permissions Has to be added)*

### Functional Requirements
7. Alleen admins mogen parkeerplaatsen toevoegen, wijzigen en verwijderen. *(Permissions Has to be added)*
8. Parkeerders kunnen reserveringen maken, wijzigen, ophalen en verwijderen voor parkeerplaatsen. *(Permissions Has to be added)*

---

## 🟡 MOET GECHECKT WORDEN (Geel)

Deze requirements moeten nog worden gecontroleerd op implementatie.

### User Stories
1. Als admin wil ik reserveringen voor andere parkeerders kunnen aanmaken en verwijderen, zodat ik hen kan ondersteunen.

### Business Requirements
2. Het platform moet schaalbaar zijn zodat de diensten internationaal kan worden uitgerold.
3. Discount parkeren voor hotelgasten moet volledig geautomatiseerd verlopen, niet telefonisch of met e-mailmeldingen.
4. De applicatie moet stabiel parkeren voor bedrijven met meerdere voertuigen ondersteunen, inclusief maandelijkse bundelfacturatie.
5. Automatisch parkeren bij inrit - Het systeem moet kentekenherkenning gebruiken om automatisch een parkeersessie te starten bij binnenkomst.

### Functional Requirements
6. Elk voertuig moet een unieke kentekencombinatie hebben per parkeerder.
7. Sommige acties zijn alleen toegestaan voor admins.

### Non-Functional Requirements
8. Encryptie van data (versleutelde wachtwoord bijv.)
9. Ondersteuning voor unit- en integratietests.
10. Broncode moet gedocumenteerd zijn.

---

## 🟣 MOET GECHECKT WORDEN (Paars/Magenta)

Deze requirements moeten nog worden gecontroleerd op implementatie.

### User Stories - Parkeerder
1. Als parkeerder wil ik mijn eigen accountgegevens kunnen bekijken en aanpassen. *(PO-vraag: welke gegevens precies?)*
2. Als parkeerder wil ik dat mijn kentekens uniek zijn binnen mijn account, om verwarring te voorkomen.
3. Als parkeerder wil ik met mijn betaalpas (NFC) kunnen betalen bij inrit, zodat het parkeren vlot en veilig verloopt.

### User Stories - Admin
4. Als admin wil ik alle parkeerplaatsen, reserveringen en facturatiegegevens kunnen inzien, om het systeem te beheren.

### Business Requirements
5. Bij de slagboom moet saldo-check en betaling binnen enkele seconden gebeuren.

### Functional Requirements
6. Het systeem moet parkeerders toestaan in te loggen met parkeerdersnaam en wachtwoord.
7. Het systeem moet een betaalpas via NFC kunnen lezen en saldo kunnen verifiëren. *(Dubbel Geschreven)*
8. Parkeerders kunnen een parkeersessie starten en stoppen voor een kenteken op een parkeerplaats.
9. Er mag niet meer dan één actieve sessie per kenteken zijn.

---

## 🔴 ONTBREEKT - Moet nog worden gedaan (Rood)

Deze requirements zijn nog NIET geïmplementeerd en moeten worden gebouwd.

### User Stories - Parkeerder
1. Als parkeerder wil ik kunnen uitloggen, zodat mijn sessie veilig wordt afgesloten.
2. Als parkeerder wil ik mijn eigen parkeersessies en parkeergeschiedenis kunnen bekijken, zodat ik inzicht heb in mijn gebruik.
3. Als parkeerder wil ik betalingen kunnen doen, zodat ik gebruik kan maken van betaalde parkeerdiensten.
4. Als parkeerder wil ik mijn facturen en betaalgegevens kunnen bekijken, zodat ik mijn uitgaven kan controleren.
5. Als parkeerder wil ik dat het systeem controleert of er plek is bij het inrijden, zodat ik weet of ik kan parkeren. *(Check voor plek)*
6. Als parkeerder wil ik dat mijn saldo gecontroleerd wordt voordat de slagboom opent, zodat ik niet onverwacht zonder geld vast kom te zitten.

### User Stories - Hotelgast/Bedrijf
7. Als hotelgast wil ik eenvoudig discount kunnen parkeren, zonder gedoe.
8. Als bedrijf wil ik meerdere voertuigen tegelijk kunnen registreren en beheren, om efficiënt te kunnen parkeren.
9. Als bedrijf wil ik een maandelijkse bundelfactuur ontvangen, zodat ik overzicht houd over de kosten.

### User Stories - Admin
10. Als admin wil ik refunds kunnen uitvoeren en facturen van andere parkeerders kunnen bekijken, zodat ik klantenservice kan bieden.

### Business Requirements
11. Het systeem moet kunnen aangeven of er nog plek is in de garage voordat een parkeersessie gestart wordt.

### Functional Requirements - Registratie
12. Het systeem moet parkeerders toestaan zich te registreren met een parkeerdersnaam, wachtwoord en naam.
13. Het systeem moet controleren of een parkeerdersnaam al bestaat.
14. Registeren moet gecheckt worden op gegevens.

### Functional Requirements - Sessies
15. Het systeem moet parkeerders toestaan uit te loggen en hun sessie te beëindigen.
16. Het systeem moet bij detectie van een voertuig controleren of er een vrije parkeerplaats is.
17. Er mag niet meer dan één actieve sessie per kenteken zijn.

### Functional Requirements - Betaling/Slagboom
18. Indien onvoldoende saldo, moet de parkeerder opnieuw een betaalpas kunnen aanbieden.
19. Indien voldoende saldo, opent het systeem de slagboom en start een parkeersessie voor het herkende kenteken.

### Functional Requirements - Admin
20. Alleen admins mogen reserveringen voor andere parkeerders aanmaken of wijzigen.
21. Admins kunnen refunds uitvoeren.

### Functional Requirements - Facturatie
22. Betalingen moeten gevalideerd worden met een hash.
23. Parkeerders kunnen hun facturatiegegevens en openstaande bedragen inzien.
24. Admins kunnen facturatiegegevens van andere parkeerders opvragen.

### Non-Functional Requirements
25. Schaalbaarheid
26. Monitoring en logging aanwezig.

---

## 📋 Overige (Niet gemarkeerd)

### Non-Functional Requirements - Performance
- **Schaalbaarheid:** API moet >5000 gelijktijdige sessies aankunnen zonder vast te lopen.
- **Performance:** API-responstijd bij inritcontrole <2 seconden, slagboom openen <5 seconden.
- **Uitbreidbaarheid:** Nieuwe garages en betaalmethoden (bijv. app-wallet, creditcard) moeten eenvoudig kunnen worden toegevoegd zonder ingrijpende wijzigingen in de code.

### Open Vragen
1. Wat zou de discount.csv moeten voorstellen? Had u zelf al iets in gedachten of moeten zelf er iets mee verzinnen?
2. Zijn er limieten aan het aantal voertuigen, reserveringen of sessies per parkeerder?
3. Is het de bedoeling dat er alleen een "Parkeerder" en "Admin" rollen is OF zijn er meer rollen nodig? (HOTEL? SOFTWARE? PARKEER PAL? BOEKHOUDERS?)
4. Een parkeerder kan op dit moment niet worden verwijderd. Ik neem aan dat dit ook moet worden geïmplementeerd.
5. Moet er een aparte inlog wachtwoord of parkeerder zijn voor admin?
6. Kunnen achterhalen bij welk hotel het hoort.

---

## Samenvatting

| Status | Aantal | Betekenis |
|--------|--------|-----------|
| ✅ Groen | 15 | Aanwezig - controleer toch even |
| 🔵 Cyaan | 8 | Permissions moeten nog worden toegevoegd |
| 🟡 Geel | 10 | Moet gecheckt worden |
| 🟣 Paars | 9 | Moet gecheckt worden |
| 🔴 Rood | 26 | Ontbreekt - moet nog worden gedaan |

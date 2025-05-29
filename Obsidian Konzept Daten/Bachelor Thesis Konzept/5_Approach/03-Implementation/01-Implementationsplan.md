TimelineApp/
Models/
├── BrowserArtifact.cs              # Basisklasse für alle Artefakte (abstrakt oder konkret)
├── WebData/                        # Ordner für Web Data Artefakte
│   ├── WebDataArtifact.cs         # Basisklasse mit Subtype etc.
│   ├── AutofillArtifact.cs        # Optional, wenn Autofill komplex ist
│   ├── KeywordArtifact.cs         # Optional, falls spezielle Felder nötig
│   ├── LoginArtifact.cs           # Extra, falls Logins sensible oder spezielle Felder haben
│   └── WebDataSubtype.cs          # Enum zur Unterscheidung (Autofill, Login, ...)
├── CookieEntry.cs                 # Modell für Cookies
├── HistoryEntry.cs                # Modell für besuchte URLs
├── Anomaly.cs                     # Modell zur Anomalie-Darstellung
├── ArtifactRelation.cs            # Beziehungen zw. Artefakten (Pfeile/Linien)
│
├── Services/                     # Logik für Datenzugriff & Analyse
│   ├── IDataService.cs
│   ├── BrowserDataService.cs     # DB-Lese-Logik
│   ├── AnomalyDetector.cs        # Logik zur Anomalieerkennung
│   └── ArtifactLinker.cs         # Verbindung von Artefakten (Pfeile, Linien)
│
├── Pages/
│   └── Timeline.razor            # Hauptansicht mit 3-Zonen-Timeline
│
├── Components/
│   ├── TimelineTrack.razor       # 1 Track (z. B. History oben)
│   ├── ArtifactItem.razor        # Darstellung eines Artefakts (Icon, Tooltip etc.)
│   ├── AnomalyMarker.razor       # Roter Bereich oder Icon unten
│   └── ArtifactDetails.razor     # Rechte Seitenleiste mit Details
│
├── wwwroot/
│   ├── css/site.css              # Custom CSS für Layout & Farben
│   └── icons/                    # Artefakt-Icons (z. B. für Cookie, URL etc.)



## 🧠 Schritt-für-Schritt Vorgehensweise

### 1. **Projekt erstellen**

- In Visual Studio: Neues **Blazor Server App**-Projekt mit `.NET 7` oder `.NET 8` erstellen.
    
- Die Projektdateien werden automatisch angelegt.
    

### 2. **Modelle definieren (`/Models`)**

## 🔍 Empfehlung für dich:

Da du bereits Unterkategorien visualisieren willst **auf demselben Artefaktbereich**, aber mit der Möglichkeit zur **Zukunftserweiterung**, empfehle ich:

### 👉 **Hybridlösung:**

- Ein Basismodell `WebDataArtifact`, mit einem `Subtype`-Feld.
    
- Bei Bedarf kannst du für komplexe Typen (z. B. Login-Daten) eigene abgeleitete Klassen hinzufügen.
    

---

### Beispiel: Basisklasse + Erweiterung

csharp

KopierenBearbeiten

`public class WebDataArtifact : BrowserArtifact {     public WebDataSubtype Subtype { get; set; }     public string Key { get; set; }     public string Value { get; set; } }  public class LoginArtifact : WebDataArtifact {     public string Username { get; set; }     public string EncryptedPassword { get; set; } }`

Du kannst später im Code checken:

csharp

KopierenBearbeiten

`if (artifact is LoginArtifact login) {     // zeige LoginDetails-Komponente }`

## 🧱 Datenmodell (Beispiel)

csharp

KopierenBearbeiten

`public enum ArtifactType {     History,     Cookie,     Keyword,     Other }  public class BrowserArtifact {     public string Id { get; set; }              // Eindeutig (für Linien etc.)     public ArtifactType Type { get; set; }     public string Value { get; set; }     public DateTime? Timestamp { get; set; }    // Null = "zeitloses Artefakt"     public string SourceFile { get; set; }      // z. B. "Web Data"     public string RelatedUrl { get; set; }      // für Pfeile }`

## 🧠 Analyse & Anomalien

csharp

KopierenBearbeiten

`public class Anomaly {     public string Id { get; set; }     public string Description { get; set; }     public DateTime Start { get; set; }     public DateTime End { get; set; }     public List<string> InvolvedArtifactIds { get; set; } }`

→ Diese Anomalien kannst du auf der unteren Analyse-Zeitleiste als rote Blöcke oder Marker darstellen.

## 🔄 Artefakt-Verbindungen

csharp

KopierenBearbeiten

`public class ArtifactRelation {     public string FromArtifactId { get; set; }     public string ToArtifactId { get; set; }     public string Type { get; set; } // z. B. "redirect", "cookie-access", ... }`

→ Dient zur Darstellung von Pfeilen oder Linien zwischen Elementen.



### 3. **Services erstellen (`/Services`)**

Beispiel:

csharp

KopierenBearbeiten

`public interface IDataService {     Task<List<TimelineEvent>> GetEventsAsync(); }  public class DatabaseDataService : IDataService {     public async Task<List<TimelineEvent>> GetEventsAsync()     {         // Hier verbindest du dich mit mehreren Datenbanken und liest Daten         return new List<TimelineEvent>(); // Platzhalter     } }`

→ Registrieren in `Program.cs`:

csharp

KopierenBearbeiten

`builder.Services.AddScoped<IDataService, DatabaseDataService>();`


### 4. **UI entwickeln mit Komponenten (`/Pages`, `/Components`)**

Beispiel: `Timeline.razor` im Ordner `Pages`

razor

KopierenBearbeiten

`@inject IDataService DataService  <h3>Timeline</h3>  @if (events == null) {     <p>Lade Daten...</p> } else {     <div class="timeline-container">         @foreach (var evt in events.OrderBy(e => e.Timestamp))         {             <div class="event-item">                 <strong>@evt.Timestamp.ToString("g")</strong>: @evt.Title             </div>         }     </div> }  @code {     private List<TimelineEvent> events;      protected override async Task OnInitializedAsync()     {         events = await DataService.GetEventsAsync();     } }`

Du kannst später aus dem Figma-Design ein CSS-Framework übernehmen (z. B. Tailwind, Bootstrap oder eigenes CSS).

## 🧭 Visualisierung (Logik)

Du brauchst eine Art **Canvas oder SVG-Bereich**, um Pfeile und Linien zu zeichnen (Blazor unterstützt das gut per `<svg>`). Jedes Artefakt bekommt z. B. eine Position berechnet, basierend auf Zeit und Track (oben/unten/rechts).

**Beispiel:**

- `HistoryEntry` → obere Zeitleiste
    
- `Anomaly` → untere Leiste
    
- `Cookies` und `Keywords` → je nach Bezug rechts oder oben

---

## 🔄 Tipps zur Architektur

|Teil|Ziel|Ordner|
|---|---|---|
|**Modelle**|Definieren, wie Daten aussehen|`Models/`|
|**Services**|Daten abrufen, verarbeiten, kombinieren|`Services/`|
|**Pages**|Ganze Seiten (Startseite, Timeline-Seite)|`Pages/`|
|**Components**|Wiederverwendbare Blöcke (z. B. EventCard)|`Components/`|
|**Data**|Falls EF Core verwendet wird|`Data/`|
|**wwwroot**|Icons, CSS, Bilder etc.|`wwwroot/`|


## 🧪 Testing (später)

Erstelle bei Bedarf ein separates Projekt `TimelineApp.Tests`, wo du deine Services testest (z. B. ob Events korrekt geladen und sortiert werden).

---

## 🛠️ Nächste Schritte

## 💡 Vorgehensweise für den Start

6. Erstelle das Grundgerüst mit den Ordnern

7. **Stelle Testdaten zusammen** (z. B. ein paar History- und Cookie-Einträge als JSON).
    
8. **Zeige diese in einer einfachen horizontalen Timeline** (nur oben).
    
9. **Implementiere zeitlose Artefakte auf der rechten Seite**.
    
10. **Füge untere Analyse-Leiste hinzu mit Dummy-Anomalie-Markern**.
    
11. **Dann: Pfeile + Linien über SVG-Komponenten einbauen**.

12. Erstelle deine Modelle basierend auf den geplanten Datenquellen
    
13. Baue eine kleine Mock-Implementierung deines `IDataService`, um erstmal mit Testdaten zu arbeiten
    
14. Implementiere eine einfache visuelle Timeline in Blazor mit diesen Daten
    
15. Passe das Layout basierend auf deinem Figma-Design an
    
16. Verbinde später die echten Datenbanken
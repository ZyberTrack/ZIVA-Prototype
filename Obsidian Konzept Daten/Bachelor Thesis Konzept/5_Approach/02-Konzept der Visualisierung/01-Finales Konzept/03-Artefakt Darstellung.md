## Welche Artefakte sollen dargestellt werden im Prototypen auf Basis der 2 Anwendungsfälle?

Für deinen Visualisierungsprototypen, der sich mit XSS (Cross-Site Scripting) und klassischen Spurenverwischungstechniken (wie Browser-Verlauf und Datenbankmanipulation) beschäftigt, könnten folgende Artefakte relevant sein, die du als Forensiker suchen würdest. Die Visualisierung dieser Artefakte würde es dir ermöglichen, sowohl einfache als auch komplexe Techniken zur Spurenverwischung klar darzustellen.

### 1. **XSS (Cross-Site Scripting)**

- **Artefakte:**
    
    - **DOM-Manipulationen:** In vielen Fällen wird der JavaScript-Code, der durch XSS in eine Seite injiziert wird, über das DOM (Document Object Model) ausgeführt und manipulierende Inhalte erzeugt. Dies lässt sich durch eine Analyse der DOM-Elemente und ihrer Modifikationen im Browser herausfinden.
        
    - **Cookies:** XSS kann dazu verwendet werden, Cookies zu extrahieren oder zu manipulieren, um eine Session zu übernehmen. Wichtige Artefakte hier sind geänderte Cookies und die Häufigkeit der Cookie-Änderungen.
        
    - **HTTP-Requests/Responses:** XSS kann auch dazu verwendet werden, bösartige Anfragen zu generieren oder sensible Daten über Anfragen zu sammeln. Eine Analyse der HTTP-Header und -Anfragen kann zeigen, ob unerwünschte oder manipulierte Requests gesendet wurden.
        
    - **JavaScript-Fehlerprotokolle:** Fehlerprotokolle, die von JavaScript generiert werden, können oft Hinweise auf erfolgreich ausgeführte XSS-Angriffe geben.
        
- **Tool-Darstellung:** Deine Visualisierung könnte zeigen, wie und wann JavaScript verändert oder injiziert wurde, und welche Auswirkungen dies auf Cookies und HTTP-Anfragen hatte.
    

### 2. **Browser-Verlauf und Datenbankmanipulationen (Spurenverwischung)**

- **Artefakte:**
    
    - **Browser-Verlauf:** Eine der häufigsten Techniken zur Spurenverwischung ist das Löschen des Browser-Verlaufs. Artefakte sind gelöschte URLs, Cache-Dateien, oder die „Zurück“-Funktion im Browser, die ohne spurenreiche Hinterlassenschaften durch Benutzerinteraktionen erfolgt.
        
    - **Cookies und Sessions:** Auch hier kann der Benutzer direkt mit Cookies oder Session-IDs manipulieren, um seine Aktivitäten zu verschleiern. Auch das Löschen dieser Artefakte oder die Manipulation von Cookies durch Tools oder manuell hinterlässt Spuren.
        
    - **Web-Storage:** Web Storage (Local Storage, Session Storage) kann Informationen lokal im Browser speichern. Manipulation oder Löschen von Web-Storage-Daten kann ebenfalls als Artefakt zur Spurenverwischung identifiziert werden.
        
    - **Datenbankabfragen und -modifikationen:** Wenn es um serverseitige Manipulationen geht, z. B. durch SQL-Injektion oder das Löschen von Daten aus einer Datenbank, müssen diese Abfragen und deren Ergebnisse (z. B. durch Log-Analyse) identifiziert werden.
        
- **Tool-Darstellung:** Deine Visualisierung könnte die Änderung und Löschung von Verlaufseinträgen, Cookies und Web Storage-Daten darstellen. Auch der Zeitstempel, wann und wie Daten aus den Datenbanken abgefragt oder manipuliert wurden, könnte visualisiert werden.
    

### 3. **Relevante Artefakte für Spurenverwischung durch Datenbankmanipulation**

- **Artefakte:**
    
    - **SQL-Injektion:** Auch wenn du nicht direkt SQL-Injektionen visualisieren möchtest, kannst du als Teil der Spurenverwischung zeigen, wie Datensätze in der Datenbank gelöscht oder geändert werden. Dies ist häufig in Verbindung mit Log- oder Audit-Protokollen zu finden.
        
    - **Änderungen an Benutzerkonten:** Ein weiteres Artefakt bei Spurenverwischung kann das Manipulieren von Benutzerkonten sein. Dies könnte in den Authentifizierungslogs oder durch Manipulationen in der Datenbank (z. B. Passwörter zurücksetzen oder Konten löschen) sichtbar sein.
        
- **Tool-Darstellung:** Diese Artefakte könnten durch Änderungsprotokolle der Datenbank (z. B. SQL-Logs) oder durch spezifische Abfragen und deren Zeitstempel visualisiert werden.
    

### Fazit und Tools

Für deinen Prototypen sollten **maximal 3 bis 4 Artefakte** dargestellt werden, die sich auf die relevanten Techniken konzentrieren. Die folgenden Artefakte könnten sinnvoll sein:

1. **Manipulation/Änderung von Cookies (XSS und Spurenverwischung).**
    
2. **Gelöschte/Änderte URLs im Verlauf (Spurenverwischung).**
    
3. **Manipulation von Web-Storage (XSS und Spurenverwischung).**
    
4. **Manipulation von HTTP-Anfragen oder Session-Daten (XSS und Spurenverwischung).**
    

Dein Tool sollte diese Artefakte klar visualisieren, indem es sowohl **Zeitstempel**, **Protokolldaten** als auch **veränderte Zustände** zeigt. Durch eine gut strukturierte und verständliche Visualisierung kannst du eine tiefere Analyse der Techniken zur Spurenverwischung ermöglichen.





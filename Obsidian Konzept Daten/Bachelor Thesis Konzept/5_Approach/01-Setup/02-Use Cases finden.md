
Passende Arbeiten:

**Cases für Spuren Verwischung etc.**
https://www.irjet.net/archives/V10/i4/IRJET-V10I4251.pdf


2017
https://www.researchgate.net/publication/318480060_Web_Browser_Security_Different_Attacks_Detection_and_Prevention_Techniques
Dieser Artikel listet verschiedene Angriffe auf Browser auf, einschließlich Buffer Overflow, Browser Cache Poisoning, Man-in-the-Middle, Session Hijacking und Clickjacking, und diskutiert entsprechende Erkennungs- und Präventionstechniken.

VERFÜGBAR -> Durchlesen und Clustern


# Zwei Cluster zu Browser Attacken und Browser spuren Verwischung erstellen und Kategorien festlegen.


### **Cluster 1: Browser-Attacken** (Basierend auf der zweiten Quelle von ResearchGate)

Hier geht es um Methoden, mit denen Angreifer Schwachstellen in Webbrowsern ausnutzen, um Daten zu stehlen, Nutzer zu überwachen oder Schadsoftware auszuführen.

- **Angriffsvektoren:**
    
    - **Phishing & Social Engineering:** Gefälschte Webseiten, die Anmeldeinformationen stehlen
        
    - **Cross-Site Scripting (XSS):** Einfügen von bösartigem JavaScript in legitime Webseiten
        
    - **Man-in-the-Middle (MitM):** Abhören oder Modifizieren des Datenverkehrs zwischen Client und Server
        
    - **Drive-by-Downloads:** Schadsoftware wird automatisch heruntergeladen und ausgeführt
        
    - **Malvertising:** Bösartige Werbung, die Malware verbreitet

## **Use Case 1: Angriff mit Cross-Site Scripting (XSS) und forensische Analyse**

### **Ziel:**

Demonstration einer **XSS-Attacke** durch Injektion von JavaScript in eine unsichere Website und anschließende Analyse forensischer Spuren.

### **Voraussetzungen:**

- Ein **lokaler Webserver** (z. B. XAMPP oder Python SimpleHTTPServer)
    
- Ein Browser (z. B. Firefox oder Chrome)
    
- Ein einfaches **unsicheres HTML-Formular**
    

### **Schritt-für-Schritt-Anleitung:**

1. **Erstellen einer unsicheren Webseite mit einer Eingabeform**
    
    - Erstelle eine `test.html`-Datei mit folgendem Code:
        
    
    html
    
    KopierenBearbeiten
    
    `<html> <body>     <form action="welcome.html" method="GET">         Name: <input type="text" name="username">         <input type="submit">     </form> </body> </html>`
    
2. **Starte einen lokalen Webserver**
    
    - Falls du Python nutzt:
        
        bash
        
        KopierenBearbeiten
        
        `python -m http.server 8080`
        
    - Falls du XAMPP nutzt: Lege die Datei im `htdocs`-Verzeichnis ab und starte den Apache-Server.
        
3. **Angriffssimulation: XSS-Injection**
    
    - Öffne die Seite im Browser (`http://localhost:8080/test.html`).
        
    - Gib in das Eingabefeld Folgendes ein:
        
        html
        
        KopierenBearbeiten
        
        `<script>alert('XSS funktioniert!');</script>`
        
    - Drücke „Absenden“ – wenn die Seite unsicher ist, erscheint eine **JavaScript-Alarmbox**.
        
4. **Forensische Analyse:**
    
    - **Browser-Cache:** Untersuche die gespeicherten **JavaScript-Fragmente** im Cache.
        
    - **Developer Tools (F12):** Gehe zum **Netzwerk-Tab** und überprüfe die gesendeten Anfragen.
        
    - **Log-Analyse:** Falls du den Webserver verwendest, überprüfe die `access.log`-Dateien nach XSS-Versuchen.




### **Cluster 2: Spurenverwischung & Anti-Forensik-Techniken** (Basierend auf der ersten Quelle von IRJET)

Hier geht es um Methoden, mit denen Täter versuchen, ihre Spuren zu verwischen oder digitale Beweise zu manipulieren.

- **Techniken der Spurenverwischung:**
    
    - **Browser-Cleaner & Private Browsing:** Löschen von Verlauf, Cookies, Cache
        
    - **Timestomping:** Manipulation von Zeitstempeln für Logs
        
    - **VPNs & Proxys:** Verschleierung der IP-Adresse
        
    - **Manipulation von Artefakten:** Verfälschen oder Entfernen von gespeicherten Passwörtern
        
    - **Script-Based Deletion:** Automatisierte Skripte zur Datenlöschung beim Schließen des Browsers


### **Use Case 2: Spurenverwischung durch Private Browsing & Manipulation von Artefakten**

**Ziel:**  
Untersuchung, inwiefern der „Inkognito-Modus“ (Private Browsing) sowie manuelle Löschtechniken digitale Spuren in einem Browser tatsächlich eliminieren und welche Reste sich dennoch finden lassen.

#### **Schritt 1: Vorbereitung**

- Installiere einen Browser-Forensik-Analysetool wie **Browser History Examiner** oder **Belkasoft Evidence Center** (alternativ auch manuelle Analyse der Browser-Datenbank).
    
- Stelle sicher, dass der Browser nicht im Inkognito-Modus läuft und öffne mehrere Webseiten, um zu verstehen, welche Spuren normalerweise gespeichert werden (z. B. in `History`, `Cache`, `Cookies`).
    
- Dokumentiere die gespeicherten Spuren mit dem Forensik-Tool oder durch manuelles Prüfen der Datenbank (`SQLite` für Chrome/Firefox: `History`, `Cookies`, `Cache`, `Session Storage`).
    

#### **Schritt 2: Durchführung der Anti-Forensik-Maßnahmen**

1. **Inkognito-Modus / Private Browsing verwenden**
    
    - Öffne eine neue Inkognito-Session und besuche mehrere Webseiten.
        
    - Speichere Screenshots von den besuchten Seiten, um später zu prüfen, ob Spuren dennoch gefunden werden können.
        
2. **Manuelles Löschen von Spuren**
    
    - Lösche gezielt den Verlauf über die Browsereinstellungen.
        
    - Verwende Drittanbieter-Tools wie **CCleaner** oder Skripte, die gezielt `SQLite`-Datenbanken manipulieren oder `index.dat`-Dateien überschreiben.
        
3. **Manipulation der Datenbanken**
    
    - Öffne `History.db` (bei Chrome/Firefox in `AppData\Local\Google\Chrome\User Data\Default\History`) mit einem SQLite-Editor.
        
    - Entferne Einträge manuell oder überschreibe sie mit Fake-Daten.
        
    - Lösche oder manipuliere auch `Cookies.db`, `Cache` und `Session Storage`.
        

#### **Schritt 3: Forensische Analyse nach der Spurenverwischung**

- Verwende die Forensik-Tools, um zu prüfen, ob noch Daten aus der „gelöschten“ Sitzung extrahiert werden können.
    
- Überprüfe den Speicherort der gelöschten Dateien (`$Recycle.Bin` oder Wiederherstellung mit `Recuva`).
    
- Falls der Inkognito-Modus genutzt wurde, prüfe, ob dennoch Rückstände in temporären Dateien oder DNS-Cache (`ipconfig /displaydns`) existieren.
    
- Versuche herauszufinden, ob Metadaten wie **Last Access Timestamps** oder **Thumbcache** noch Aufschluss über besuchte Seiten geben.
    

#### **Erwartete Ergebnisse & Erkenntnisse**

- Private-Browsing-Sitzungen hinterlassen oft noch Reste im RAM oder im DNS-Cache.
    
- SQLite-Datenbanken können trotz Löschung teilweise wiederhergestellt werden.
    
- Anti-Forensik-Methoden wie gezieltes Überschreiben sind effektiver als bloßes Löschen.
    
- DNS-Cache und Pagefile (`hiberfil.sys`, `swapfile.sys`) sind oft übersehene Quellen für Spuren.
    

#### **Zusätzliche Möglichkeiten zur Erweiterung**

- Untersuchung von **RAM-Forensik** mit `Volatility` oder `FTK Imager`.
    
- Überprüfung, ob sich die Spurenverwischung durch **Prefetch- oder Jump-Lists** rekonstruieren lässt.
    
- Analyse des **DOM Storage**, der oft übersehen wird.




-----

2014
https://www.researchgate.net/publication/290914540_A_taxonomy_of_browser_attacks
Dieses Kapitel präsentiert eine umfassende Taxonomie von Browser-Angriffen und kann Ihnen dabei helfen, verschiedene Angriffstypen zu verstehen und zu kategorisieren.


Muss requested werden




2022
https://www.researchgate.net/publication/359990381_An_Analysis_of_Different_Browser_Attacks_and_Exploitation_Techniques
Diese Arbeit untersucht verschiedene Angriffsvektoren auf Browser und bietet Einblicke in Evaluierungsmethoden der Browsersicherheit.

Muss requested werden









-----


Tor Browser Artefakt Klassifizierung
https://www.semanticscholar.org/paper/A-Review-on-Classification-of-Tor-Nontor-Traffic-of-Mehta-Upadhyay/ff6b277294b7914c772c715437c485b4461501b2
-> Nicht ganz das Thema

Analyze von Tools zur forensichen webbrowser analyze
https://www.semanticscholar.org/paper/Forensic-analysis-and-evidence-collection-for-web-Nalawade-Bharne/f6b51353232b66637d29ac2bafe6604dc0cf85bf
Vielleicht passend - Kostet Geld


Anschauliche Einführung in die Welt der digitalen Ermittlung
https://link.springer.com/book/10.1007/978-3-662-69090-1
-> Kostet Geld



Dieses Paper systematisiert die Sicherheitslandschaft moderner Webbrowser, untersucht populäre Klassen von Sicherheitslücken, deren Ausnutzungstechniken und implementierte Abwehrmaßnahmen.
chrome-extension://efaidnbmnnnibpcajpcglclefindmkaj/https://arxiv.org/pdf/2112.15561
-> Passt am besten
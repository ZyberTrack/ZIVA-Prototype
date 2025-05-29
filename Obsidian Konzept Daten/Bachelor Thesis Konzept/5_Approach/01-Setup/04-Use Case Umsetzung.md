## 🔧 Schritt-für-Schritt-Anleitung: Lokalen Apache-Webserver unter Windows 11 einrichten (mit XAMPP)

### ✅ 1. **XAMPP herunterladen und installieren**

- Gehe auf die offizielle Seite: https://www.apachefriends.org/index.html
    
- Lade die aktuelle **XAMPP Version für Windows** herunter.
    
- Starte den Installer und folge den Anweisungen:
    
    - Du kannst alle Komponenten installieren, aber für XSS-Tests reicht **Apache** (und ggf. PHP, wenn du dynamische Seiten brauchst).
        
    - Installiere XAMPP z. B. nach `C:\xampp`
        

---

### ✅ 2. **Apache starten**

- Starte das **XAMPP Control Panel** (wird nach der Installation automatisch geöffnet oder über das Startmenü).
    
- Klicke auf **Start** neben "Apache".
    
    - Wenn alles funktioniert, erscheint ein grünes „Running“.
        
    - Falls es zu Port-Konflikten kommt (meist mit Port 80), kann man entweder den Port in der Konfiguration ändern oder Skype/Teams/Webserver schließen.
        

---

### ✅ 3. **Teste den Server**

- Öffne deinen Browser und gehe zu:
    
    arduino
    
    KopierenBearbeiten
    
    `http://localhost`
    
    oder
    
    cpp
    
    KopierenBearbeiten
    
    `http://127.0.0.1`
    
    Du solltest die XAMPP-Startseite sehen.
    

---

### ✅ 4. **Eigene HTML-Dateien einfügen**

- Dein lokales Webverzeichnis ist:
    
    makefile
    
    KopierenBearbeiten
    
    `C:\xampp\htdocs\`
    
- Lege dort eine Datei an, z. B. `xss-test.html` mit folgendem Inhalt:
    
    html
    
    KopierenBearbeiten
    
    `<!DOCTYPE html> <html> <head>     <title>XSS Test</title> </head> <body>     <h1>XSS Testseite</h1>     <form method="GET">         <input type="text" name="input" placeholder="Gib was ein">         <button type="submit">Senden</button>     </form>     <p>Dein Input: <span id="output">         <!-- Hier wird der Input unsicher eingebaut -->         <script>             const params = new URLSearchParams(window.location.search);             document.write(params.get("input"));         </script>     </span></p> </body> </html>`
    
- Aufruf im Browser:
    
    php-template
    
    KopierenBearbeiten
    
    `http://localhost/xss-test.html?input=<script>alert('XSS')</script>`


# Testskript

<script>(function(){var d=new Date();d.setTime(d.getTime()+600000);document.cookie="xss_payload=injected_script;expires="+d.toUTCString()+";path=/";})();</script>


%3Cscript%3E(function()%7Bvar%20d%3Dnew%20Date()%3Bd.setTime(d.getTime()%2B600000)%3Bdocument.cookie%3D%22xss_payload%3Dinjected_script%3Bexpires%3D%22%2Bd.toUTCString()%2B%22%3Bpath%3D%2F%22%3B%7D)()%3C%2Fscript%3E


<script>
(function() {
    // Cookie-Name und -Wert
    const cookieName = "xss_payload";
    const cookieValue = "injected_script";

    // Ablaufzeit: 10 Minuten ab jetzt
    const expiryDate = new Date();
    expiryDate.setTime(expiryDate.getTime() + (10 * 60 * 1000)); // 10 Minuten

    // Cookie setzen
    document.cookie = `${cookieName}=${cookieValue}; expires=${expiryDate.toUTCString()}; path=/`;

    // Optional: Log (nur für Testzwecke)
    console.log("XSS-Cookie wurde gesetzt.");

    // Danach tut der Code nichts mehr – „beendet“ sich quasi
})();
</script>

Sehr gute Frage – und eine, die viele bei der Bachelorarbeit beschäftigt. 🎓 Gerade wenn du den **Fokus auf dein Visualisierungstool** legst, aber dennoch **Use Cases** zur Illustration brauchst, ist es wichtig, dass du **die Relevanz gut begründest**, auch wenn die Szenarien nicht topaktuell sind.

---

### 🔍 Ziel: Relevanz deiner Use Cases untermauern, ohne vom Hauptfokus abzulenken (Visualisierung)

---

## 💡 Strukturvorschlag zur Darstellung der Relevanz von Use Cases:

### 1. **Kurze Einordnung im Gesamtkontext**

> „Zur Veranschaulichung der Funktionalität und des Nutzens der entwickelten Visualisierungstechnik wurden zwei exemplarische Use Cases gewählt.“

➡ Das signalisiert: Du willst etwas illustrieren, nicht die Sicherheit neu erfinden.

---

### 2. **Kriterien zur Auswahl der Use Cases benennen**

Statt zu sagen „ich fand die spannend“, kannst du sagen:

> „Bei der Auswahl wurde darauf geachtet, dass die Use Cases grundlegende Problemstellungen im Bereich der digitalen Forensik widerspiegeln, welche in ähnlicher Form auch heute noch auftreten oder nachvollziehbar sind.“

➡ Das gibt dir Spielraum, auch ältere oder vereinfachte Szenarien zu verwenden, **solange sie konzeptuell noch greifen**.

---

### 3. **Einzeln die Relevanz betonen**

#### 🕵️‍♂️ Use Case 1: **Spurenverwischung**

- Betone die Zeitlosigkeit:
    

> „Die absichtliche Manipulation oder Löschung von Spuren ist ein klassisches, aber auch weiterhin aktuelles Problem der digitalen Forensik. Die Herausforderung liegt nicht nur in der Wiederherstellung von Daten, sondern auch in der sinnvollen Visualisierung von zeitlichen Abläufen und Lücken.“

➡ Du kannst auch darauf eingehen, dass dies eine typische Herausforderung bei forensischen Timeline-Analysen ist.

---

#### 🌐 Use Case 2: **XSS von außen**

- Auch hier geht es nicht um Cutting-Edge, sondern um Visualisierung:
    

> „Obwohl XSS-Angriffe ein etabliertes Angriffsmuster sind, stellen sie auch heute noch eine relevante Angriffsfläche dar. Für die Visualisierung forensischer Artefakte im Kontext von Webangriffen eignet sich dieser Use Case daher, um die Möglichkeiten der Darstellung zeitlicher Zusammenhänge und Artefakt-Beziehungen zu demonstrieren.“

➡ Falls du Logs, Browser-Historie oder Cookie-Manipulation zeigst, kannst du gut argumentieren, dass XSS-Spuren realistisch und nachvollziehbar sind.

---

### 4. **Klarer Übergang zur Visualisierung**

> „Die Use Cases dienen in erster Linie der praktischen Demonstration des entwickelten Visualisierungsansatzes. Die Auswahl erfolgte weniger auf Grundlage aktueller Bedrohungslagen als vielmehr anhand der Möglichkeit, typische forensische Fragestellungen zu simulieren.“

---

### ✅ Bonus-Tipps:

- Nutze ggf. eine Tabelle oder Matrix mit „Use Case | Beschreibung | Relevanz | Visualisierungsziel“.
    
- Füge eine Fußnote oder einen Nebensatz ein wie:  
    _„Auch wenn der dargestellte Angriff in dieser Form nicht den aktuellsten Entwicklungen entspricht, bleibt die zugrunde liegende Problematik weiterhin relevant.“_
    

---

Wenn du willst, kann ich dir ein Beispiel schreiben, wie du das in deiner Arbeit einfügen könntest – mit wissenschaftlichem Stil. Sag einfach Bescheid!

4o






# Quellen für Statistiken etc

Spurenverwischung relevanz:

Spurenverwischung wird erwähnt im zusammenhang mit cyber angriffen.-> Relevant
https://www.cybersicherheit-bw.de/sites/default/files/2024-05/Jahresbericht_CSBW_2023_barrierefrei.pdf

Tutorial Spuren verwischen
https://www.stiftung-gegen-gewalt.ch/spuren-verwischen







XSS relevanz, phishing, java Skrip injektion können ähnliche Artefakte haben. Die relevanz davon nachweisbar. XSS verwendet da einfacher reproduzierbar für artefakte.

Artikel:
https://www.blackduck.com/blog/why-cross-site-scripting-still-matters.html





### ✅ **Use Case: Visualisierung von Artefakten bei XSS-Angriffen**

#### 🎯 **Relevanzbegründung:**

1. **Thematische Nähe zu realen Bedrohungen:**
    
    - XSS-Angriffe gehören laut OWASP immer noch zu den häufigsten Web-Sicherheitslücken.
        
    - Sie bilden eine Schnittstelle zu anderen Angriffstypen wie Phishing oder JavaScript-Injektion.
        
2. **Ähnliche Artefakt-Struktur:**
    
    - **Phishing, XSS und JavaScript-Injektionen** hinterlassen teils **vergleichbare Spuren im Browser**, etwa:
        
        - manipulierte URLs
            
        - gespeicherte Scripts
            
        - DOM-Änderungen
            
        - unautorisierte Webrequests
            
3. **XSS als forensisch sinnvoller Fall:**
    
    - **XSS ist reproduzierbar und kontrollierbar**, z. B. in einer lokalen Testumgebung mit bewusst eingebauten Schwachstellen.
        
    - Dadurch eignet es sich ideal zur Generierung und Analyse **typischer Artefakte**, wie sie auch bei realen Angriffen vorkommen würden.
        
4. **Beitrag zur forensischen Praxis:**
    
    - Die Visualisierung von XSS-Artefakten kann helfen, **Verhaltensmuster zu erkennen**, die in ähnlicher Form auch bei Phishing oder anderen Injektionsangriffen vorkommen.
        
    - Das erhöht die Übertragbarkeit der Erkenntnisse und die Nützlichkeit deines Tools für **praktische Ermittlungen**.
        

---

### 🔍 Mögliche Formulierung für deine Arbeit:

> „Da sich Artefakte aus Cross-Site Scripting (XSS), Phishing und JavaScript-Injektionen teilweise ähneln, bietet sich XSS als exemplarischer Anwendungsfall an. Die durch XSS erzeugbaren Spuren sind kontrollierbar reproduzierbar und lassen sich in einer forensischen Timeline-Visualisierung abbilden. Die gewonnenen Erkenntnisse können potenziell auch zur Analyse anderer Injektions- oder Social-Engineering-Angriffe beitragen.“


Quelle
https://www.researchgate.net/publication/371724261_An_Analysis_of_XSS_Vulnerabilities_and_Prevention_of_XSS_Attacks_in_Web_Applications
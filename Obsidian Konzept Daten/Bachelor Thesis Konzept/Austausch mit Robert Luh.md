# To Do

- Figma Grafiken -> Legenden überarbeiten.
- Verwendete Tools Zitieren (Excalidraw, Figma, Adobe Farbcyrcle)
- Farb Akzente mit Adobe Fyrbcyrcle definieren um Auffällige Farben für wichtige und unwichtige ereginisse festzulegen.



# FRAGEN ROBERT LUH

- Torsten meint Grafischer Ansatz allein reicht ned für gute Arbeit. Mathematik in Zusammenhang
- Ist mein Ansatz gut oder ausgelutscht? Was sagt Robert Luh? Ehrlich?

- Verbindung zu Clustern? (Parallele zu Aktienbörse) Bekannte methode transferieren?
	- Cluster Ausreißer Analysieren? -> Konzept Anomalien zu entdecken. Quasi nur die Cluster Ausreißer deutlich auf der Zeitachse darstellen.
	-  ?Zweite Abhängigkeit wie Region hinzufügen?


- Wenn mein Konzept nachwievor so passt - Wichtigkeit des Prototypen?



-  ==**Was kann meine Arbeit aufwerten? Was fehlt, damit es wissenschaftlich wertvoll wird?==


- ==**Wie soll ich testdaten generieren? 1 monat reicht ja nicht für den Prototypen? Aber immer noch besser im Lab??==
	- ==Soll ich auf bestimmte Funktionen -> Artefakt Arten einschränken? Oder sollte es alles können? ->== 
	- ==Oder Konzept alles Können aber Prototyp nicht==



User Test machen mit Figma Grafiken -> 2-3 Use Cases, Zeit Stoppen etc
Später die gleichen Leute den Prototypen testen lassen.

-> Proof of Concept

Personen, Umfeld ist wichtig zu wissen -> Standard Werte qualitative Forschung.

2-3 Use Cases -> Prototyp

Alles Usecases auf paper verweisen woher sie kommen. grob Background. Warum nehme ich die bestimmten nur etc.

zb 23 use cases -> 5 Cluster von jedem cluster 1 use case.
 

Vergleichen mit alternativen lösung? best case.


Mensch kommt zum schluss nicht automatisch anomalie erkennen.


Visualisierung im Fokus oder intelligenz? -> Visualisierung

Prototyp
3 Artefakt kategoerien darstellen.


Konzept in den Approach -> und prototypen + user test in implementation.


Alle Hypothesen müssen beantwortet und mit Experiment belegt sein!


related work unterschied highliten.




2 Fragen -> Alles im beriecht Betriebssysteme + 1. Zur Arbeit




✔ **Eine klare Problemstellung formulieren** – Welches forensische Problem löst deine Visualisierung besser als bestehende Methoden?
-> Schnellere Zielfahndung als mit Tabellenanalyse.

✔ **Die Vorteile des Konzepts evaluieren** – Warum ist dein Ansatz besser oder innovativer?
-> Weil wir Menschen lieber Visuell arbeiten.



# Nach was suche ich?


## Neuer/Zusätzlicher Ansatz Cluster
> Grundsätzlich werden Domänen in ihrer darstellung Nachwievor anhand von Häufigkeit Abstrahiert. Die Schärfe wird durch das zoomen eingestellt. Jedoch sind alle anderen Domänen die keine Cluster Ausreißer darstellen TRANSPARENTER!

Analyse der Domänen durch clustering:
- Nach Häufigkeit
- Nach region
- Tag,nacht (Uhrzeit)
- Domänenlänge
- Aufenthaltsdauer

**Alle Cluster "Ausreißer" werden ROT auf dem Zeitstrahl markiert!**



# ChatGPT zu Was sucht Forensiker

 >  **In meinem Szenarien wird VOR ALLEM nach mehreren ZUSAMMENHÄNGENDEN der folgenden Szenarien gesucht.**
 >  
 >  Jeder Auffälliger Bereich der einen der Folgenden Szenarien aufweist wird auf dem Zeitstrahl markanter dargestellt. Bereiche, die mehrere Zusammenhängende Szenarien beinhalten werden nochmal hervorgehoben.
 
Unsere Analyse beschränkt sich auf einen Browser, weswegen wahrscheinlich alles was mehrere Browser beansprucht zu Futur Work kommt.


ZIVA Kategorisiert diese 6 Anomalie Arten und zeigt grafisch dann praktisch die gleiche Farbe/Form für alle verdächtigen Ereignisse der gleichen Art auf dem Zeitstrahl an.

Im Hintergrund wird dezent der gesamte Domänenverlauf dargestellt. Er ist jederzeit visuell griffbereit. 

Anstelle Artefakt Kategorien -> Domänen oder Zeitverbundene (in Zusammenhang gebrachte) Anomale Ereignisse und unter der Trennlinie allgemeine (Zeitlose) Anomale Beweisartefakte.



Prompt: wie genau sehen Beweismaterialien aus? Nach welchen Kategorien von Anomalien oder Szenarien wird genau gesucht?

##### ChatGPT:
### **Wie sehen digitale Beweismaterialien in der Browser-Forensik aus?**

Digitale Beweise bestehen aus **Daten-Artefakten**, die auf **Nutzeraktivitäten, Sicherheitsverstöße oder Manipulationen** hinweisen. Sie können als **Textdateien, Datenbanken, Logs oder binäre Dateien** vorliegen und sind oft zeitlich gestempelt.

---

## **Kategorien von Anomalien & Verdächtigen Szenarien**

### **1. Ungewöhnliches Nutzerverhalten**

🔎 _Ziel: Identifikation verdächtiger Aktivitäten oder Nutzerinteraktionen_

- **Ungewöhnliche Surfzeiten** (_z. B. Aktivität zu ungewöhnlichen Uhrzeiten_) --> Suche mit Clusterdiagramm
- **Häufiges Löschen von Verlauf & Cookies** (_Anti-Forensik-Versuche_) --> Cluster Diagramm? Oder alles nehmen?
- **Mehrere Identitäten/Accounts auf einem Gerät**
- **Plötzliche Änderung des Browser-Verhaltens** (_z. B. Wechsel auf Tor/VPN_)

---

### **2. Spuren von Cyberkriminalität & Angriffen**

🔎 _Ziel: Aufdecken von Phishing, Malware-Verbreitung oder Hacking-Aktivitäten_

- **Zugriff auf kompromittierte Webseiten** (_z. B. Darknet, Scam-Seiten, Exploit-Kits_)
- **Verdächtige Downloads** (_Malware, ausführbare Dateien, verschlüsselte ZIPs_)
- **Manipulierte Cookies/Sessions** (_z. B. Session Hijacking, gestohlene Tokens_)
- **Cross-Site-Scripting (XSS) & Injection-Spuren** (_Eingabe von bösartigem Code in Felder_)
- **Häufige Weiterleitungen & Referrer-Spoofing** (_Hinweise auf Phishing-Versuche_)

---

### **3. Datenmanipulation & Anti-Forensik-Techniken**

🔎 _Ziel: Erkennen von Spurenverschleierung oder verschleierten Angriffen_

- **Manuell veränderte oder manipulierte Zeitstempel**
- **Verwendung von "Secure Delete"-Techniken oder Wiping-Tools**
- **Browser-Forensik-Blocker & Plugins (z. B. Privacy Badger, NoScript, Canvas Defender)**
- **Anormale Verlaufsdaten (z. B. Reihenfolge der URLs passt nicht zu typischem Surfverhalten)**
- **Unlogische Artefakt-Korrelation (z. B. besuchte Seite ohne dazugehörige Cookies oder Cache-Daten)**

---

### **4. Insider-Bedrohungen & Datendiebstahl**

🔎 _Ziel: Nachweis von unerlaubtem Zugriff oder Datenweitergabe_

- **Login-Versuche von ungewöhnlichen Standorten/IPs**
- **Download oder Upload großer Datenmengen auf Cloud-Dienste (Dropbox, Google Drive)**
- **Kopieren von sensiblen Daten auf externe Speichergeräte**
- **Plötzlicher Zugriff auf interne Systeme oder geschützte Ressourcen**
- **Nutzung anonymer Browser oder verschlüsselter Kommunikationsdienste**

---

### **5. Spuren von Online-Kriminalität & Betrug**

🔎 _Ziel: Identifikation betrügerischer Aktivitäten, Identitätsdiebstahl oder betrügerischer Kommunikation_

- **Mehrere Kreditkarten-/Zahlungstransaktionen innerhalb kurzer Zeit**
- **Fake-Accounts oder Nutzung gestohlener Identitäten**
- **Verdächtige Social-Media-Aktivitäten (z. B. Fake-Profile, Scam-Nachrichten)**
- **Crypto-Wallet-Interaktionen (z. B. Darknet-Zahlungen, Geldwäsche)**

---

### **6. Verdächtige Software & Erweiterungen**

🔎 _Ziel: Erkennen von Malware, Keyloggern oder betrügerischen Add-ons_

- **Unbekannte oder neue Browser-Plugins**
- **Automatisierte oder ungewöhnlich schnelle Mausbewegungen & Klicks** (_Bot-Aktivitäten_)
- **Verdächtige Proxy-/VPN-Einstellungen (z. B. plötzlicher Wechsel auf ausländische IPs)**
- **Persistente Skripte oder unerklärlicher Netzwerk-Traffic im Hintergrund**

---

## Alternativ Intern/Extern


Ja, diese Beweisspuren lassen sich sinnvoll in **interne (Nutzerverhalten) und externe (System- & Netzwerkspuren)** Spuren unterteilen.

### **📂 Interne Spuren (Nutzeraktivitäten & Manipulationen)**

➡️ _Ziel: Direkte Aktionen des Nutzers identifizieren_

- **Surfverhalten** (Besuchte Seiten, ungewöhnliche Zeiten)
- **Löschen von Artefakten** (Verlauf, Cookies, Cache)
- **VPN/Tor-Nutzung** (Plötzliche Verhaltensänderung)
- **Verdächtige Logins & Accounts** (Mehrere Identitäten, Standortwechsel)
- **Manipulierte Daten** (Veränderte Zeitstempel, gelöschte Spuren)
- **Datenweitergabe** (Cloud-Uploads, USB-Kopien)

### **🌐 Externe Spuren (Netzwerkaktivitäten & Systemmanipulationen)**

➡️ _Ziel: System- & netzwerkbezogene Anomalien erkennen_

- **Kompromittierte Webseiten** (Darknet, Scam, Exploits)
- **Phishing & Malware** (Verdächtige Downloads, schadhafte Links)
- **Session-Manipulation** (Gestohlene Cookies, Token-Hijacking)
- **Referrer-Spoofing & Weiterleitungen** (Phishing, Fake-Traffic)
- **Netzwerkspuren von VPNs & Proxys** (IP-Wechsel, ungewöhnlicher Traffic)
- **Bösartige Erweiterungen & Scripts** (Keylogger, persistente Hintertüren)











----




Im Folgenden sind die Spuren aufgelistet nach welchen ZIVA in den Beweisartefakten sucht.


\begin{itemize}
    \item[\textbf{1.}] \textbf{Ungewöhnliches Nutzerverhalten (Interner Einfluss)}. 
    Ziel ist die Identifikation verdächtiger Aktivitäten oder Nutzerinteraktionen. (Unerlaubter Zugriff oder Datenweitergabe, verdächtiges Navigier-Verhalten, Spurenverschleierung) ZIVA stellt Ereignisse dieser Art in orangener Farbe auf der Zeitachse dar:

        \begin{itemize}
            \item \textbf{Ungewöhnliche Surfzeiten} (z. B. Aktivität zu ungewöhnlichen Uhrzeiten) %--> Suche mit Clusterdiagramm
            \item \textbf{Häufiges Löschen von Verlauf \& Cookies} (Anti-Forensik-Versuche) %--> Cluster Diagramm? Oder alles nehmen?
            \item \textbf{Mehrere Identitäten/Accounts auf einem Gerät}
            \item \textbf{Plötzliche Änderung des Browser-Verhaltens} (z. B. Wechsel auf VPN, plötzlicher Wechsel auf ausländische IPs)
            \item \textbf{Nutzung verschlüsselter Kommunikationsdienste}
            \item \textbf{Manuell veränderte oder manipulierte Zeitstempel}
            \item \textbf{Verwendung von "Secure Delete"-Techniken oder Wiping-Tools} 
            \item \textbf{Browser-Forensik-Blocker \& Plugins} (z. B. Privacy Badger, NoScript, Canvas Defender)
            \item \textbf{Anormale Verlaufsdaten} (z. B. Reihenfolge der URLs passt nicht zu typischem Surfverhalten)
            \item \textbf{Unlogische Artefakt-Korrelation} (z. B. besuchte Seite ohne dazugehörige Cookies oder Cache-Daten)
            \item \textbf{Mehrere Kreditkarten-/Zahlungstransaktionen innerhalb kurzer Zeit}
            \item \textbf{Crypto-Wallet-Interaktionen} (z. B. Darknet-Zahlungen, Geldwäsche)
            \item \textbf{Login-Versuche von ungewöhnlichen Standorten/IPs} 
            \item \textbf{Download oder Upload großer Datenmengen auf Cloud-Dienste} (Dropbox, Google Drive)
            \item \textbf{Kopieren von sensiblen Daten auf externe Speichergeräte} % wie erkennt man sensiblöe daten?
            \item \textbf{Plötzlicher Zugriff auf interne Systeme oder geschützte Ressourcen}
        \end{itemize}

    \item[\textbf{2.}] \textbf{Spuren von Cyberkriminalität \& Angriffen (Externer Einfluss)}. 
    Ziel ist das Aufdecken von Phishing, Malware-Verbreitung oder Hacking-Aktivitäten. ZIVA stellt Ereignisse dieser Art in roter Farbe auf der Zeitachse dar:

        \begin{itemize}
            \item \textbf{Zugriff auf kompromittierte Webseiten} (z. B. Darknet, Scam-Seiten, Exploit-Kits)
            \item \textbf{Verdächtige Downloads} (Malware, ausführbare Dateien, verschlüsselte ZIPs)
            \item \textbf{Manipulierte Cookies/Sessions} (z. B. Session Hijacking, gestohlene Tokens)
            \item \textbf{Cross-Site-Scripting (XSS) \& Injection-Spuren} (Eingabe von bösartigem Code in Felder)
            \item \textbf{Häufige Weiterleitungen \& Referrer-Spoofing} (Hinweise auf Phishing-Versuche)
            \item \textbf{Unbekannte oder neue Browser-Plugins} 
            \item \textbf{Persistente Skripte oder unerklärlicher Netzwerk-Traffic im Hintergrund}
        \end{itemize}


% SOLL IN FUTUR WORK
% Soll 3 überhaupt inkludiert werden? -> sprengt evtl den Ramen der Arbeit. Aber erwähnen?
%    \item[\textbf{3.}] \textbf{Spuren von Online-Kriminalität \& Betrug}. 
%    Ziel ist die Identifikation von Identitätsdiebstahl oder betrügerischer Kommunikation:
%
%        \begin{itemize}
% %           \item \textbf{Fake-Accounts oder Nutzung gestohlener Identitäten}
%            \item \textbf{Verdächtige Social-Media-Aktivitäten} (z. B. Fake-Profile, Scam-Nachrichten)
%        \end{itemize}




\end{itemize}







----




# ChatGPT 
> Wäre Wichtig zu erwähnen warum man diese Software nimmt oder nicht nimmt

Derzeit existieren verschiedene Tools zur Erstellung interaktiver Zeitachsen-Visualisierungen mit Funktionen wie Zoom und Anpassung des Abstraktionsgrads. Allerdings sind diese nicht speziell auf die forensische Analyse von Browser-Aktivitäten mit Domänenfokus zugeschnitten.

**Beispiele für allgemeine interaktive Zeitachsen-Tools:**

- **TimelineJS**: Ein Tool zur Erstellung interaktiver Zeitachsen, das von Medien wie CNN und Time genutzt wird.
    
    [brandwatch.com](https://www.brandwatch.com/de/blog/hilfreiche-tools-fuer-die-datenvisualisierung/?utm_source=chatgpt.com)
    
- **Genially**: Bietet Vorlagen für interaktive Zeitstrahl-Diagramme, die für verschiedene Zwecke angepasst werden können.
    
    [genially.com](https://genially.com/de/vorlage/interaktive-zeitstrahl-diagramm-ii/?utm_source=chatgpt.com)
    
- **Piktochart AI**: Ein KI-gestützter Zeitstrahl-Generator, der Daten in ansprechende Timelines verwandelt.
    
    [piktochart.com](https://piktochart.com/de/ai-timeline/?utm_source=chatgpt.com)
    

Obwohl diese Tools leistungsfähig sind, fehlt ihnen die Spezialisierung auf die forensische Browser-Analyse mit Schwerpunkt auf Domänen. Ein solches spezialisiertes Tool könnte die Analyse von Browser-Zeitachsen durch interaktive Visualisierung von Domänenzugriffen erheblich verbessern.





























#### Related Approaches

Thema: 
- Anomalie erkennung mithilfe von Clustern Visualisierung
- Fokus auf Ausreißer gestreckt auf die Zeitachse.


Visualisierung allgemein: https://www.tableau.com/de-de/learn/articles/data-visualization





related?
Artifakts with minimal interaction
https://www.semanticscholar.org/paper/Analyzing-Tor-Browser-Artifacts-for-Enhanced-Web-in-Javed-Sajjad/a2705d84eff9f11c8c2859bae6f2e34b938d50bf
Data Extraction
https://www.semanticscholar.org/paper/Persistent-Browser-Storage-Data-Extractor-Aakanksha-Kumar/ef7ae0723efb242f229430f60881e6589b705a16






Spritzgusses - Fertigungsverfahren zur Herstellung von Kunststoffteilen -  Qualitätssicherung
chrome-extension://efaidnbmnnnibpcajpcglclefindmkaj/https://www-ai.cs.tu-dortmund.de/PublicPublicationFiles/honysz_2017a.pdf

Verschiedene Cluster Methoden
chrome-extension://efaidnbmnnnibpcajpcglclefindmkaj/https://repositum.tuwien.at/retrieve/21651


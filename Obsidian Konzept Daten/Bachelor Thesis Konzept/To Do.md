
---
# TO DO MARCH 2025

1. ~~Forschungsfragen und Hypothesen Überarbeiten!~~
2. ~~Approach Konzept Vorstellung fertigstellen -> in Arbeit übernehmen und Beschreiben.~~
	1. ~~Verlaufzeitachse Beschreiben~~
	2. ~~Analyse Zeitachse Beschreiben~~
	3. ~~**Methodologie optimieren/revision**~~
3. ~~Methodologie erklären. -> Prototyp Erstellung (Konzept -> Prototyp/Use Cases Test etc, Beschränkung etc). Analyse mit User Tests. Hintergrund der Personen?~~
4. ~~**Ähnliche Visualisierungstechniken Anschauen/raussuchen -> related Work?**~~
5.  ~~Arbeit über Browser Forensische Use Cases raussuchen~~
	1. ~~Entsprechende Arbeit finden~~
	2. ~~Clustern (Use Cases Kategorisieren nach Komplexität und Art (Umfang, Abdeckung der Use Case Arten)) und 2-3 von jedem Cluster für meine Arbeit aussuchen und begründen warum~~
		1. ~~Entscheidung Fällen in die Arbeit inkludieren und begründe~~
	3. ~~Use Cases mit Excalidraw darstellen und planen~~

# TO DO UNTIL END of April 2025

1. ~~Intern/Externe Einflüsse weglassen -> Figma draft überarbeiten!!!~~
2. ~~Fetslegen welche Artefakte im Zusammenhang mit den Use Cases dargestellt werden~~
	1. ~~Artefakte definieren~~

3. ~~Statistiken, etc raussuchen die relevanz von 2 usecases begründen~~

4. ~~Use Cases Umsetzen und mit foxton BHC Extrahieren  ---> **Proof of Concept PT.1**~~
	1. ~~Umstig auf Faxton Browser History Capturer: https://www.foxtonforensics.com/browser-history-capturer/~~
	2. ~~Use Cases umsetzen~~
		1. ~~Spuren verwischen~~
		2. ~~XSS attacke in skript in cooky?~~
	3. ~~Mit Figma einen exakten Draft für beide Use Cases Nachbauen~~
		1. ~~Daten zusammensuchen in det Extraierten zeug~~
			1. ~~Was ist anomal daran, was erkennt ZIVA? (Cookie, Rest-spuren Sessions etc... für Verlauf)~~
		2. ~~Timestampd ggf convertieren~~
		3. ~~Draft nachbauen. möglichst exact!~~
	4. ~~An 2 Personen Austesten, ob es selbsterklärend genug ist.~~
		1. ~~User Test Fragen vorbereiten + Test Vorbereiten~~
		2. ~~Test an 2 Personen durchführen und dokumentieren~~
		3. ~~Ergäbnisse der Protokolle beschreiben~~
		4. ~~Figma Drafts einbinden~~




5. ~~Konzept Figma Draft überarbeiten~~ 
	2. ~~Figma drafts des allgemeinen Konzeptes überarbeiten und updaten!~~
	3. Probeweise nochmal einer Person vorlegen! -> CELINA
	4. ~~Approach Konzept ergänzen -> ZEITSTEMPEL UNBEKANNT Sektion!!~~
6. ~~Approach neuer Strucktur Finales Konzept -> Validierung -> implementation -> Validierung überarbeiten.~~




7. ~~"Experiment Setup" unterkapitel ausführlicher schreiben (Use-Cases -> Allgemein besser dokumentieren, was gemacht wurde)~~
	1.  ~~Use Cases in BA erläutern, erklären dokumentieren was gemacht wurde, wie und warum.~~
	2. ~~Artefakte Begründen!! -> Definition anpassen bzw genauer erklären Welche DBs durchsucht werden etc. -> Sektion überarbeiten!!! (Tabelle aus [[06-User-Test-1]])~~
	3. ~~Foxton Browser History Viewer statt RS Browser im finalen User Test als gegentest nehmen~~


8. ~~Related Work überarbeiten [[Related Work]] !! Neue Arbeiten inkludieren -> Unterschied besser begründen abgrenzen!~~


9. ~~Einführung revision/ergänzung -> Arbeit hilft die Anomalien in den Zeitlichen Kontext zu bringen -> Visualisierung -> Schneller, einfacher, verständlicher -> **Vorausgesetzt funktionierende Anomalie Erkennung.**  -> „Voraussetzung für den Nutzen dieser Darstellung ist eine zumindest grundlegende Anomalie-Erkennung, die auffällige Ereignisse identifizieren und zur Markierung vorschlagen kann.“~~



# To Do TILL END OF 2025


8. Implementation Beginnen ---> **Proof of Concept PT.2**
	3. Prototypen Anhand des Optimierten Konzepts durch die User Tests nachbauen. Darstellungsfokussiert. 4 Artefakt Arten Limitiert und 2 Use Cases Erkennung.
		1. C# Projekt Umgebung anlegen mit GitHub 
		2. Visualisierungsmethode Skizzieren. Methode der Implementierung skizzieren. verstehen!
			1. Planen und zeichnen und durchdenken
		3. Strucktur für das Programm anlegen
			1. ~~Blazer verstehen und werwenden.~~
			2. Diagramm mit Excalidraw erstellen
			3. Programmieren Beginnen
				1. Darstellung der Daten
					1. ~~Zeitstrahlen anzeigen~~
						1. **Zeitstrahl FIX -> rendert immer die senkrechten Linien zu beginn, anstelle dass sie mit scrollen. Verwirrende Ansicht!** unbedingt fixen damit man nachvollziehen kann, dass die Zeitachse gescrollt wird. -> EVTL liegt es an der startposition?
						2. eigene Leitlinien für die weiteren gewünschten Artefakt Arten bauen (Cookies etc)
						3. ~~Zeitstrahl scrollen können - fixen -> liegt das an der fläche der ebene?~~
							1. ~~Zeitstrahl mit Maus ziehen -> verschieben können~~
							2. ~~Zoomen soll den dargestellten Zeitintervall festlegen~~
							3. Zeitachsen Artefakt Positionsdarstellung FIXEN!!!
							4. startpunkt der Zeitline 1-2h vor ersten datensatz aus export
								1. ~~Dynamisches darstellen der Daten in echtzeit anhand der Daten~~
								2. ~~endlos scrollen ermöglichen~~
							5. ~~Listen aus datenbanken werden nach Zeitstempel sortiert.~~
					2. ~~Zeitlose Artefakte Bereich auf Zeitstrahl darstellen~~
				2. Import von Daten
					1. History Datenbank Imortieren
					2. Einfügen weiterer Artefakte
						1. Cookie Database
						2. WebData Database
						3. Favicons?
					3. Soll eine eigene Database angelegt werden, die alle artefakte durchparsed und für ZIVA anpasst? -> eigenes Format übertragen um alles gemeinsam zu haben
						1. oder alternatriv einfach nur das darstellen was bei import importiert wird. -> unschön aber schneller.
					4. Objekte oder Symbole für Artefakt Darstellung und farben selektion festlegen
						1. Farben per URL Zufällig festlegen?
				3. Darstellung der Artefakte auf dem Zeitstrahl
					1. Darstellung aller besuchten Domänen der Reihe nach
							1. FEHLER FIX. Nur darstellung der 1. Zwei elemente der Datenbank ->
								1. Genaue exakte darstellung!
							2. Farben und größe Anpassen.
					2. Zusammenfassung von häufig besuchten Domänen
						1.  Implementierung der Einfachen Domöne Zusammenfassung -> Zusammengehörige domänen per website referenz oder reihenfolge.
						2. Implementierung der langzeit zusammenfassungen
					3. Zusammenhänge/Referenzen markieren.
				4. Analyse Implementieren
					1. Patterns für Anomalien der Use Cases implementieren.




	4. User Test machen -> Vergleich mit RS Browser/other ähnliches Programm Analyse und ZIVA -> Zeit stoppen.
		1. Erneut die 2 Use Cases Verwenden!
			1. Bei bedarf anpassen und dokumentation der Use Cases überarbeiten
		2. Test durchführen!




10. GPT Prompt dokumentieren!!! Für appendix (Modell o4 mini)

# To Do Till END OF MAY

9. Resultate Dokumentieren
10. Introduction wissenschaftlicher dokumentieren!!
11. Discussion
12. Futur Work (Conclusion)
	5. Analyse Zeitstrahl -> Arten der Anomalien darstellen?


13. Overal Revision!!! K.O. Kriterien -> Introduction? Related Work? Background?
	1. Plagiat Test: [https://plagiat.oeh.fhstp.ac.at/login.php](https://plagiat.oeh.fhstp.ac.at/login.php "https://plagiat.oeh.fhstp.ac.at/login.php")
	2. Abkürzungen? etc

# TILL END OF JUNE

14. Präsentation
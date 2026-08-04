# ZIVA-Prototyp

> **Hinweis für Gutachter**
>
> Dieser Prototyp wurde über einen längeren Zeitraum iterativ mit Unterstützung von KI (ChatGPT GPT-4o, GPT-5.3 und GPT-5.5) entwickelt. Die KI wurde als Entwicklungswerkzeug für Implementierung, Fehlersuche und die Ausarbeitung von Algorithmen eingesetzt. Die fachliche Konzeption, Architektur, Implementierung und Validierung des Prototyps erfolgten durch den Autor. :contentReference[oaicite:0]{index=0}

Prototyp eines Visualisierungstools für Browserforensik, entwickelt im Rahmen einer Bachelorarbeit.

## Überblick

ZIVA ist ein Prototyp zur Unterstützung browserforensischer Untersuchungen. Das Werkzeug korreliert Browserartefakte automatisch, rekonstruiert Navigationsereignisse und stellt diese in einer interaktiven Zeitleiste dar. Ziel ist es, den manuellen Analyseaufwand zu reduzieren, indem verschiedene Browserartefakte in einer gemeinsamen Oberfläche zusammengeführt und Ermittler durch eine regelbasierte Anomalieerkennung unterstützt werden.

---

# Hauptfunktionen

- Import von Chromium-Browserprofilen
- Interaktive Zeitleistenvisualisierung
- Automatische Korrelation von Browserartefakten
- Rekonstruktion von Navigationspfaden
- Regelbasierte Anomalieerkennung
- Domänenfilter
- Artefaktfilter
- Zoomen und Navigieren innerhalb der Zeitleiste
- Detaillierte Artefaktinspektion
- Tooltips und kontextbezogene Informationen

---

# Benutzerhandbuch

## Import eines Browserprofils

1. Anwendung starten.
2. Eine beliebige Datei innerhalb des Chromium-Browserprofilordners auswählen.
3. ZIVA erkennt das Browserprofil automatisch und importiert alle unterstützten Browserdatenbanken.
4. Analyse starten.

---

## Navigation in der Zeitleiste

### Zoomen

**STRG + Mausrad**

Vergrößert oder verkleinert die Zeitleiste.

> **Hinweis:**  
> Der Zoom fokussiert derzeit den aktuellen sichtbaren Bereich und nicht den Mauszeiger oder das ausgewählte Artefakt. Für eine optimale Nutzung sollte vor dem Zoomen zunächst das gewünschte Artefakt ausgewählt werden. Dieses Verhalten wird in einer zukünftigen Version verbessert.

---

### Zwischen Artefakten navigieren

**← / → Pfeiltasten**

Wechselt zum vorherigen bzw. nächsten sichtbaren Artefakt innerhalb der Zeitleiste.

---

### Filter

Das Filtermenü befindet sich in der **oberen rechten Ecke**.

Derzeit verfügbare Filter:

- Domänen
- Artefakttypen
- Analyseergebnisse
- Navigationspfad

*Eine ausführliche Beschreibung der einzelnen Filter wird in einer zukünftigen Version ergänzt.*

---

## Artefaktdetails

Durch Anklicken eines Artefakts können

- Metadaten eingesehen,
- Zeitstempel angezeigt,
- Beziehungen zwischen Artefakten untersucht sowie
- weitere forensische Informationen dargestellt werden.

---

# Geplante Weiterentwicklung

Der Prototyp befindet sich weiterhin in der Entwicklung.

Geplante Erweiterungen umfassen:

- Verbesserte Rendering-Performance
- Vollständige Überarbeitung der Benutzeroberfläche
- Unterstützung zusätzlicher Browserartefakte
- Mehrsprachige Benutzeroberfläche (Deutsch / Englisch)
- Robusterer Profilimport und Parser
- Persistente Fälle mit Speicher- und Ladefunktion
- Manuelles Erstellen und Bearbeiten von Artefaktbeziehungen
- Ermittlernotizen und Annotationen
- Verbesserter Zoom der Zeitleiste
- Erweiterte Suchfunktion
- Unterstützung weiterer Chromium-basierter Browser
- Export von Untersuchungsberichten
- Plugin-Architektur für zusätzliche Artefaktparser
- Verbesserte Anomalieerkennung durch konfigurierbare Regelsätze
- Höhere Skalierbarkeit für große Browserprofile
- Entschlüsselung von Cookies

---

# Aktuelle Einschränkungen

- Der Zoom zentriert sich derzeit nicht auf das ausgewählte Artefakt.
- Es werden ausschließlich Chromium-basierte Browserprofile unterstützt.
- Einige Parser befinden sich noch im experimentellen Entwicklungsstadium.
- Die Importgeschwindigkeit kann bei großen Browserprofilen abnehmen.

---

# Lizenz

Copyright (c) 2026 Viktor Olenberg.

Alle Rechte vorbehalten.

Dieses Repository wird ausschließlich zu Dokumentations- und wissenschaftlichen Zwecken veröffentlicht. Der Quellcode darf ohne ausdrückliche Genehmigung des Autors weder kopiert, weitergegeben, verändert noch anderweitig verwendet werden.

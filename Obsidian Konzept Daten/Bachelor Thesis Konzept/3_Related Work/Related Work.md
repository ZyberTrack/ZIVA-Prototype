
# 04.2025 UPDATE


Hier sind die Quellen mit den entsprechenden Links zu den genannten Arbeiten:

1. **"Graph-based Temporal Analysis in Digital Forensics"**  
    [Link zur Arbeit](https://scholar.afit.edu/etd/2241/?utm_source=chatgpt.com)
    Diese Masterarbeit von Nikolai A. Adderley präsentiert TAIMA, ein Prototyp-Tool, das eine graphbasierte Zeitachse zur Ereigniskonstruktion in digitalen Forensik-Untersuchungen bietet. Die Arbeit zeigt, wie abstrakte und visuelle Techniken genutzt werden, um Systemereignisse zu rekonstruieren und zu analysieren.
    
    - **Fokus:** Allgemeine Ereignisrekonstruktion in Systemforensik mit Graphen und Zeitachsen
    
	- **Browser-spezifisch:** ❌
    
	- **Anomalie-Erkennung:** Teilweise (Graphen helfen bei Erkennung)
    
	- **Ähnlichkeit:** **Mittel**
    
	- **Fazit:** Konzeptuell verwandt, aber kein Fokus auf Browser-Artefakte – kann eher als _theoretische Grundlage_ dienen.
    
    
2. **"Computer forensic timeline visualization tool"**  
    [Link zur Arbeit](https://dl.acm.org/doi/10.1016/j.diin.2009.06.008?utm_source=chatgpt.com)
    Diese Studie beschreibt CyberForensic TimeLab, ein Prototyp-Tool, das Beweismittel anhand ihrer Zeitvariablen indiziert und auf einer Zeitachse darstellt. Es wird gezeigt, dass dieses Tool den Ermittlern hilft, kohärente Beweise schneller und intuitiver zu finden.
    
    - **Fokus:** Entwicklung eines Tools zur visuellen Zeitachsen-Darstellung digitaler Artefakte
    
	- **Browser-spezifisch:** ❌
    
	- **Anomalie-Erkennung:** ❌
    
	- **Ähnlichkeit:** **Niedrig bis mittel**
    
	- **Fazit:** Klassiker, zeigt wie Zeitachsen genutzt werden, aber nicht modern oder spezialisiert auf Browser.
    
    
3. **"A Study for Classification of Web Browser Log and Timeline Visualization"**  
    [Link zur Arbeit](https://link.springer.com/chapter/10.1007/978-3-642-35416-8_14?utm_source=chatgpt.com)
    Diese Arbeit schlägt eine Methode zur Klassifizierung von Webbrowser-Protokollen und deren Darstellung auf einer Zeitachse vor. Sie zielt darauf ab, die Analyse von Browser-Artefakten zu verbessern und die Effizienz der Untersuchung zu steigern.
    
    - **Fokus:** Klassifikation von Browser-Logs und Visualisierung auf einer Zeitachse
    
	- **Browser-spezifisch:** ✅
    
	- **Anomalie-Erkennung:** ❌
    
	- **Ähnlichkeit:** **Hoch bei Visualisierung**, aber kein intelligentes Anomalie-System
    
	- **Fazit:** Am ähnlichsten zu deinem Thema, **aber dein Ansatz mit Analyse-Zeitachse + Anomalien ist neu** und wäre eine sehr sinnvolle Weiterentwicklung.
    
4. **"Advancing Web Browser Forensics: Critical Evaluation of Emerging Tools and Techniques"**  
    [Link zur Arbeit](https://arxiv.org/abs/2410.12605?utm_source=chatgpt.com)
    Diese Arbeit bietet eine umfassende Analyse von Webbrowser-Artefakten und bewertet verschiedene forensische Tools und Techniken. Sie untersucht, wie unterschiedliche Browser-Modi und -Datenquellen in forensischen Untersuchungen berücksichtigt werden können.
    
    - **Fokus:** Überblick über Browser-Forensik-Tools, Evaluation
    
	- **Browser-spezifisch:** ✅
    
	- **Anomalie-Erkennung:** ❌
    
	- **Ähnlichkeit:** **Gering bis mittel**
    
	- **Fazit:** Nützlich als Related Work, aber keine eigene Visualisierungslösung.
    
5. **"Reconstructing and Visualizing Evidence of Artifact from Firefox SessionStorage"**  
    [Link zur Arbeit](https://link.springer.com/chapter/10.1007/978-3-319-15087-1_7?utm_source=chatgpt.com)
    Diese Studie befasst sich mit der Rekonstruktion und Visualisierung von Beweismitteln aus dem Firefox SessionStorage. Sie zeigt, wie Artefakte aus dem SessionStorage extrahiert und auf einer Zeitachse dargestellt werden können.
    
    - **Fokus:** Extraktion & Visualisierung von Firefox-SessionStorage
    
	- **Browser-spezifisch:** ✅
    
	- **Anomalie-Erkennung:** ❌
    
	- **Ähnlichkeit:** **Teilweise**
    
	- **Fazit:** Technisch eng verwandt, aber auf einzelne Browser-Komponente beschränkt – **dein Ansatz ist breiter und intelligenter.**
    

## Weitere Bedenken -> In BA Related Work zu ergänzen

FRAGE:
mein ansatz der anomalie erkennung ist eher eine visueller ansatz der anomalie markierung. mein prototype funktioniert da auch erst eher statisch. future work knann diesen ansatz noch erweitern. Ist das dann denoch ein problem?

###### ChatGPT:

Nein, das ist **absolut kein Problem** – im Gegenteil, das ist sogar **typisch und sinnvoll für eine Bachelorarbeit**.

### Warum dein Ansatz völlig legitim ist:

#### ✅ **1. Visual-first statt AI-first ist realistisch und wertvoll**

Dein Ansatz der **visuellen Anomalie-Markierung** (also z. B. durch farbliche Hervorhebung, zeitliche Gruppierung oder Icons) ist ein **sehr nützlicher erster Schritt**, bevor man komplexere automatische Anomalieerkennung implementiert. Visuelle Muster sind für Analyst*innen oft intuitiver.

#### ✅ **2. Prototyp = statisch ist okay**

Dass dein Prototype die Anomalien **statisch markiert** (z. B. durch manuell gesetzte oder einfach detektierte Events) ist völlig in Ordnung in einer Bachelorarbeit. Es zeigt, dass du:

- ein funktionierendes Grundkonzept entworfen hast,
    
- eine Visualisierung entwickelt hast,
    
- und **Platz für Future Work** gelassen hast.
    

#### ✅ **3. "Future Work" ist gewollt**

Die Beschreibung von Weiterentwicklungen ist sogar **erwünscht** in wissenschaftlichen Arbeiten. Du zeigst damit, dass du dein Thema **reflektiert** hast und seine **Potentiale** erkennst. Eine typische Formulierung wäre z. B.:

> „Der aktuell implementierte Prototyp nutzt einfache heuristische Regeln zur Markierung potenzieller Anomalien auf der Zeitachse. In zukünftiger Arbeit könnte dieser Ansatz durch maschinelles Lernen oder Regelwerke weiter verfeinert und automatisiert werden.“

#### ✅ **4. Abgrenzung ist sauber möglich**

Du kannst dich in deiner Arbeit ganz klar von existierenden Tools abgrenzen, etwa so:

> „Im Gegensatz zu bestehenden Tools wie Timesketch oder [XY], die primär auf eine lineare Darstellung von Artefakten ohne semantische Auswertung setzen, verfolgt dieser Ansatz eine kombinierte Darstellung von Artefakten und visuell gekennzeichneten Auffälligkeiten im Kontext der Browser-Forensik.“


### Fazit:

🔵 **Nein, es ist kein Problem.**  
Dein Ansatz ist:

- **originell genug** für eine Bachelorarbeit,
    
- **methodisch sinnvoll**, auch wenn statisch,
    
- und **realistisch umsetzbar** im gegebenen Zeitrahmen.







## Effektivität von Visualisierung in der Forensik:


## Abstraktion von Artefakten für die Zeitachsen Analyse


## Stand der Wirtschaft im Angebot von Software für Zeitachsen Analyse




**Vor allem erklären WARUM diese Quellen mich dazu inspiriert haben meine Arbeit zu schreiben.**



# Tutorial

Our exploration covers various aspects:

\begin{itemize}
    \item \textbf{Distinguishing Factors}:
        We elucidate how our work diverges from the existing body of published works.
        What sets our scenario apart?
        Are our goals or assumptions different?
        Does our practical testing or prototype implementation offer a unique perspective?
        Could advancements in computational power, compared to what was affordable a decade ago, enable the resolution of specific problems?
        
    \item \textbf{Placement in Your Document}:
        The placement of the Related Work chapter depends on the nature and complexity of your work.
        It can be positioned near the beginning, possibly integrated with the background section, providing readers with essential context to understand your specific scenario.
        Alternatively, it can be placed at the end, following your work and results but preceding the conclusion.
        In this latter arrangement, readers gain a comprehensive understanding of your work before delving into the discussion of differences with existing works.
        
    \item \textbf{Grouping and Categorization}:
        When your literature search yields numerous related works, consider grouping them into subcategories.
        It's important to remember that most of these references are likely to be entirely new to your readers, making categorization even more valuable.
\end{itemize}
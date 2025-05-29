
# Einführung für BA arbeit

Zuerst ergänzen dass rechts ein bereich für artefakte ohne Zeitstempel hinzugefügt wurde.

erklären, dass für den user test nur begrenzte artefakte verwendet wurden und welche, um die übersichtlichkeit zu wahren. Fokus auf verständlichkeit und selbsterklärung der oberfläche:

Folgende Databases und untergeordnete Tabels daraus wurden für dien User Test 1 verwendet (Tabell davonb machen und in BA implementieren bei USer Test 1. Begründen warum und weshalb etc)



- History
	- Visits
- Web Data
	- keywords
- Cookies



Dann User Test dokumentireen

10 Fragen stellen

Am Ende Verbesserungsvorschläge?






# Timestamps umwandeln


#### Cookies

SELECT
  *,
  datetime(creation_utc / 1000000 + strftime('%s', '1601-01-01'), 'unixepoch', 'localtime') AS creation_datetime_local,
  datetime(last_access_utc / 1000000 + strftime('%s', '1601-01-01'), 'unixepoch', 'localtime') AS last_access_datetime_local
FROM cookies
ORDER BY creation_utc DESC;


### Verlauf



# 1-Erkennung bösartiger cookies

#### **Verdächtiger Domainname**

- Cookies, die zu Domains gehören, die **nicht zur aufgerufenen Website gehören** (z. B. `ads.suspiciousdomain.ru` bei Besuch von `example.com`).
    
- Subdomains mit vielen zufälligen Zeichen (`a8sd9f8sdf.example.com`).
    

#### **Lange Lebensdauer (expires_utc weit in der Zukunft)**

- Tracking-Cookies wollen lange „überleben“, z. B. über mehrere Jahre (oft bis 2038 oder mehr).
    
- Normale Session-Cookies haben eher ein baldiges Ablaufdatum oder kein `expires_utc` (sie sind dann temporär).
    

#### **Hohe Aktivität oder viele identische Cookies**

- Viele Cookies von derselben Domain mit leicht unterschiedlichen Namen – kann auf Fingerprinting hindeuten.
    
- Cookies, die sich sehr oft neu setzen lassen oder sich bei jeder Aktivität ändern.
    

#### **Nicht gesichert (kein `is_secure`)**

- Wenn ein Cookie **nicht als „secure“ markiert ist**, kann es über unverschlüsselte HTTP-Verbindungen übertragen werden – ein Risiko, besonders für Login- oder Session-Cookies.
    

#### **Nicht `HttpOnly`**

- Cookies ohne `is_httponly`-Flag können von JavaScript ausgelesen werden → anfälliger für XSS-Angriffe.
    

#### **Verdächtige Inhalte im Namen oder Wert**

- Namen oder Inhalte wie `uid`, `track`, `sessid`, `token`, `adid`, `ga_`, `fbp`, `trk`, `pixel`, etc.
    
- Base64-ähnliche Zeichenfolgen oder UUIDs, die wie Tracker aussehen.
    

#### **Herkunft von Ad-Netzwerken oder Trackern**

- z. B. `doubleclick.net`, `google-analytics.com`, `facebook.net`, `taboola`, `outbrain`, `yadro.ru`, `bing.com`, usw.
    
- Diese sind technisch nicht „bösartig“, aber können invasiv sein.



# 2-Erkennung von Verlaufslücken

erstellte artefakte zu einer Zeit wo kein Verlauf vorhanden ist, (Cookies, session Ids, downloads...)

**History Database:**

SELECT
  v.id AS visit_id,
  datetime(v.visit_time / 1000000 + strftime('%s', '1601-01-01'), 'unixepoch', 'localtime') AS datetime_local,
  u.url AS current_url,
  u.title AS current_title,
  v.transition,
  v.from_visit,
  f.url AS referrer_url,
  f.title AS referrer_title
FROM visits v
LEFT JOIN urls u ON v.url = u.id
LEFT JOIN visits vf ON v.from_visit = vf.id
LEFT JOIN urls f ON vf.url = f.id
ORDER BY v.visit_time DESC;


#### **Top Sites (Top-Site-Vorschläge bei neuem Tab)**

- 📂 **Pfad:**  
    `~/.config/google-chrome/Default/Top Sites`
    
- 🗂️ **Art:** SQLite-Datenbank
    
- 📌 **Tabelle:** `top_sites`
    

**🧠 Enthält:**

- Häufig besuchte Seiten (auch nach Löschen des Verlaufs)
    
- `url`, `title`, `last_forced_topsite`
    

**💡 Hinweis:** Wird **nicht sofort geleert** – bleibt oft erhalten.

--> Befehl für database

SELECT * from top_sites;

---

#### **Favicon-Dateien / Icons von besuchten Seiten**

- 📂 **Pfad:**  
    `~/.config/google-chrome/Default/Favicons`
    
- 🗂️ **Art:** SQLite
    
- 📌 **Tabellen:** `favicon_bitmaps`, `favicons`, `icon_mapping`
    

**🧠 Enthält:**

- Kleine Bilder von Seiten, die besucht wurden
    
- Verlinkt mit `page_url`
    

**💡 Tipp:**  
Über `icon_mapping.page_url` → rekonstruierbar.

--> Befehl für Database sql

SELECT
  f.url AS favicon_url,
  f.icon_type,
  b.width,
  b.height,
  datetime(b.last_updated / 1000000 + strftime('%s', '1601-01-01'), 'unixepoch', 'localtime') AS last_updated_local
FROM favicons f
JOIN favicon_bitmaps b ON f.id = b.icon_id
ORDER BY b.last_updated DESC;

---

#### **Web Data (Suchbegriffe, Formulardaten, Autofill)**

- 📂 **Pfad:**  
    `~/.config/google-chrome/Default/Web Data`
    
- 🗂️ **Art:** SQLite
    
- 📌 **Tabellen:** `autofill`, `autofill_profiles`, `keywords`
    

**🧠 Enthält:**

- Eingegebene URLs als Suchvorschläge oder bei Formularfeldern
    
- Suchbegriffe mit Zeitstempeln (`last_used`)


SQL- QUEARRY:

->> keywords


SELECT
  id,
  short_name,
  keyword,
  favicon_url,
  url,
  originating_url,
  usage_count,
  datetime(date_created / 1000000 + strftime('%s', '1601-01-01'), 'unixepoch', 'localtime') AS created_local,
  datetime(last_modified / 1000000 + strftime('%s', '1601-01-01'), 'unixepoch', 'localtime') AS modified_local,
  datetime(last_visited / 1000000 + strftime('%s', '1601-01-01'), 'unixepoch', 'localtime') AS visited_local
FROM
  keywords
ORDER BY
  last_visited DESC;



->> autofill


->> autofill profiles





---

#### **Session Storage / Tabs von früheren Sitzungen**

- 📂 **Pfad:**  
    `~/.config/google-chrome/Default/Sessions/`  
    z. B. `Session_*`, `Tabs_*`
    
- 🗂️ **Art:** Binärformat (nicht SQLite)
    

**🧠 Enthält:**

- Geöffnete Tabs vor Schließen
    
- URLs, die zuletzt offen waren
    

**🔧 Tools:**

- `chrome_session_extractor.py`
    
- `Session Buddy` (Extension, falls noch aktiv)
    

---

#### **Local Storage & IndexedDB (WebApps)**

- 📂 **Pfad:**  
    `~/.config/google-chrome/Default/Local Storage/leveldb/`  
    `~/.config/google-chrome/Default/IndexedDB/`
    
- 🗂️ **Art:** LevelDB, JSON, Binär
    

**🧠 Enthält:**

- Website-Daten, Keys, Tokens → oft mit Domainname
    

---

#### **Cache-Dateien (HTML, JS, Bilder)**

- 📂 **Pfad:**  
    `~/.cache/google-chrome/Default/Cache/`
    
- 📂 **Oder:**  
    `~/.config/google-chrome/Default/Code Cache/`
    
- 🗂️ **Art:** Chrome-Cache (binär, aber analysierbar)
    

**🧠 Enthält:**

- Ressourcen von besuchten Webseiten
    
- HTML-Fragmente, Bilder mit URL-Hinweisen
    

**🔧 Tools:**

- `ChromeCacheView` (NirSoft)
    
- `Hindsight` von **obsidianforensics**
    

---

#### **Downloads / FileSystem API**

- 📂 **Pfad:**  
    `~/.config/google-chrome/Default/Downloads`
    
- 🗂️ **Art:** SQLite
    
- 📌 **Tabelle:** `downloads`
    

**🧠 Enthält:**

- Datei-URLs, von wo heruntergeladen wurde
    
- Zeitstempel + Zielpfad
    

---

## 📦 Bonus: Weitere versteckte Hinweise

|Artefakt|Ort|Inhalt|
|---|---|---|
|`Preferences`|JSON-Datei im `Default`-Ordner|Letzte Tabs, Suchmaschinen, Default-Seiten|
|`History Provider Cache`|Nicht gelöscht bei einfachem „Verlauf löschen“|Caches für Adressleiste|
|`Media History`|Chrome/Default/Media History|Besuchte Video-/Audioseiten|


# 3-Referal URL Erkennung

wenn angegeben. -> Pfeil durchziehen.
# ZIVA Prototype

Prototype of a browser forensic timeline visualization tool developed as part of a Bachelor's thesis.

## Overview

ZIVA is a prototype designed to support browser forensic investigations by automatically correlating browser artifacts and reconstructing navigation events within a visual timeline. The prototype aims to reduce manual analysis effort by combining multiple browser artifacts into a single interactive interface and supporting investigators through rule-based anomaly detection.

---

# Main Features

- Import Chromium browser profiles
- Interactive timeline visualization
- Automatic correlation of browser artifacts
- Navigation path reconstruction
- Rule-based anomaly detection
- Domain filtering
- Artifact filtering
- Timeline zooming and navigation
- Detailed artifact inspection
- Tooltips and contextual information

---

# User Guide

## Importing a Browser Profile

1. Open the application.
2. Select any file located inside the Chromium browser profile directory.
3. ZIVA automatically detects the profile and imports all supported browser databases.
4. Start the analysis.

---

## Timeline Navigation

### Zoom

**CTRL + Mouse Wheel**

Zoom in or out of the timeline.

> **Note:**  
> The zoom currently focuses on the current viewport instead of the mouse cursor or selected artifact. For the best experience, click the artifact you want to inspect before zooming. This behavior will be improved in a future version.

---

### Navigate Between Artifacts

**← / → Arrow Keys**

Moves the focus to the previous or next visible artifact within the timeline.

---

### Filters

The filter panel is located in the **upper-right corner**.

Current filters include:

- Domains
- Artifact types
- Analysis results
- Navigation path

*A detailed description of each filter will be added in a future revision.*

---

## Artifact Details

Click an artifact to

- inspect its metadata,
- view timestamps,
- inspect relations,
- display additional forensic information.

---

# Future Work

The prototype is still under development.

Planned improvements include:

- Improved rendering performance
- Complete UI and design revision
- Support for additional browser artifacts
- Multilingual interface (English / German)
- More robust profile import and parsing
- Persistent cases with save/load functionality
- Manual creation and editing of artifact relationships
- Investigator notes and annotations
- Improved timeline zoom behavior
- Advanced search functionality
- Support for additional Chromium-based browsers
- Export of investigation reports
- Plugin architecture for additional artifact parsers
- Improved anomaly detection using configurable rule sets
- Enhanced scalability for large browser profiles
- Cookie decryption

---

# Current Limitations

- Zoom focus is not yet centered on the selected artifact.
- Only Chromium-based browser profiles are supported.
- Some parser implementations are still experimental.
- Import performance may decrease for large browser profiles.

---

# License

Copyright (c) 2026 Viktor Olenberg.

All rights reserved.

This repository is published for documentation and academic reference only. The source code may not be copied, redistributed, modified, or used without the explicit permission of the author.

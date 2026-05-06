# Spec 016: OBS Integration

**Status:** 📋 Planned
**Created:** 2025-12-26
**Dependencies:** 007-display-profiles.md, 015-remote-control-api.md

## Overview

Implement features specifically designed to integrate SDAHymns with Open Broadcaster Software (OBS). This ensures that churches live-streaming their services can easily display high-quality lower-thirds and lyrics overlays without complex workarounds.

**Goal:** Make SDAHymns the easiest hymn software to use in a professional streaming workflow.

## Goals

1.  **Optimized Window Capture:** Ensure the display window can be captured cleanly by OBS, supporting both Chroma Key (Green Screen) and native transparency.
2.  **Browser Source Output:** Provide a dedicated HTML output via the embedded web server for the highest quality text rendering in OBS.
3.  **OBS Automation:** (Optional) Control OBS scenes/sources automatically via WebSocket when hymn display changes state.
4.  **Lower-Third Layouts:** Include specific Display Profiles designed for overlay use (bottom-aligned text).

## Architecture

### 1. Window Capture Optimization

We will leverage the **Display Profiles** system (Spec 007) to create "Stream-Ready" profiles.

**Features:**
*   **Chroma Key Support:** Allow setting a specific "Key Color" (e.g., `#00FF00` Green, `#FF00FF` Magenta) as the window background.
*   **Native Transparency:** Use Avalonia's `TransparencyLevelHint="Transparent"` and `SystemDecorations="None"` to create a window that has no background at all, just text.
    *   *Note:* This requires OS support (DWM on Windows) and might behave differently on Linux/macOS. Chroma Key is the fallback.

### 2. Browser Source (via Web Server)

Leveraging the Web Server from **Spec 015**, we expose a specialized endpoint.

**Endpoint:** `http://localhost:5000/obs/output` (or `/obs/lower-third`)

**Mechanism:**
*   This page connects to the SignalR hub.
*   It receives the exact same state updates as the Remote Control.
*   It renders the text using CSS.
*   **Background:** The body background is `transparent` by default.
*   **Layout:** CSS Grid/Flexbox used to position text at the bottom (Lower Third) or Center (Lyrics).
*   **Customization:** Accepts query parameters: `?style=lower-third&font=Arial&size=48`.

### 3. OBS WebSocket Client (Automation)

**Service:** `src/SDAHymns.Desktop/Services/Obs/ObsControlService.cs`

**Library:** `obs-websocket-dotnet` (NuGet).

**Configuration:**
*   `ObsAddress`: `ws://localhost:4455`
*   `ObsPassword`: User provided.
*   `HymnSceneName`: Scene to switch to when hymn is shown.
*   `BlankSceneName`: Scene to switch to when display is blanked.

**Logic:**
*   Monitor `HymnDisplayService.IsDisplayVisible`.
*   If `true` -> `obs.SetCurrentProgramScene(HymnSceneName)`.
*   If `false` -> `obs.SetCurrentProgramScene(BlankSceneName)`.

## Implementation Plan

### Step 1: "Stream-Ready" Display Profiles
*   Create distinct preset profiles in `DisplayProfileService`:
    *   **"OBS Chroma Key (Green)"**: Green background, White text, Bottom aligned.
    *   **"OBS Chroma Key (Magenta)"**: Magenta background, White text, Bottom aligned.
    *   **"OBS Transparent"**: Transparent background (Avalonia), White text with Shadow.

### Step 2: Browser Source Page
*   Create `Resources/web/obs.html`.
*   Implement minimal Vue.js logic to listen to SignalR `ReceiveState`.
*   Style with CSS for broadcast-quality typography (text-shadows, outlines).

### Step 3: OBS Connection Settings
*   Add "OBS" tab to Settings Window.
*   Inputs: Host, Port, Password.
*   Inputs: Scene Names (Dropdown list fetched from OBS).
*   Toggle: "Enable OBS Automation".

### Step 4: Automation Logic
*   Implement `ObsControlService`.
*   Connect to `HymnDisplayService` events.
*   Send WebSocket commands on state change.

## User Workflows

### Scenario A: The "Green Screen" Method (Standard)
1.  User selects "OBS Chroma Key" profile in SDAHymns.
2.  Display Window turns bright green with lyrics at bottom.
3.  In OBS, User adds "Window Capture".
4.  Right-click Source -> Filters -> Add "Chroma Key".
5.  Select "Green". Background vanishes, lyrics remain.

### Scenario B: The "Browser Source" Method (Pro)
1.  User enables Remote Server (Spec 015).
2.  In OBS, User adds "Browser".
3.  URL: `http://localhost:5000/obs/output`.
4.  Lyrics appear with perfect alpha transparency. No color fringing.

### Scenario C: Automation
1.  User configures OBS WebSocket.
2.  Service starts. User selects Hymn 20.
3.  SDAHymns automatically tells OBS to switch to "Worship" scene.
4.  User hits "Blank".
5.  SDAHymns tells OBS to switch back to "Pulpit Camera" scene.

## Acceptance Criteria

- [ ] Preset profiles for Green/Magenta Chroma Key exist.
- [ ] Display Window renders correctly with these profiles.
- [ ] Browser Source endpoint (`/obs/output`) renders current slide.
- [ ] Browser Source background is transparent.
- [ ] OBS WebSocket connection can be configured and tested.
- [ ] Automation triggers scene changes based on blank/unblank state.

## Future Enhancements
-   **NDI Output:** Send video stream directly via NDI (Network Device Interface).
-   **Multi-View:** Output different content to different OBS instances.
-   **Advanced Triggers:** Show specific "Stinger" transitions when changing hymns.

# Spec 015: Remote Control API

**Status:** 📋 Planned
**Created:** 2025-12-26
**Dependencies:** 006-enhanced-control-window.md, 014-cli-interface.md

## Overview

Embed a lightweight web server within the application to provide a Remote Control interface accessible from any device on the local network (smartphones, tablets, laptops). This allows worship leaders or AV technicians to control the presentation from anywhere in the sanctuary.

**Goal:** Provide a polished, responsive, mobile-first web interface for controlling hymn display.

## Goals

1.  **Embedded Web Server:** Run a Kestrel (ASP.NET Core) server inside the Desktop application.
2.  **Mobile Web App:** A polished, "app-like" Single Page Application (SPA) served by the server.
3.  **Real-Time Sync:** Use SignalR to ensure the mobile interface stays perfectly in sync with the desktop state.
4.  **Security:** Simple PIN/Password authentication to prevent unauthorized access.
5.  **Optional:** Feature must be opt-in (disabled by default) via Settings.
6.  **Zero Install:** Users just scan a QR code or type a URL; no app installation required on the phone.

## Architecture

### 1. Backend (ASP.NET Core)

**Integration:**
We will add `Microsoft.AspNetCore.App` framework reference (or relevant NuGet packages if not using the SDK) to `SDAHymns.Desktop`. The server lifecycle is managed by the `App.axaml.cs` startup logic.

**Service:** `src/SDAHymns.Desktop/Services/Remote/WebServerService.cs`

*   **Endpoints (API):**
    *   `POST /api/auth/login`: Validates PIN, returns JWT or Session Cookie.
    *   `GET /api/state`: Returns current hymn, verse index, blank status.
    *   `POST /api/control/load`: Load hymn by ID.
    *   `POST /api/control/nav`: Next/Prev/Goto.
    *   `GET /api/library/search`: Search hymns.
*   **Hub (SignalR):** `HymnHub`
    *   Server -> Client: `ReceiveState(AppState state)` (pushed on any change).
    *   Client -> Server: `SendCommand(string command, object args)` (alternative to REST).

### 2. Frontend (Vue.js + Tailwind)

To ensure a polished UI without a complex Node.js build chain in the repo, we will serve a static HTML file that loads Vue.js and Tailwind (or a pre-compiled CSS file).

**Structure:**
*   `Resources/web/index.html`
*   `Resources/web/app.js` (Vue application logic)
*   `Resources/web/styles.css`

**UI Layout (Mobile View):**
*   **Top Bar:** Status ("Connected"), Menu button.
*   **Main Area:**
    *   *If Hymn Loaded:* Current Slide Text (Large Preview), "Next/Prev" floating buttons.
    *   *If Idle:* Search Bar and "Recent Hymns" list.
*   **Bottom Nav:** [Remote] [Search] [Settings]

### 3. Security

*   **Storage:** PIN stored in `AppSettings` (Key: `RemoteControlPassword`).
*   **Default:** Randomly generated 4-digit PIN on first run, user can change it.
*   **Auth:** Simple Bearer Token or Cookie checking middleware.

## Implementation Plan

### Step 1: Core Integration
*   Add `Microsoft.AspNetCore.Server.Kestrel` and `Microsoft.AspNetCore.SignalR` dependencies.
*   Create `IWebServerService` to manage Start/Stop logic.
*   Bind to `http://0.0.0.0:{Port}`.

### Step 2: API & SignalR Hub
*   Implement `HymnHub`.
*   Connect `MainWindowViewModel` events to `HymnHub` (e.g., `PropertyChanged` -> push update to clients).
*   Implement Controller endpoints for Search and Auth.

### Step 3: Web App Development
*   Design the HTML/CSS interface.
*   Implement Vue.js logic:
    *   Connect to SignalR Hub.
    *   Handle state updates (update UI when desktop changes).
    *   Handle user input (buttons -> API calls).
*   Implement "Login Screen" overlay.

### Step 4: Settings UI (Desktop)
*   Add "Remote Control" tab to Settings.
*   Toggle: "Enable Remote Server".
*   Input: "Port" (Default 5000).
*   Input: "Access Password".
*   **QR Code Generator:** Generate QR code for `http://{LocalIP}:{Port}` using a library like `QRCoder` or `Net.Codecrete.QrCodeGenerator`.

## User Workflows

### Scenario A: Worship Leader Control
1.  Leader enables "Remote Control" in app settings on the PC.
2.  Leader scans QR code on PC screen with iPad.
3.  iPad opens browser, asks for Password.
4.  Leader enters password.
5.  iPad shows large "Next Verse" button.
6.  Leader taps "Next" while singing -> PC advances slide instantly.

### Scenario B: Emergency Search
1.  PC operator is busy fixing a mic.
2.  Assistant pulls out phone, connects to Remote.
3.  Goes to "Search" tab.
4.  Finds "Hymn 50".
5.  Taps "Load".
6.  Hymn loads on main screen immediately.

## Acceptance Criteria

- [ ] Web server starts/stops based on user setting.
- [ ] Users can connect via browser using local IP.
- [ ] PIN authentication prevents unauthorized access.
- [ ] Mobile UI is responsive and "app-like" (no zooming needed).
- [ ] "Next/Prev" buttons on phone control desktop with <100ms latency.
- [ ] Desktop state changes (e.g., changing slide via keyboard) reflect on phone instantly (SignalR).
- [ ] Search functionality works on mobile.
- [ ] QR Code displayed in Desktop Settings for easy connection.

## Future Enhancements
-   **Stage View:** A specific mode for the web app that only shows the *next* verse (monitor for singers).
-   **Service Plan Editing:** Allow reordering the service plan from the phone.
-   **PWA Support:** Make the web app installable to the home screen.

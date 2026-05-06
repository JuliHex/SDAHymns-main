# Spec 014: CLI Interface & IPC

**Status:** 📋 Planned
**Created:** 2025-12-26
**Dependencies:** 001-project-structure.md, 005-basic-hymn-display.md

## Overview

Implement a comprehensive Command Line Interface (CLI) that can control the application and query data. This feature enables automation, integration with external tools (StreamDeck, scripts), and allows the application to run in a "Headless/Minimal" mode where the CLI acts as the primary controller for the projection window.

**Goal:** Create a robust, scriptable interface for the entire SDAHymns ecosystem.

## Goals

1.  **Dual-Mode CLI:**
    *   **Direct Mode:** Queries database directly (Search, Config, Stats) without needing the GUI.
    *   **Control Mode:** Sends commands to the running GUI via IPC (Inter-Process Communication).
2.  **Display-Only Mode:** Ability to launch the Desktop app with *only* the projection window (no control UI), controlled exclusively by the CLI.
3.  **Machine-Readable Output:** Full JSON support (`--json`) for all commands to enable easy parsing by AI agents and scripts.
4.  **Comprehensive Control:** Full control over navigation, blanking, searching, and configuration.

## Architecture

### 1. IPC Mechanism (Named Pipes)

*   **Technology:** `System.IO.Pipes.NamedPipeServerStream` (Server) and `NamedPipeClientStream` (Client).
*   **Pipe Name:** `SDAHymns_Pipe_{UserId}` (User-scoped to prevent permission issues).
*   **Protocol:** JSON-RPC style messages.
    *   Request: `{ "jsonrpc": "2.0", "method": "hymn.show", "params": [20], "id": 1 }`
    *   Response: `{ "jsonrpc": "2.0", "result": "ok", "id": 1 }`

### 2. Desktop App Updates

We need to support a new startup mode in `SDAHymns.Desktop`.

**File:** `src/SDAHymns.Desktop/App.axaml.cs` (Logic modification)

```csharp
// Pseudo-code logic for OnFrameworkInitializationCompleted
if (args.Contains("--display-only"))
{
    // Launch ONLY the DisplayWindow (Projection)
    // Do NOT show the Control Window (MainWindow)
    // Start the IPC Server to listen for commands
    desktop.MainWindow = new DisplayWindow();
}
else
{
    // Standard Mode: Launch Control Window
    // Start IPC Server as well
    desktop.MainWindow = new MainWindow();
}
```

### 3. CLI Command Structure

We will use `CommandLineParser` verbs to organize commands.

#### A. Control Commands (Requires IPC)
*   `hymn show <number>`: Load and display a hymn.
*   `hymn next` / `hymn prev`: Navigate verses.
*   `slide goto <index>`: Jump to specific slide/verse.
*   `display blank [--on|--off]`: Toggle black screen, or set explicit state.
*   `display theme <profile_id>`: Switch display profile.
*   `app quit`: Close the GUI.

#### B. Data/Query Commands (Direct DB Access)
*   `search "<query>"`: Search hymns (returns list).
*   `hymn get <number>`: Get full hymn details (lyrics, meta).
*   `stats show`: Show usage statistics.
*   `config list`: List current settings.
*   `config set <key> <value>`: Update a setting.

## Implementation Plan

### Step 1: IPC Service (Core)
Create `IpcService` in `SDAHymns.Core` that handles the Named Pipe logic.
*   **Server:** Listens for JSON strings, parses them, triggers events/callbacks.
*   **Client:** Connects to pipe, sends JSON string, waits for response.

### Step 2: Desktop Integration
*   Modify `App.axaml.cs` to parse command-line arguments.
*   Implement `--display-only` mode logic.
*   Start `IpcServer` on startup.
*   Map IPC messages to `MainWindowViewModel` actions (e.g., `hymn.show` -> `LoadHymnAsync`).

### Step 3: CLI Implementation
*   Implement Verbs using `CommandLineParser`.
*   **Launch Logic:**
    *   When a control command is issued (`hymn show`):
    *   Try to connect to Pipe.
    *   If failed -> Check if `--autostart` flag is present? Or just fail?
    *   *Decision:* If pipe not found, attempt to launch `SDAHymns.Desktop.exe --display-only` in background, wait 2s, retry connection.

### Step 4: Output Formatting
*   Create an `OutputFormatter` class.
*   If `--json` flag is present, serialize result object to JSON.
*   If not, use `ConsoleTable` or formatted strings for human readability.

## User Workflows

### Scenario A: Automation Script
User wants to display Hymn 20 automatically at 11:00 AM.
`script.bat`:
```bat
sda-cli hymn show 20
sda-cli display blank --off
```

### Scenario B: AI Assistant
An AI agent needs to know the lyrics of Hymn 50.
Command: `sda-cli hymn get 50 --json`
Output:
```json
{
  "number": 50,
  "title": "Domnul e Păstorul meu",
  "verses": [
    { "number": 1, "content": "..." },
    { "number": 2, "content": "..." }
  ]
}
```

### Scenario C: Minimalist Operator
Operator prefers terminal over GUI.
1.  Run `sda-cli hymn show 1`. (App launches in background, projection window appears).
2.  Press `ArrowUp` (Shell history) -> `sda-cli hymn next` to advance.

## Acceptance Criteria

- [ ] Desktop app supports `--display-only` flag (launches only projection window).
- [ ] IPC mechanism (Named Pipes) works reliably between CLI and Desktop.
- [ ] CLI automatically launches Desktop app if not running when control command issued.
- [ ] All `Control` commands function correctly (Show, Next, Prev, Blank).
- [ ] All `Query` commands return correct data from DB.
- [ ] `--json` flag produces valid, parseable JSON for all outputs.
- [ ] `config set` correctly updates `AppSettings` in database.

## Future Enhancements
-   **Interactive Mode:** A TUI (Text User Interface) mode for the CLI (`sda-cli interactive`) using `Spectre.Console` for a live dashboard in the terminal.
-   **Remote Pipe:** Allow exposing the pipe over TCP (securely) for LAN control (precursor to Spec 015 Remote API).

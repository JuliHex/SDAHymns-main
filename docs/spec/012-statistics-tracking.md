# Spec 012: Statistics Tracking

**Status:** 📋 Planned
**Created:** 2025-12-26
**Dependencies:** 006-enhanced-control-window.md

## Overview

Implement a straightforward statistics tracking system to record how often hymns are sung. Usage is determined by a logic-based trigger (visiting majority of slides) rather than simple clicks, providing accurate data on congregation favorites and hymn usage history.

**Goal:** Provide a reliable list of hymn usage statistics to help worship leaders understand which hymns are frequently or rarely used.

## Goals

1.  **Usage Trigger:** Automatically log a hymn as "Sung" when 75% of its verses have been displayed.
2.  **Statistics List:** A sortable view showing usage counts and last-sung dates for all hymns.
3.  **Hymn History:** Maintain a detailed log of usage events.
4.  **Data Management:** Ability to reset statistics (clear history).
5.  **Export:** Ability to export the statistics list to CSV.

## Architecture

### 1. Tracking Logic (The "75% Rule")

To avoid logging hymns that are just previewed or skipped through quickly, we implement a threshold logic.

**Service:** `src/SDAHymns.Core/Services/StatisticsService.cs`

*   **State:** The service maintains a `HashSet<int> VisitedVerses` for the currently loaded hymn.
*   **Event:** Listens to `HymnDisplayService.CurrentVerseChanged`.
*   **Logic:**
    1.  When hymn loads, clear `VisitedVerses`.
    2.  As user navigates, add Verse IDs to set.
    3.  Check Condition: `if (!AlreadyLogged && VisitedVerses.Count / TotalVerses >= 0.75)`
    4.  If true -> Log Usage, Update Hymn Counters, Mark `AlreadyLogged = true`.

### 2. UI - Statistics View

A new tab in the Control Window or a separate dialog window.

**File:** `src/SDAHymns.Desktop/Views/StatisticsView.axaml`

**Components:**
*   **Header:** Summary ("Total Hymns Sung: 1,245", "Unique Hymns: 320").
*   **Filter:** Category dropdown (All, Crestine, etc.).
*   **Data Grid:**
    *   **Rank:** 1, 2, 3...
    *   **Hymn:** "20. Aleluia! Răsună cântec"
    *   **Times Sung:** "45" (Sortable)
    *   **Last Sung:** "Dec 24, 2025" (Sortable)
*   **Footer Actions:**
    *   [Export to CSV]
    *   [Reset Statistics] (Red button, requires confirmation)

### 3. Data Integration

*   **Detailed Log:** Create a new `UsageStatistic` record for every "Sung" event.
*   **Aggregate Counters:** Update `Hymn.AccessCount` and `Hymn.LastAccessedAt` immediately.
    *   *Note:* Using the aggregate columns on the Hymn table makes the "Sort by Popularity" feature extremely fast compared to querying the history table every time.

## Implementation Plan

### Step 1: Tracking Logic Implementation
*   Extend `HymnDisplayService` or `MainWindowViewModel` to track verse navigation.
*   Implement the 75% calculation logic.
*   Call `StatisticsService.LogUsageAsync(hymnId)`.

### Step 2: Statistics Service
*   `LogUsageAsync`:
    *   Add `UsageStatistic` row.
    *   Increment `Hymn.AccessCount`.
    *   Update `Hymn.LastAccessedAt`.
    *   `SaveChangesAsync`.
*   `ResetStatisticsAsync`:
    *   Truncate `UsageStatistic` table.
    *   Set `AccessCount = 0` and `LastAccessedAt = null` for all hymns.

### Step 3: UI Implementation
*   Create the `StatisticsViewModel`.
*   Load data from `Hymns` table (ordered by `AccessCount` descending by default).
*   Implement "Export to CSV" using a simple StringBuilder (Comma Separated Values).

## Acceptance Criteria

- [ ] navigating through >75% of a hymn's slides triggers a usage log.
- [ ] Usage is logged only once per hymn session (no duplicates if scrolling back/forth).
- [ ] Statistics View displays correct counts and dates.
- [ ] Sorting by "Times Sung" works (Ascending/Descending).
- [ ] Sorting by "Last Sung" works.
- [ ] "Reset Statistics" clears all history and counters.
- [ ] CSV export produces a valid file with all columns.

## Future Enhancements
-   **Visual Graph:** Line chart showing usage over time.
-   **Date Range Filter:** "Show stats for 2024 only".
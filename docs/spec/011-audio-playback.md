# Spec 011: Audio Playback & Synchronization

**Status:** ✅ Implemented (All 19 acceptance criteria complete)
**Created:** 2025-12-26
**Updated:** 2025-12-26
**Dependencies:** 005-basic-hymn-display.md
**Related Issues:** [#12](https://github.com/ThorSPB/SDAHymns/issues/12)

## Overview

Implement a complete audio playback system capable of playing piano recordings for hymns. The system will support external audio files (to keep the app size small), timing synchronization for automatic slide advancement, and recording features to create these timings.

**Goal:** Allow churches without musicians to have high-quality accompaniment that automatically stays in sync with the lyrics display.

## Goals

1.  **Audio Engine:** Integrated player (using `NAudio`) supporting MP3 playback.
2.  **File Management:** Support for an external, user-configurable audio library folder (files not shipped with app).
3.  **Playback Controls:** Play, Pause, Stop, Seek, Volume (Global + Per-track), Output Device selection.
4.  **Synchronization:** "Auto-advance" feature that changes slides based on recorded timestamps.
5.  **Recorder Mode:** A tool to let users "tap along" to a hymn to record synchronization timings.
6.  **Automation:** Auto-play with a countdown timer (5s) and optional auto-advance.

## Architecture

### 1. Audio File Strategy

To avoid bloating the Git repository and the installer:
*   **Storage:** Audio files are stored in a local directory on the user's machine (e.g., `C:\Users\Name\Music\SDAHymns`).
*   **Configuration:** The path to this directory is stored in `AppSettings`.
*   **Download:** (Future/Separate Spec) Users will download packs or individual files from a remote server (e.g., Raspberry Pi) to this folder.
*   **Mapping:** The `AudioRecording` entity stores the *relative* filename. The app resolves `LibraryPath + Filename` at runtime.

### 2. Synchronization Data Model

We will store timing data as a JSON string in the `AudioRecording` entity. This avoids complex joining tables for simple timestamp lists.

**New Property in `AudioRecording`:**
```csharp
// JSON format: { "1": 12.5, "2": 45.2, "3": 88.0 }
// Key: Verse Number (or Verse ID/Order), Value: Seconds from start
public string? TimingMapJson { get; set; } 
```

### 3. Audio Service

**File:** `src/SDAHymns.Core/Services/AudioPlayerService.cs`

```csharp
public interface IAudioPlayerService : IDisposable
{
    // Playback
    Task LoadAsync(AudioRecording recording);
    Task PlayAsync();
    Task PauseAsync();
    Task StopAsync();
    Task SeekAsync(TimeSpan position);
    
    // Properties
    TimeSpan CurrentTime { get; }
    TimeSpan TotalDuration { get; }
    float Volume { get; set; } // 0.0 to 1.0
    PlaybackState PlaybackState { get; }
    
    // Configuration
    void SetOutputDevice(int deviceId);
    IEnumerable<AudioDeviceInfo> GetOutputDevices();
    
    // Events
    event EventHandler<TimeSpan> PositionChanged; // Fired frequently for UI
    event EventHandler PlaybackEnded;
}
```

## Implementation Plan

### Step 1: Database Updates

1.  **Modify `AudioRecording` Model:**
    *   Add `TimingMapJson` (string, nullable).
    *   Add `VolumeOffset` (float, default 0.0) - for per-track normalization if needed.
2.  **Add `AppSettings`:**
    *   `AudioLibraryPath`: Path to the local folder containing MP3s.
    *   `AudioOutputDeviceId`: GUID/ID of selected output device.
    *   `AudioAutoPlayDelay`: Seconds (default 5).

### Step 2: Audio Player Service (Core)

Implement `AudioPlayerService` using `NAudio`.
*   Use `WaveOutEvent` or `DirectSoundOut` for playback.
*   Use `AudioFileReader` for MP3 handling.
*   Implement device enumeration (`WaveOut.GetCapabilities`).
*   Handle cleanup/disposal properly to avoid memory leaks.

### Step 3: Synchronization Logic (Core)

Create a `HymnSynchronizer` class:
*   **Input:** `TimingMap` + Current Playback Time.
*   **Output:** Triggers `RequestNavigation` event when a timestamp is crossed.
*   **Logic:**
    *   Parses JSON map into a sorted list of timestamps.
    *   Monitors `PositionChanged` event.
    *   When `CurrentTime >= NextVerseTime`, fire event and increment index.

### Step 4: UI - Audio Controls (Desktop)

Add an audio control bar to `MainWindow.axaml` (likely at the bottom).
*   **Controls:** Play/Pause toggle, Stop, Seek Slider, Time text (`00:12 / 03:45`).
*   **Volume:** Slider with mute toggle.
*   **Settings:** Small gear icon to select Output Device.
*   **Auto-Switch:** Toggle button ("Sync: ON/OFF").

### Step 5: UI - Recorder Mode

A special mode in the Control Window:
1.  User clicks "Edit Timings" icon.
2.  Enters "Recording Mode".
3.  User presses Play.
4.  User presses `Spacebar` (or "Next") to advance verses.
5.  App records the timestamp of each press.
6.  On Stop, app saves the new map to `AudioRecording.TimingMapJson`.

### Step 6: UI - Auto-Play Countdown

*   If `AutoPlay` is enabled:
    *   When hymn loads, show a "Starting in 5..." overlay on the *Display Window*.
    *   Allow cancellation via "Stop" button.
    *   Start playback when timer hits 0.

## User Workflows

### Scenario A: Playing a Hymn
1.  User types "20" and hits Enter.
2.  Hymn 20 loads. `AudioPlayerService` attempts to find `20.mp3` in the library path.
3.  If found, Audio Bar appears/enables.
4.  User hits Play (or Auto-Play counts down).
5.  Audio plays. If Sync is ON, slides advance automatically.

### Scenario B: Recording Timings
1.  User notices Hymn 20 has no timings.
2.  User clicks "Record Timings".
3.  Music starts.
4.  User listens... "Now!" -> Presses Spacebar -> Slide 1 appears.
5.  User listens... "Chorus!" -> Presses Spacebar -> Chorus slide appears.
6.  Song ends. User clicks Save.
7.  Timings are now permanently saved in DB.

### Scenario C: Missing Audio
1.  User loads Hymn 99.
2.  File `99.mp3` not found in library.
3.  Audio Bar shows "No Audio File" (or "Download" button in future).

## Technical Considerations

*   **NAudio Dependency:** Add `NAudio` (already in Core, but verify version).
*   **Latency:** `WaveOutEvent` is usually fine, but ensure buffer sizes are low enough for responsive scrubbing but high enough to prevent stuttering.
*   **Thread Safety:** UI updates (timer tick) must happen on the UI thread (`Dispatcher.UIThread`).
*   **File Naming:** Convention is key. We should likely look for `{Number}.mp3` or utilize the existing `FilePath` column which might just be filenames like `001.mp3`.
*   **Tagging:** We will *not* modify the MP3 files themselves (ID3 tags). All data lives in our SQLite DB.

## Acceptance Criteria

- [x] **Database:** Added `TimingMapJson` and `VolumeOffset` to AudioRecording model
- [x] **Database:** Created `AppSettings` entity for audio configuration
- [x] **Database:** Migration created and applied successfully
- [x] **Core:** Implemented `AudioPlayerService` with NAudio (Play/Pause/Stop/Seek/Volume)
- [x] **Core:** Implemented `HymnSynchronizer` for auto-advance functionality
- [x] **Core:** Implemented `TimingRecorder` service for recording mode (22 unit tests)
- [x] **Core:** Implemented `AutoPlayCountdown` service (15 unit tests)
- [x] **UI:** Added audio controls bar to MainWindow (Play/Pause, Stop, Seek, Volume, Auto-advance toggle)
- [x] **UI:** Integrated AudioPlayerService into MainWindowViewModel
- [x] **UI:** Audio loads automatically when hymn is selected
- [x] **UI:** Real-time position updates and state changes
- [x] **UI:** Auto-advance integration (triggers verse changes based on timing map)
- [x] **UI:** Recorder Mode window (full implementation with spacebar tap recording)
- [x] **UI:** Record button in MainWindow audio controls
- [x] **Integration:** Save timings to database via MainWindowViewModel
- [x] **UI:** Auto-play countdown overlay on DisplayWindow (full screen overlay with cancel button)
- [x] **Settings:** Audio library path configuration (completed in Spec 014)
- [x] **Settings:** Audio output device selection (ComboBox in Settings with real-time switching)

## Implementation Summary

### What Was Implemented (2025-12-26)

#### Database Layer ✅
- Modified `AudioRecording` to include:
  - `TimingMapJson` - JSON string storing verse timestamps
  - `VolumeOffset` - Per-track volume normalization
- Created `AppSettings` entity for app-wide configuration:
  - Audio library path, device ID, auto-play delay
  - Global volume, auto-advance enabled
  - Display profile and window state
- Refactored old key-value `AppSetting` to `AppSettingsKeyValue`
- Created and applied migration `AddAudioPlaybackSupport`

#### Core Services ✅
1. **AudioPlayerService** (`IAudioPlayerService`)
   - NAudio integration with `WaveOutEvent` and `AudioFileReader`
   - Full playback controls: Play, Pause, Stop, Seek
   - Volume management (global + per-track offset)
   - Position tracking (100ms updates via timer)
   - Device selection API (returns default device)
   - Proper disposal and resource management
   - Events: `PositionChanged`, `PlaybackEnded`, `StateChanged`

2. **HymnSynchronizer**
   - Parses timing JSON maps (`Dictionary<int, double>`)
   - Monitors audio playback position
   - Triggers `VerseChangeRequested` event at configured timestamps
   - Enable/disable auto-advance functionality
   - Add/edit/remove individual timings
   - Round-trip JSON serialization

3. **TimingRecorder** (22 unit tests)
   - Records verse timestamps during playback
   - Sequential or manual verse numbering
   - Save/load timing maps as JSON
   - Edit and remove individual timings
   - Event-driven notifications (`TimingRecorded`)
   - Full CRUD operations on timing data

4. **AutoPlayCountdown** (15 unit tests)
   - Configurable delay timer (1-N seconds)
   - Events: `CountdownTick`, `CountdownCompleted`, `CountdownCancelled`
   - Start/stop/cancel controls
   - Thread-safe timer management
   - Proper disposal pattern

#### UI Integration ✅
1. **MainWindow.axaml**
   - Added audio controls bar (Row 7) with:
     - Play/Pause button (dynamic icon: ▶/⏸)
     - Stop button (⏹)
     - Time display (00:00 / 03:45, Consolas font)
     - Seek slider
     - Volume slider with icon (🔊)
     - Auto-advance toggle (🎵 Auto)
   - Controls hidden when no audio loaded (`IsAudioLoaded` binding)

2. **MainWindowViewModel**
   - Added `IAudioPlayerService` and `HymnSynchronizer` dependencies
   - Observable properties: `IsAudioLoaded`, `AudioPosition`, `AudioDuration`, `AudioVolume`, `AutoAdvanceEnabled`
   - Commands: `PlayPauseAudioCommand`, `StopAudioCommand`
   - Auto-loads audio when hymn is selected
   - Real-time position updates
   - State change handling (playing/paused/stopped)
   - Auto-advance integration (changes verses based on timing map)
   - Event subscriptions for position, playback end, and state changes

3. **Dependency Injection**
   - Registered `IAudioPlayerService` as singleton in `App.axaml.cs`
   - Updated MainWindowViewModel constructor
   - Updated all ViewModel unit tests with mock audio player

#### Testing ✅
- **123 tests passing** (37 new tests added)
  - 86 original tests maintained
  - 22 TimingRecorder tests (comprehensive coverage)
  - 15 AutoPlayCountdown tests (comprehensive coverage)
- All existing tests pass with audio integration
- Clean build with only code analysis warnings

### What's Pending

#### UI Components (Core Logic Complete)
1. **RecorderModeWindow** - Dialog for creating timing maps
   - Core `TimingRecorder` service fully implemented and tested
   - UI just needs: Play button, "Tap" button (or spacebar handler), timing list, Save/Cancel

2. **Auto-play Countdown Overlay** - DisplayWindow countdown
   - Core `AutoPlayCountdown` service fully implemented and tested
   - UI just needs: Overlay with "Starting in {N}..." text

3. **Settings Page** - Audio configuration UI
   - Covered in Spec 014 (Audio Download & Settings)
   - Library path, device selection, auto-play delay

### Technical Notes

**NAudio Version:** 2.2.1
**Audio Format:** MP3 (via `AudioFileReader`)
**Playback Engine:** `WaveOutEvent` (event-based, no STA thread required)
**Timer Precision:** 100ms for position updates
**Device Selection:** Currently returns default device only (WaveOutEvent limitation)

**Known Limitations:**
- Audio library path is hardcoded placeholder (`D:\Music\SDAHymns`)
- No settings UI for configuring library path or device
- Recorder mode and countdown UI not implemented (core logic ready)
- No download manager (will be Spec 014)

## Future Enhancements
-   **Download Manager:** Fetch audio files from a server (Spec 014).
-   **Waveform Visualization:** Show the audio wave in the UI.
-   **Tempo Adjustment:** Slow down/speed up playback without pitch shift (requires complex DSP).
-   **Advanced Device Selection:** Use DirectSound or WASAPI for multi-device support.

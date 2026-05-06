# Installation Guide - SDAHymns

This guide explains how to download and install SDAHymns on your computer.

## Download

### Latest Release

Go to the **Releases** page: [https://github.com/ThorSPB/SDAHymns/releases](https://github.com/your-org/SDAHymns/releases)

Download the file for your operating system:

| Platform | File | Size |
|----------|------|------|
| **Windows** | `SDAHymns-vX.X.X-windows-x64.zip` | ~90 MB |
| **macOS (Apple Silicon)** | `SDAHymns-vX.X.X-macos-arm64.zip` | ~80 MB |
| **macOS (Intel)** | `SDAHymns-vX.X.X-macos-x64.zip` | ~80 MB |
| **Linux** | `SDAHymns-vX.X.X-linux-x64.tar.gz` | ~80 MB |

> **Not sure which macOS version?**
> - M1, M2, M3 Macs → Download **arm64**
> - Intel Macs → Download **x64**

---

## Installation

### Windows

1. **Download** `SDAHymns-vX.X.X-windows-x64.zip`

2. **Extract** the ZIP file
   - Right-click → "Extract All..."
   - Choose a location (e.g., `C:\Program Files\SDAHymns`)

3. **Run** the application
   - Open the extracted folder
   - Double-click `SDAHymns.Desktop.exe`

4. **(First Time) Windows Security Warning**
   - Windows may show "Windows protected your PC"
   - Click **"More info"** → **"Run anyway"**
   - This happens because the app is not signed (coming in future release)

5. **Create Desktop Shortcut** (Optional)
   - Right-click `SDAHymns.Desktop.exe`
   - Select "Create shortcut"
   - Move shortcut to Desktop

**No .NET installation required!** The app is self-contained.

---

### macOS

1. **Download** the appropriate file:
   - Apple Silicon (M1/M2/M3): `SDAHymns-vX.X.X-macos-arm64.zip`
   - Intel: `SDAHymns-vX.X.X-macos-x64.zip`

2. **Extract** the ZIP file
   - Double-click the downloaded file
   - macOS will extract it automatically

3. **Move to Applications** (Optional but Recommended)
   ```bash
   mv SDAHymns.Desktop /Applications/
   ```

4. **Make executable**
   ```bash
   chmod +x /Applications/SDAHymns.Desktop
   ```

5. **Run** the application
   - Open Terminal
   - Run: `/Applications/SDAHymns.Desktop`

   OR:
   - Double-click in Finder (if you moved to Applications)

6. **(First Time) Gatekeeper Warning**
   - macOS may block the app: "cannot be opened because it is from an unidentified developer"
   - Fix:
     - Open **System Preferences** → **Security & Privacy**
     - Click **"Open Anyway"** next to the SDAHymns message
     - OR run: `xattr -d com.apple.quarantine /Applications/SDAHymns.Desktop`

**No .NET installation required!** The app is self-contained.

---

### Linux

1. **Download** `SDAHymns-vX.X.X-linux-x64.tar.gz`

2. **Extract** the archive
   ```bash
   tar -xzf SDAHymns-vX.X.X-linux-x64.tar.gz
   cd SDAHymns-vX.X.X-linux-x64
   ```

3. **Make executable**
   ```bash
   chmod +x SDAHymns.Desktop
   ```

4. **Run** the application
   ```bash
   ./SDAHymns.Desktop
   ```

5. **(Optional) Install to /opt**
   ```bash
   sudo mv SDAHymns.Desktop /opt/sdahymns/
   sudo ln -s /opt/sdahymns/SDAHymns.Desktop /usr/local/bin/sdahymns

   # Now you can run from anywhere:
   sdahymns
   ```

6. **(Optional) Create Desktop Entry**
   Create file: `~/.local/share/applications/sdahymns.desktop`
   ```ini
   [Desktop Entry]
   Name=SDA Hymns
   Comment=Romanian SDA Hymnal Application
   Exec=/opt/sdahymns/SDAHymns.Desktop
   Icon=audio-card
   Terminal=false
   Type=Application
   Categories=Audio;Music;
   ```

**No .NET installation required!** The app is self-contained.

---

## Troubleshooting

### Windows: "Windows protected your PC"

**Cause:** App is not digitally signed.

**Solution:**
1. Click **"More info"**
2. Click **"Run anyway"**

This is safe - the app is open-source and built by GitHub Actions.

---

### macOS: "Cannot be opened because it is from an unidentified developer"

**Solution 1 (Easy):**
1. Go to **System Preferences** → **Security & Privacy**
2. Click **"Open Anyway"** at the bottom
3. Click **"Open"** in the dialog

**Solution 2 (Command Line):**
```bash
xattr -d com.apple.quarantine /Applications/SDAHymns.Desktop
```

---

### Linux: "Permission denied"

**Cause:** File is not executable.

**Solution:**
```bash
chmod +x SDAHymns.Desktop
./SDAHymns.Desktop
```

---

### App Won't Launch (All Platforms)

**Check:**

1. **Extracted fully?**
   - Make sure ZIP/TAR is fully extracted
   - Don't run from inside the archive

2. **Database exists?**
   - Check `Resources/hymns.db` exists next to the executable
   - If missing, re-download and extract

3. **Antivirus blocking?**
   - Some antivirus may flag the app
   - Add exception if needed

4. **System Requirements:**
   - Windows 10 or later
   - macOS 10.15 (Catalina) or later
   - Linux with X11 or Wayland

---

### Missing Features or Crashes

**First steps:**

1. **Check version**
   - Make sure you downloaded the latest release

2. **Check database**
   - Verify `Resources/hymns.db` is present (should be ~1.6 MB)

3. **Report bug**
   - Open an issue: https://github.com/your-org/SDAHymns/issues
   - Include:
     - Operating system and version
     - SDAHymns version
     - Steps to reproduce
     - Error message (if any)

---

## Updating to New Version

### Manual Update

1. **Download** the new version
2. **Close** the current running application
3. **Delete** the old application folder
4. **Extract** the new version
5. **Run** the new version

**Your settings are preserved** (stored separately in user config folder).

### Future: Auto-Update

A future version will include automatic updates:
- App checks for updates on startup
- One-click update button
- No manual downloads needed

---

## Uninstallation

### Windows

1. Close the application
2. Delete the application folder
3. (Optional) Delete user data:
   - `%APPDATA%\SDAHymns`

### macOS

1. Close the application
2. Delete `/Applications/SDAHymns.Desktop`
3. (Optional) Delete user data:
   - `~/Library/Application Support/SDAHymns`

### Linux

1. Close the application
2. Delete the application files:
   ```bash
   sudo rm /opt/sdahymns/SDAHymns.Desktop
   sudo rm /usr/local/bin/sdahymns
   ```
3. (Optional) Delete user data:
   - `~/.local/share/SDAHymns`
   - `~/.config/SDAHymns`

---

## System Requirements

### Minimum Requirements

| Component | Requirement |
|-----------|-------------|
| **OS** | Windows 10, macOS 10.15, or modern Linux |
| **RAM** | 512 MB |
| **Storage** | 200 MB free space |
| **Display** | 1024x768 or higher |

### Recommended Requirements

| Component | Requirement |
|-----------|-------------|
| **OS** | Windows 11, macOS 13+, or Ubuntu 22.04+ |
| **RAM** | 1 GB or more |
| **Storage** | 500 MB free space |
| **Display** | 1920x1080 or higher (for projection) |

---

## Support

Need help?

- **Documentation:** [GitHub Wiki](https://github.com/your-org/SDAHymns/wiki)
- **Issues:** [Report a bug](https://github.com/your-org/SDAHymns/issues)
- **Discussions:** [Community forum](https://github.com/your-org/SDAHymns/discussions)

---

## License

SDAHymns is open-source software licensed under AGPL-3.0.

See [LICENSE](../LICENSE) for details.

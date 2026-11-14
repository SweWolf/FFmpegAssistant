# FFmpegAssistant

A Windows desktop app that works together with the web browser extension [Privatkopiera](https://github.com/stefansundin/privatkopiera). Privatkopiera gives you an FFmpeg command for the video you are watching; paste it into FFmpeg Assistant, choose a folder and a file name, and the app runs the command for you, with live progress, validation of the downloaded file and automatic retries.

In other words, FFmpeg Assistant is a user interface for the commands that Privatkopiera creates.

![Screenshot](Assets/Screenshot.png)

## Instruction Video
- https://youtu.be/AiEukK-xyYI

## Features

### Downloading
- Auto-suggests the output folder and file name for Movies, TV Shows and Subtitles (the Subtitle option sets the extension to `.srt` and keeps the rest)
- Title box shows (and lets you edit) the detected show or movie title used for the output file name
- Auto-increments episode numbers based on existing files in the folder
- Season and Episode boxes let you override the auto-suggested episode number
- Download queue: add the next downloads while one is running (for example all the episodes of a season); they start one after the other
- Real-time progress grid (duration, frame, FPS, size, time, bitrate, speed, elapsed) with a progress bar
- Estimated remaining time with stable speed sampling, also for downloads counted in parts (such as subtitles)
- When a download is finished, the status shows the total time, for example "Done in 15:11"
- Color-coded status messages: light yellow while in progress, orange when retrying, red on errors and green when done
- Watch while downloading: streams to a .ts file so you can open it immediately, then converts it to the final format automatically when the download is complete
- Live streams (for example a TV channel) are detected from the playlist and recorded in watch mode automatically; click **Stop Download** to finish the recording
- Stream and URL downloads get `-c copy` added automatically, so the video is copied instead of re-encoded (can be turned off in Settings)
- Power outage protection: downloads to a `(part)` file and only renames it to the final name after the file has been validated; if the power goes out, the interrupted download and the queue are offered again the next time the app starts
- Auto-retry on failure: configurable maximum number of attempts; each retry is shown in the Attempt counter
- Validates the downloaded video file after each attempt
- Keeps the PC awake during a download, so it doesn't go to sleep halfway (the screen can still turn off)
- Flashes the taskbar button when a download finishes or fails while you are working in another window

### Tools
- **Validate Video File** (Tools menu): check any video file on your computer with the same FFmpeg check that runs after a download
- Save a downloaded video's subtitles as separate `.srt` files next to it (optional, see Settings)
- Built-in help: **Help > FFmpeg Assistant Help** (F1)
- **Help > Open FFmpeg Call Log** and **Open Log Folder**: see the exact FFmpeg command line of every download attempt
- Right-click menu (Copy, Paste, Clear and Paste) in the text boxes and the folder box of the main window
- Extract an embedded subtitle track from a video file, or download subtitles directly from an M3U8 stream
- Create Desktop and/or Start Menu shortcuts via the Tools menu
- Automatic update check against GitHub Releases on startup

### Safety checks
- Asks before creating an output folder you typed yourself (folders the app suggests are created automatically)
- Checks the output folder and file name for characters that aren't allowed in file names before FFmpeg runs
- File-exists protection before overwriting
- Cancel mid-download with optional cleanup of the partial file
- Close protection: warns if you try to close the app during a download and deletes the partial file automatically

### Fits any screen
- Adapts to small screens and display scaling: when the window is short, the progress grid shows its values side by side, and if that isn't enough, the window scrolls instead of controls overlapping

### Keyboard shortcuts

| Shortcut | Action |
|---|---|
| F1 | Open the help |
| Ctrl+E | Download (Add to Queue during a download) |
| Ctrl+O | Open File |
| Ctrl+Shift+O | Open Folder |
| Alt+B | Browse for the output folder |
| F6 | Go to the Command box |

### Settings (Tools → Settings)

- **FFmpeg path**: set a custom path to `ffmpeg.exe` for systems where FFmpeg is not on the system PATH; leave it empty to use the PATH
- **Auto-retry on failure**: set the maximum number of download attempts (leave it empty, 0 or 1 to disable auto-retry)
- **Check for new version at startup**: Yes/No toggle for the automatic update check
- **Action when download finished**: play a sound (built-in or your own custom file), show a message box, or do nothing
- **Extra folders in the Folder list**: add the folders you often save to
- **Show Save Subtitles as Separate Files**: shows the checkbox in the main window that saves a downloaded video's subtitles as `.srt` files next to it (hidden by default)
- **Add "-c copy" to Stream and URL Commands**: on by default; turn it off if you want FFmpeg to re-encode the video

### Logs

- Application log: `%APPDATA%\SweWolfSoftware\FFmpegAssist\FFmpegAssistant.log`
- FFmpeg's output for each download or validation: `%APPDATA%\SweWolfSoftware\FFmpegAssist\Logs` (opened with the Open Log File button); files older than 30 days are deleted automatically
- The exact FFmpeg command line of every download attempt: `%APPDATA%\SweWolfSoftware\FFmpegAssist\FFmpeg_calls.log` (opened with **Help > Open FFmpeg Call Log**)

## Requirements

- Windows 10 or later
- .NET 10 (only for the small download; the standalone exe includes it)
- FFmpeg, either on the system PATH or configured via Tools → Settings
- The web browser extension Privatkopiera (see https://stefansundin.github.io/privatkopiera)

## License

MIT

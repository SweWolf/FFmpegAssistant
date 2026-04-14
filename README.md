# FFmpegAssistant

A Windows Forms application that helps you run FFmpeg commands by letting you paste an existing command, choose an output folder, and specify a file name — then executes the modified command for you.

Basically, this is user interface for the web browser extension Privatkopiera, so this is like a user interface for that addon. Privatkopera can be downloaded from the website https://github.com/stefansundin/privatkopiera

![Screenshot](Assets/Screenshot.png)

## Features

- Detects FFmpeg commands on the clipboard at startup
- Auto-suggests output folder and file name for Movies and TV Shows
- Auto-increments episode numbers based on existing files in the folder
- Real-time progress grid (duration, frame, FPS) with progress bar
- Estimated remaining time with stable speed sampling
- Cancel mid-download with optional cleanup of the partial file
- File-exists protection before overwriting
- ffprobe validation of the completed file
- Application log stored in `%APPDATA%\SweWolfSoftware\FFmpegAssist`

## Requirements

- Windows 10 or later
- .NET 10
- FFmpeg and ffprobe available on the system PATH

## License

MIT

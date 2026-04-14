# FFmpegAssistant

A Windows Forms application that is an extension to the web browser extension Privatkopiera, that helps you run FFmpeg commands by letting you paste an existing command, choose an output folder, and specify a file name — then executes the modified command for you.

![Screenshot](Assets/Screenshot.png)

## Instruction Video
- https://youtu.be/AiEukK-xyYI

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
- The web browser extension Privatkopiera (see https://stefansundin.github.io/privatkopiera/ )

## License

MIT

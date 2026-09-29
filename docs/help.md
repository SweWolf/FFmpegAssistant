# FFmpeg Assistant Help

FFmpeg Assistant downloads videos from the web and saves them on your computer. It works
together with the web browser extension **Privatkopiera**, which gives you an FFmpeg command for
the video you are watching. Paste the command into FFmpeg Assistant, choose a folder and a file
name, and click **Download**. FFmpeg Assistant runs the command for you, shows the progress,
checks the downloaded file and tries again if the download fails.

## Contents

- [Getting Started](#getting-started)
- [The Main Window](#the-main-window)
  - [Command](#command)
  - [Folder and File Name](#folder-and-file-name)
  - [Auto-Suggest Folder and File Name](#auto-suggest-folder-and-file-name)
  - [Enable Watching While Downloading](#enable-watching-while-downloading)
  - [Save Subtitles as Separate Files](#save-subtitles-as-separate-files)
  - [Downloading](#downloading)
  - [Progress and Status](#progress-and-status)
- [What Happens During a Download](#what-happens-during-a-download)
- [Download Queue](#download-queue)
- [Validate Video File](#validate-video-file)
- [Extract Subtitle File](#extract-subtitle-file)
- [Settings](#settings)
- [Create Shortcut](#create-shortcut)
- [Keyboard Shortcuts](#keyboard-shortcuts)
- [Log Files](#log-files)
- [Troubleshooting](#troubleshooting)

## Getting Started

### What You Need

- Windows 10 or Windows 11.
- **FFmpeg**, the free program that does the actual download. Download it from
  [ffmpeg.org](https://ffmpeg.org/download.html) and install it so that it is on the system PATH,
  or tell FFmpeg Assistant where `ffmpeg.exe` is under **Tools > Settings**.
  [Extract Subtitle File](#extract-subtitle-file) and
  [Save Subtitles as Separate Files](#save-subtitles-as-separate-files) also need `ffprobe.exe`,
  which comes with FFmpeg. Keep it in the same folder as `ffmpeg.exe`.
- The web browser extension
  [Privatkopiera](https://stefansundin.github.io/privatkopiera), which creates the FFmpeg
  commands.
- The small download of FFmpeg Assistant also needs .NET 10. The standalone version includes it.

You can see which FFmpeg version is found under **Help > About**.

### Your First Download

1. Open the video in your web browser, click the Privatkopiera button and copy the FFmpeg
   command it shows.
2. Start FFmpeg Assistant. If the command is still on the clipboard, it is pasted into the
   **Command** box automatically. Otherwise, paste it there yourself.
3. Choose **Movie** or **TV Show** to get a suggested folder and file name, or choose them
   yourself under **Folder** and **File Name**.
4. Click **Download**.

When the download is finished, FFmpeg Assistant plays a sound (you can change this in
[Settings](#settings)). Click **Open File** to watch the video.

## The Main Window

At the top you enter the command and choose where the file is saved. The
**Auto-Suggest Folder and File Name** area helps you name movies and TV show episodes. The
buttons start and control the download, and the lower part of the window shows the progress.

### Command

The FFmpeg command to run, usually copied from Privatkopiera. It is filled in automatically
when:

- the clipboard contains an FFmpeg command when FFmpeg Assistant starts,
- FFmpeg Assistant is started with a command on the command line (see
  [Create Shortcut](#create-shortcut)).

You can also paste just a web address (for example the address of an `.m3u8` playlist), or the
path of an `.m3u8` file on your computer. FFmpeg Assistant turns it into a full command for you:
`ffmpeg -i "<address>"`.

You don't need to change the output file name at the end of the command: FFmpeg Assistant
replaces it with the **Folder** and **File Name** you choose.

### Folder and File Name

**Folder** is where the file is saved. The list contains your **Videos** folder and its
**Movies** and **TV Shows** subfolders, followed by any folders you have added under
[Extra Folders in the Folder List](#extra-folders-in-the-folder-list) in the settings. Choose one
of them, type or paste another folder, or click **...** to browse for one.

If the folder does not exist, it is created when you click **Download**. Folders that
FFmpeg Assistant suggested itself are created without asking. For a folder you typed, you are
asked first.

**File Name** is the name of the downloaded file. If you leave it empty, the file name from the
command is used. If the command has no output file name (for example when you pasted just a web
address), the file is called `output.mp4`, or `subtitles.srt` for subtitles.

You may leave out the extension (such as `.mp4`): it is taken from the output file name in the
command. If the name has another extension, the right one is added after it, so `Episode.The Name` becomes
`Episode.The Name.mp4`.

### Auto-Suggest Folder and File Name

This area fills in **Folder** and **File Name** for you, with the title, year and episode
number in the name. Media servers such as Plex, Jellyfin and Kodi use these names to recognise
the movie or show and fetch its poster, description and episode titles.

#### Movie

Choose **Movie** to save the file in **Videos\\Movies** as `Title (Year).mp4`.

- **Title** is filled in from the file name in the command: the text before the first `-` or
  `[`. You can change it.
- **Year** is optional: the year the movie was released, as shown on
  [IMDb](https://www.imdb.com) or [The Movie Database](https://www.themoviedb.org). It helps
  Plex, Jellyfin and Kodi find the right movie when several have the same title. You can write
  it with 2 or 4 digits: `26` becomes `2026` when you leave the box, and `95` becomes `1995`.

#### TV Show

Choose **TV Show** to save the file in its own folder under **Videos\\TV Shows**, for example
`Videos\TV Shows\My Show (2024)`. Each episode is named `My Show (2024) - s01e05.mp4`.

- **Title** and **Year** work as for movies. Together they give the name of the show's folder.
- For a TV show, **Year** is the year the **show first started**, not the year of the episode
  you are downloading. Use the year shown on IMDb or The Movie Database, even for an episode
  from a later season. This way Plex, Jellyfin and Kodi can tell the show apart from others with
  the same name, for example a remake.
- FFmpeg Assistant looks at the episodes that are already in the folder, and the ones that are
  being downloaded (also in another FFmpeg Assistant window) or are waiting in the
  [Download Queue](#download-queue), and suggests the next one. If `s01e05` is the last one, the
  new file becomes `s01e06`. A `(part)` file left over from an interrupted download doesn't
  count: that episode still needs to be downloaded.
- **Season** and **Episode** show the suggested numbers. Change them to download another episode.
  They are only shown for TV shows.

#### Automatic Recognition

- When you leave the **Title** box, FFmpeg Assistant looks for a folder with that title in
  **Movies** and **TV Shows**, with or without a year, for example `My Show (2024)`. If it finds
  exactly one, it chooses **Movie** or **TV Show** and fills in **Year** for you.
- FFmpeg Assistant remembers which folder under **TV Shows** you downloaded each show to. The
  next time you paste a command for the same show while **TV Show** is chosen, or choose
  **TV Show** after pasting it, that folder is chosen automatically, and the next episode is
  suggested. **Title** and **Year** are filled in from the folder name. This is useful when the
  command has the wrong name for the show: if it says `Gränslandet` and you changed **Title** to
  `Robinson Gränslandet` before you downloaded the first episode, the next episodes get
  `Robinson Gränslandet` automatically.

### Enable Watching While Downloading

Normally you can't watch a video until the download is finished. Check
**Enable Watching While Downloading** to download to a `.ts` file first. You can open it with
**Open File** as soon as the download has started (use a player that can play unfinished files,
such as VLC). When the download is finished, the `.ts` file is converted to the final format and
deleted. The conversion only copies the video and sound, so it is quick and does not lower the
quality.

While the box is checked, FFmpeg Assistant does not play a sound, show a message or flash the
taskbar button when the download is finished, because you are already watching. If you uncheck
the box before the download is finished, you get the usual notification. Errors are always shown.

This option is not used for subtitle files.

### Save Subtitles as Separate Files

Check **Save Subtitles as Separate Files** to save the subtitles that are stored inside the
downloaded video as `.srt` files in the same folder. Some players and TVs only show subtitles from
a separate file.

The box is read when the download is finished and the file has been checked, not when you click
**Download**. So you can still check or uncheck it during the download. Status then shows
"Download finished, extracting subtitles...". Only after that is the download done, and you get
the usual notification.

The subtitle file gets the same name as the video, for example `My Movie.srt`. If the video has
several subtitle tracks, each file also gets the language of the track, and `forced` or `sdh`
(subtitles for the deaf and hard of hearing) if the track is marked that way:
`My Movie.eng.srt`, `My Movie.swe.srt`, `My Movie.eng.forced.srt`. A track without a language gets
its number instead, for example `My Movie.3.srt`. Most players, such as VLC, Plex and Kodi,
understand these names.

If a subtitle file already exists, you are asked whether to overwrite it. Click **No** to keep the
existing file.

Subtitles that are stored as pictures (for example on Blu-ray and DVD) can't be saved as `.srt`
and are skipped. If the video has no subtitles, Status says so. If saving the subtitles fails,
FFmpeg Assistant shows what went wrong. The downloaded video is not affected.

### Downloading

- **Download** starts the download. During a download the button is called **Add to Queue**: it
  adds the next download to the [Download Queue](#download-queue). Once a command is downloading
  or in the queue, the button is disabled until you change the **Command**, **Folder** or
  **File Name**, so the same video isn't downloaded twice by mistake. If the download fails or you
  cancel it, the button is enabled again, so you can start it again.
- **Cancel** stops the download. If a partial file was saved, you are asked whether to delete it.
  If more downloads are waiting in the queue, you are first asked whether to cancel them too.
- **Clear** empties all the boxes, so you can start over. During a download it only empties the
  boxes: the progress of the running download stays.
- **Open File** opens the downloaded file in your default video player.
- **Open Folder** opens the folder in File Explorer, with the file selected if it exists. During a
  download, it opens the folder of the running download.
- **Open Log File** shows FFmpeg's report of the download. Useful when something went wrong.

You can change the boxes during a download to prepare the next one. They are not used until you
click **Add to Queue**. **Tools > Validate Video File...** and **Subtitles > Extract Subtitle File...**
are not available during a download, because they fill in the boxes themselves.

### Progress and Status

The table shows what FFmpeg reports during the download:

| Row | Meaning |
|-----|---------|
| **Duration** | The length of the video |
| **Frame** | The number of video frames downloaded so far |
| **FPS** | Frames per second: how fast the frames are processed |
| **Size** | The size of the file so far, in MB |
| **Time** | How much of the video has been downloaded |
| **Bitrate** | The amount of data per second of video |
| **Speed** | How many times faster than real time the download runs |
| **Elapsed** | How long the download has been running |

For some streams, FFmpeg can't tell the length in advance. Then **Time** shows the number of
downloaded parts instead, for example `120/450`.

The progress bar and **Estimated remaining time** include the check of the file after the
download (see [What Happens During a Download](#what-happens-during-a-download)).

**Status** shows what is going on. Its colour shows how things are going:

- **light yellow**: the downloaded file is being checked,
- **orange**: the download failed and is being tried again,
- **red**: something went wrong,
- **green**: done.

**Job** shows which download of the queue is running: `2/5` means the second of five. Click it to
open the [Download Queue](#download-queue) window.

**Attempt** shows which try the download is on (see
[Auto-Retry on Download Failure](#auto-retry-on-download-failure)).

If the window is small, the table shows its rows in two columns side by side. If that is still not
enough, you can scroll the window.

## What Happens During a Download

FFmpeg Assistant does more than run the command:

1. **Download to a part file.** The video is saved as `My Movie (part).mp4` first. If the power
   goes out or the PC crashes, you won't mistake a broken file for a finished one.
2. **Check the file.** When the download is finished, FFmpeg reads the whole file to make sure it
   is not damaged. This is the "Validating downloaded file..." step. It is usually much faster
   than the download.
3. **Rename it.** Only when the file is OK is `(part)` removed from the name.
4. **Save the subtitles**, if **Save Subtitles as Separate Files** is checked (see
   [Save Subtitles as Separate Files](#save-subtitles-as-separate-files)).

If the download or the check fails, it is tried again automatically (see
[Auto-Retry on Download Failure](#auto-retry-on-download-failure)). If the last try also fails,
FFmpeg Assistant shows what went wrong. For a damaged file, you are asked whether to delete it,
and then whether to try again.

Subtitle files (`.srt`) are not checked: they are renamed as soon as they are downloaded.

Also:

- If the file already exists, you are asked whether to overwrite it.
- The PC is kept from going to sleep during the download. The screen can still turn off.
- The taskbar button shows the progress, and flashes when the download is finished or has
  failed while you are working in another window.
- If you close FFmpeg Assistant during a download, you are asked first, and the partial file is
  deleted.
- If the power goes out or the PC crashes during a download, FFmpeg Assistant asks the next time
  it starts whether to start the download again (see [Download Queue](#download-queue)).

## Download Queue

You don't have to wait until a download is finished to start the next one. During a download,
fill in the next command, folder and file name, and click **Add to Queue**. The download waits in
the queue and starts when the downloads before it are finished. Add as many as you like: for
example all the episodes of a season. For TV shows, the next episode number is suggested each
time, counting the episodes in the queue.

FFmpeg Assistant checks each download when you add it, so the questions come while you are at the
PC: whether to create a missing folder, and whether to overwrite a file that already exists. The
same file can't be in the queue twice.

**Job**, at the right of **Status** and **Attempt**, shows which download is running, for example `2/5`. Click it, or choose
**Tools > Download Queue**, to see the queue in its own window. It shows the **Status**,
**File Name**, **Folder** and **Command** of each download, and follows the queue while it runs.
Select one or more downloads and click **Remove** (or press **Delete**) to take them out of the
queue. The running download can't be removed: click **Cancel** in the main window instead.

**Cancel** asks what to cancel when more downloads are waiting:

- **Yes**: cancel this download and remove the waiting ones from the queue.
- **No**: cancel only this download and continue with the next one.
- **Cancel**: continue downloading.

When a download in the queue fails, FFmpeg Assistant doesn't stop to show a message (you may be
away from the PC). It goes on with the next download. When the queue is finished, a message lists
the downloads that failed, and why. A damaged file is kept, so you can check it. Links from the
web often stop working after a few hours, so a download far back in a long queue may fail for
that reason. Copy a new command from Privatkopiera and add it again.

The [Action When Download Finished](#action-when-download-finished) (for example the sound)
happens once, when the whole queue is finished.

**Enable Watching While Downloading** is read when each download starts, and
**Save Subtitles as Separate Files** when each download is finished, so they apply to all the
downloads in the queue from then on.

### After a Power Outage

The queue is saved on the disk after every change. If the power goes out or the PC crashes during
a download, FFmpeg Assistant asks the next time it starts whether to start the download again.
It starts from the beginning, followed by the ones that were waiting. Click **No** to remove them
from the queue.

If you close FFmpeg Assistant while downloads are waiting, they are kept in the queue, and you are
asked the same question the next time it starts.

### More Than One Window

You can open FFmpeg Assistant more than once, but only one window at a time can use the queue.
In another window you can still download one file at a time, but not add to the queue, and that
download is not saved in case of a power outage.

## Validate Video File

**Tools > Validate Video File...** checks any video file on your computer with the same check
that runs after a download. Choose the file, and FFmpeg reads all of it. The file is not changed.

Status then shows "The video file is OK." or "The video file is corrupted or unreadable." Click
**Open Log File** to see what FFmpeg found, or **Open File** to play the file.

During the check, the **Command** box shows the command that runs, and **Folder** and
**File Name** show the file. Click **Cancel** to stop the check. To start a download afterwards,
paste a new command.

## Extract Subtitle File

**Subtitles > Extract Subtitle File...** saves a subtitle track that is stored inside a video file
as a separate file.

1. Choose the video file. The last downloaded video is suggested.
2. Choose one of the subtitle tracks in the list, for example `Stream #2 — SWE — [subrip]`, and
   click **OK**.
3. The **Command** and **File Name** boxes are filled in. Choose a **Folder** and click
   **Download**.

The subtitle file gets the right extension for its format, for example `.srt`, `.ass` or `.vtt`.

To download subtitles from the web instead, paste the Privatkopiera command or the address of
the subtitle playlist (`.m3u8`) into **Command**. FFmpeg Assistant recognises subtitle playlists
and saves them as `.srt` files.

## Settings

Open the settings with **Tools > Settings**. Click **OK** to save your changes, or **Cancel** to
close the window without saving.

### FFmpeg

**Path to ffmpeg.exe**: where FFmpeg is installed. Click **Browse...** to find `ffmpeg.exe`.
Leave it empty to use the FFmpeg on the system PATH.

### Auto-Retry on Download Failure

**Maximum Number of Attempts**: how many times a download is tried before FFmpeg Assistant gives
up. A download is tried again when FFmpeg fails, or when the downloaded file is damaged. The
default is 5. Leave it empty, or enter 0 or 1, to not try again.

### Action When Download Finished

**Action** decides what happens when a download is finished. For the
[Download Queue](#download-queue), it happens once, when the whole queue is finished:

- **Play a Sound**: plays the sound chosen under **Sound**. Click **▶** to listen to it. Choose
  **Custom Sound File...** at the end of the list to use your own WAV file, and select it under
  **Custom Sound File**.
- **Message Box**: shows the message "The download is complete." (or "All 5 downloads are
  complete." after a queue).
- **None**: does nothing. Status still shows "Done".

### Check for New Version

**Check for New Version at Startup**: when **Yes**, FFmpeg Assistant checks on GitHub whether a
new version is available each time it starts. If there is one, **New Version Available** appears
at the right end of the menu bar. Click it to open the download page.

### Extra Folders in the Folder List

Add the folders you often save to, for example `D:\Videos\Music Videos` or a folder on a network
drive. Type one folder per line, and press **Enter** to start a new line. When you click **OK**,
they are added to the **Folder** list in the main window, in the same order, after the
**TV Shows** folder.

Each line must be a full path that starts with a drive letter or `\\server`. Empty lines are
ignored. If a folder in the list does not exist when you download to it, you are asked whether
to create it.

## Create Shortcut

**Tools > Create Shortcut...** creates shortcuts to FFmpeg Assistant. Check where you want them:

- **Desktop**
- **Start Menu (Programs)**

Choose whether the shortcuts are for the **Current User Only** or for **All Users**. All users may
require administrator rights.

You can also start FFmpeg Assistant from the command line with a web address. It is put into
the **Command** box:

```
FFmpegAssistant.exe "https://example.com/video.m3u8"
```

## Keyboard Shortcuts

| Key | Action |
|-----|--------|
| **F1** | Open this help |
| **Ctrl+E** | Download, or Add to Queue during a download |
| **Ctrl+O** | Open File |
| **Ctrl+Shift+O** | Open Folder |
| **Alt+B** | Browse for the output folder |
| **F6** | Go to the **Command** box |

## Log Files

FFmpeg Assistant keeps its files in `%AppData%\SweWolfSoftware\FFmpegAssist`:

- `FFmpegAssistant.log`: a short line for every download: the command, the result, retries and
  so on. It is kept short automatically.
- `Logs\<file name>.txt`: FFmpeg's report of each download (the last attempt). Click
  **Open Log File** to open the latest one. The check of the file after the download is not
  included.
- `Logs\<file name> - validation.txt`: FFmpeg's report of each
  [Validate Video File](#validate-video-file) check.
- `Logs\errors.log`: a list of the downloads that failed.
- `Queue.json`: the downloads that are waiting in the [Download Queue](#download-queue). It is
  deleted when the queue is empty.

To open the folder, type `%AppData%\SweWolfSoftware\FFmpegAssist` in the address bar of File
Explorer.

## Troubleshooting

**"FFmpeg was not found on this system."**
FFmpeg Assistant can't find `ffmpeg.exe`. Click **Yes** to locate it: FFmpeg Assistant remembers
where it is and tries again. You can also set the path in **Tools > Settings**. See
[What You Need](#what-you-need).

**"FFmpeg exited with an error (code ...)"**
Something went wrong inside FFmpeg. The message shows the last error lines. Click
**Open Log File** for the whole report. Common causes are an expired link (copy a new command
from Privatkopiera) or a lost internet connection.

**"The downloaded file appears to be corrupted"**
The file was downloaded, but FFmpeg found errors in it, even after all attempts. Delete it and try
again, preferably with a new command from Privatkopiera.

**"... contains the character ..., which is not allowed in file and folder names."**
The folder or file name contains a character Windows doesn't allow, such as `?`, `*`, `:` or `|`.
Remove it and try again.

**"The Command box shows the command of the last validation."**
After [Validate Video File](#validate-video-file), the **Command** box shows the command of the
check. Paste a download command to start a download.

**"Could not probe the file"** or **"the subtitles could not be saved"**
[Extract Subtitle File](#extract-subtitle-file) and
[Save Subtitles as Separate Files](#save-subtitles-as-separate-files) need `ffprobe.exe`. Make sure it is in the same
folder as `ffmpeg.exe`, or on the system PATH.

**"No subtitle streams were found in the selected file."**
The video file doesn't contain any subtitles.

**"Another FFmpeg Assistant window is downloading a queue."**
Only one FFmpeg Assistant window can use the queue. Add the download in the window that is
downloading, or wait until the download in this window has finished (see
[More Than One Window](#more-than-one-window)).

**"The file ... is already in the download queue."**
A download with the same folder and file name is already waiting in the queue. Change the file
name (for example the episode number).

**The episode number is not suggested.**
FFmpeg Assistant only recognises episodes named like `My Show - s01e05.mp4`, with the same
extension as the new download. Check the names of the files in the show's folder, or enter
**Season** and **Episode** yourself.

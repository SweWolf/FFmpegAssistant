# FFmpeg Assistant: Future Features

Ideas that are not built yet. Nothing here is decided: discuss it before implementing.
Last updated 2026-11-13, after the 1.7.0 release.

## Right-click menu (main window text boxes and folder box)

The menu has Copy, Paste and Clear and Paste (`AttachTextBoxContextMenu` in `Form1.cs`).

- **Select All and Cut.** The standard Windows menu had them, and ours replaced it. Select All is
  the one most missed by mouse-only users. Small change to one function, plus a line in the help.
- **Items for specific boxes.** The same menu is used for every box. Ideas: "Paste and Download"
  in the Command box, "Clear" in the folder box. A new idea, not a fix.
- **Hint when the clipboard has no text.** Paste and Clear and Paste are greyed out when the
  clipboard holds a file or an image, with no explanation.
- Not checked yet: the menu at a high DPI setting, and the combo box's edit hook after the
  handle is re-created.

## Subtitles

- **SRT download ideas (#14):** detect a subtitle address on paste, use the same name as the last
  download with `.srt`, no language selection. Whether to add a third radio button was undecided;
  the Subtitle radio button in 1.7.0 covers part of this.

## Code

- **Nullable warnings:** 6 old compiler warnings (CS8622, CS8602, CS8600, CS8604) in `Form1.cs`.
  Clean up in a separate commit.

## Sibling apps (MP3toMP4, SplitMediaFiles, MergeMediaFiles)

Not done, and not to be done in the other repos unasked.

- Download queue with power outage recovery (postponed).
- `SleepBlocker` and `TaskbarFlash` helpers (keep the PC awake, flash the taskbar button).
- The FFmpeg call log menu: Split and Merge still lack it.
- The right-click text box menu from 1.7.0.

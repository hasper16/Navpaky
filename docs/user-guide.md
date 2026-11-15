# Rekey user guide

## The tray icon

Rekey lives in the notification area as a small key showing the language you type in now, such as "EN" or "RU". Windows 11 may hide it under the arrow; to keep it in sight, turn it on under Settings → Personalization → Taskbar → Other system tray icons.

- The key looks different when Rekey is not correcting: an amber edge while paused, a faded label while Auto Correction is off, and a red edge with a dot when Rekey paused itself after losing the keyboard. Point at the icon to read the state, including how many minutes of a pause remain.
- Click the icon with either button for the menu: turn Auto Correction on or off, pause it for 15 minutes, 30 minutes, 1 hour, 3 hours or until Rekey restarts (or resume), turn Rekey off or back on in the app you were just typing in, open Settings, Statistics or About, and quit.
- After Rekey switches the layout, a small key with the new language appears for a moment next to the text cursor, or next to the mouse pointer in apps that draw their own cursor. You can turn it off in Settings → General, and turn on a short sound after each correction there too.

## Settings

Settings opens from the tray menu; choosing it again brings the open window forward. Every change applies at once, with no Save button.

- **General**: start Rekey when you sign in to Windows (Task Manager → Startup apps can turn this off too, and Settings shows it), the cursor badge, and the correction sound with a choice of three sounds you can play.
- **Correction**: automatic correction, Sensitivity (Balanced, Cautious, Aggressive), the keys that end a word and trigger a correction (Space, Enter, Tab), leaving a word alone after you switch the layout yourself, and checking the text before replacing it.
- **Hotkey**: Double Shift, Double Ctrl, Double Alt, Ctrl+Space, Win+Space, or your own combination. When a combination is already Windows' layout shortcut or used by another app, Rekey says so and keeps the old Hotkey until you choose "Use it anyway" or pick another.
- **Apps and words**: apps where Rekey stays off (pick a running app or choose a program), and the Protected and Learned Words described below.
- **Diagnostics and privacy**: reconnecting to the keyboard when Windows stops sending keys, restarting Rekey when it stops responding twice, keeping statistics, the word-analysis log, and the buttons that show statistics and export diagnostics.

## Statistics and diagnostics

Rekey counts, per day and for the last 30 days, the words it analysed, its automatic and manual corrections, your undos (and what share of corrections they are), how each word was judged (corrected, left to wait for the next word, kept, kept as a Protected Word) and how long corrections took. Choose Statistics in the tray menu to see them. The counts never contain words; they are kept in `%LOCALAPPDATA%\Rekey\stats.json`, and turning statistics off in Settings → Diagnostics and privacy deletes them.

Rekey also keeps a log of the last 500 moments when it lost or regained the keyboard or stopped itself to stay safe (`incidents.jsonl`): times and codes only, never text or window titles.

For troubleshooting you can turn on the **word-analysis log**. It is off by default because it records the words you type, how they read in your other layouts and what Rekey decided. It stays on this PC in `%LOCALAPPDATA%\Rekey\analysis`, only your Windows account can open it, each day older than 7 days is deleted, and turning the log off deletes it at once. Rekey never sends it anywhere.

**Export diagnostics…** shows the exact text Rekey would export: its version, Windows version, layouts, settings, statistics and incidents, with your profile folder, user name and computer name replaced by placeholders. It contains no typed words, Protected Words, Learned Words or window titles unless you tick "Include word-analysis log". Save it to a file (the Desktop is suggested) or copy it; this button is the only time Rekey writes to the clipboard.

## Teaching Rekey

No word list covers every name, brand, slang word or abbreviation you type. Rekey learns from you in two ways and keeps what it learns in `%LOCALAPPDATA%\Rekey\words.json`, so it is still there after a restart.

### Protected Words: words Rekey leaves alone

A Protected Word is never corrected automatically. If Rekey corrects a word you meant to type as it was, undo the Correction (Ctrl+Z or the Hotkey right after it): Rekey restores your text and from then on leaves that word alone.

- Example: you type "ntcn" and Space in English (US) and Rekey turns it into "тест". You press Ctrl+Z. The next "ntcn" you type stays "ntcn".
- Words shorter than 3 letters are not protected this way, so one Undo of "yt" does not stop Rekey from correcting "yt" later.
- When several Words were corrected together, each of them with 3 letters or more becomes protected, and a group of Words that contains a Protected Word is not corrected.
- Protection is matched without regard to case or to punctuation around the word.
- The Hotkey still corrects a Protected Word: when you ask explicitly, Rekey does what you ask.

### Learned Words: words Rekey accepts

When you correct a word with the Hotkey and the result is not a word Rekey knows in that Language, Rekey learns it. From then on it treats the word as a common word of that Language and corrects it automatically like any other.

- Example: you type "rhby;" in English (US); Rekey does not know "кринж", so it leaves the word. You press the Hotkey and it becomes "кринж". The next time you type "rhby;" and Space, Rekey corrects it to "кринж" on its own.
- If Rekey learned a word by mistake, undo a Correction that typed it: the word is forgotten.
- Converting selected text with the Hotkey teaches Rekey nothing, since a selection can be a whole sentence.

### Managing the lists

Settings → Apps and words shows both lists, where you can add and remove words; a change takes effect for the next word you type. Each list holds up to 10,000 words. When a list is full, Rekey drops the word it learned on its own that was used least recently; words you added yourself are never dropped, and adding one more to a list full of your own words is refused.

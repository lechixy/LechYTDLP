---------------
# v3.0.0
This one is a tuff update because i need to change the way how we progress tools like: yt-dlp and ffmpeg. 
> 🎉 emoji ones are proud changes of mine (hard to make) <3

# Features
- **You can download 3 videos (you can change the number from Options) at once now (Concurrent Downloads) 🎉**
- **Now you don't need to wait for yt-dlp to fetch the video info, we fetch it in background and you can add new links while it's fetching 🎉**
- **Adding processing urls button to right top of main page (also you can cancel them, just click to the button and it will show a list of the urls being processed) 🎉**
- **Adding Pause/Resume or Cancel functionality to the downloads 🎉**
- **Add multiple downloads at once with separating links by space (link1 link2 link3)**
- Adding "Let yt-dlp decide" preset (might be useful for some users)
- "Enter" key support for Link Text Box (press Enter to start download)
- Adding CookieFileInWrongEncoding exception to the error handling (for users who are using cookies in wrong encoding)

# Refactors
- Refactoring how app handle processing: Created a new class ProcessBase, moved yt-dlp to a separate class for better maintainability and easier testing. Now you can easily add support for other tools like gallery-dl in the future.
- Processing tools are now singletons, so we don't need to create a new instance every time we want to use them. (YTDLP, GalleryDL)
- Moved types like RequestData, DownloadInfo, UpdateResult to a separate file for better maintainability and easier testing.
- Processes don't return only exit code but also the code, output and error messages, so we can use them for better error handling and logging.

# Fixes
- Fixing best video + best audio not working for some videos (added fallback /best)
- LECHYTDLP-X error from Sentry (because of wrong exception handling)
- LECHYTDLP-16 error from Sentry (because of wrong exception handling)
- LECHYTDLP-11 error from Sentry (because of PickFile Com exceptions)
- LECHYTDLP-K, LECHYTDLP-M error from Sentry (because of null passing to LogService.Add() - NullReferenceException)
- LECHYTDLP-12 error from Sentry (because of SQLite Error 13: 'database or disk is full')
- LECHYTDLP-E error from Sentry (because of UpdateLogBadge null reference)
- LECHYTDLP-F error from Sentry (because of GetFiles tries to get a directory that doesn't exist)
- LECHYTDLP-H error from Sentry (because of WinRT exceptions)
- LECHYTDLP-5 error from Sentry (because of it is being used by another process)
- LECHYTDLP-T error from Sentry (because of IOException)
- LECHYTDLP-G error from Sentry (because of IndexOutOfRangeException)

---------------
# v2.0.0
**Playlist support finally no more junk to download all that playlist. Just paste link select preset you want to use hit download 💖**

- Updated yt-dlp to stable@2026.07.04

# Features
- Adding Playlist support to the app (Download all videos from a playlist)
- Changing the width and height of the main window to be more compact and user-friendly (1000x800)
- Refactoring GetVideoInfo for Playlist support
- Adding enter animations to Select Format Dialog
- Adding new presets: 4k, 2k, 1080p, 720p, 480p, 360p

# Fixes
- Fixing the issue sometimes API not pasting the URL to the download textbox (because of threading issues)

**Full Changelog**: https://github.com/lechixy/LechYTDLP/compare/v1.6.5...v2.0.0

---------------
# v1.6.7
This build is most stable and has been tested for a long time. It is recommended for most users.

Thank you for using LechYTDLP <3

- Updated yt-dlp to stable@2026.06.09
# Features

- Added a new feature to check for updates of LechYTDLP (show a dialog when a new version is available)
- Adding new logo and banner to the project (Updated README.md with new branding)
- Adding Presets to Select Format Dialog as well
- Adding a new feature to allow users to select the download path for each download (via a folder picker dialog)
- Adding “Retry” button to failed downloads
- Adding "Remove" button to context menu
- Adding "Delete" option to context menu
- Adding "Copy Media" option to context menu
- Adding "Force Overwrites" to options page (File section)
- Adding "Custom YT-DLP Parameters" to options page (More section)
- Adding "Concurrent Fragments" option to options page (Downloads section)
- Adding Metadata's to history page to show more information about the downloaded media
- Now your download history will be saved in Documents/LechYTDLP/Database/history.db (so your history will not be lost when you uninstall the app)

# Fixes
- Filepath breaks with characters like: ş ü ğ or emojis (because of python not handling Unicode properly)
- Extension not pasting url to download textbox
- Select Format Dialog not showing when called from extension
- Fixed the sizing of downloaded items in the history page (not taking full width)

**Full Changelog**: https://github.com/lechixy/LechYTDLP/compare/v1.5.0...v1.6.7
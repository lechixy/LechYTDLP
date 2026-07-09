# Features

# Fixes

---------------
# v1.6.5
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
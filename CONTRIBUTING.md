# Contributing to LechYTDLP

Bug fixes, improvements, documentation, and translations are welcome. Please follow the [Code of Conduct](CODE_OF_CONDUCT.md).

## Reporting bugs and suggesting features

Search the [existing issues](https://github.com/lechixy/LechYTDLP/issues) before opening a new one. Pick the relevant [issue template](https://github.com/lechixy/LechYTDLP/issues/new/choose) and include enough context for someone else to understand the problem or reproduce the bug.

Use the [Security Policy](SECURITY.md) to report a suspected vulnerability.

## Running the app locally

1. On Windows, install Visual Studio with WinUI development tools and the .NET 8 SDK. Use a Visual Studio version that can open `.slnx` solutions.
2. Fork and clone the repository, then open [LechYTDLP.slnx](LechYTDLP.slnx) in Visual Studio.
3. Place `yt-dlp.exe` and `ffmpeg.exe` in `LechYTDLP/Tools/`. Use the [yt-dlp releases](https://github.com/yt-dlp/yt-dlp/releases) and a Windows build linked from the [FFmpeg download page](https://ffmpeg.org/download.html).
4. Create `LechYTDLP/appsettings.Local.json` with `{}` as its contents. The app requires this file at startup. Git ignores it and the `Tools` directory.
5. Select the platform that matches your machine, choose the **LechYTDLP (Unpackaged)** launch profile, and run the app with **F5**.

## Submitting a pull request

Create a branch from `main` and keep the changes focused. Before opening a pull request, build the solution and try the affected workflow in the app. Summarize what changed, link related issues, and describe how you verified it. Include screenshots for visible UI changes when they help with review.

## Translations

Strings are kept in `LechYTDLP/Strings/<locale>/Resources.resw`. Use [the English resources](LechYTDLP/Strings/en-US/Resources.resw) as a reference, preserve resource keys and format placeholders, and check the affected screens in the target language. When adding a language, add it to the `Languages` list in [SettingsService.cs](LechYTDLP/Services/SettingsService.cs) so users can select it in the app.

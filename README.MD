<p align="center">
  <img width="65%" src="https://raw.githubusercontent.com/lechixy/LechYTDLP/refs/heads/main/Assets/lechytdlp_banner.png">
</p>
<p align="center">
<strong>English</strong> • <a href="https://github.com/lechixy/LechYTDLP/blob/main/README.tr.MD">Türkçe</a>
</p>

---
Another **[YT-DLP](https://github.com/yt-dlp/yt-dlp)** Gui application for **Windows** but it built with **Windows 11 Style (Win UI 3)**

<a href="https://apps.microsoft.com/detail/9N28HRK3320G?referrer=appbadge&cid=GitHub_README&mode=direct">
	<img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/>
</a>
<img alt="AppShowcase" src="https://raw.githubusercontent.com/lechixy/LechYTDLP/refs/heads/main/Assets/lechytdlp_showcase.png" />

This is my first project with Win UI 3 trying to do my best 💖

<h3 align="center">
<a font-size="50" href="https://www.notion.so/lechixy/2dc9ce1d06e580d0824fe51030fe51f9?v=2dc9ce1d06e580f38b45000ca867db36&source=copy_link">🗺 Here is the roadmap and to-do list for this project 📌</a>
</h3>

## How to install 📥
### Which version should you choose? 
- **Microsoft Store version** provides automatic updates. **(Suggested)**
- **GitHub version** is free and open-source, but requires manual updates. Installing the GitHub version is a bit more complicated than the Microsoft Store version.

### Using Microsoft Store
<a href="https://apps.microsoft.com/detail/9N28HRK3320G?referrer=appbadge&cid=GitHub_README&mode=direct">
	<img src="https://get.microsoft.com/images/en-us%20dark.svg" width="300"/>
</a>

### Using .msixbundle installer
1. Go to the [latest release](https://github.com/lechixy/LechYTDLP/releases/latest) page
2. Download the **"*.cer"** file *(real certificates cost a lot of money)*
3. Open the certificate and press **"Install Certificate..."**
4. On the Certificate Import Wizard, select **"Local Machine"**, press **"Next"** and grant Admin Access
5. Select **"Place all certificates in the following store"**, then **"Browse..."**, choose **"Trusted Root Certification Authorities"** and **"OK"**
6. Finally, press **"Next"** and then **"Finish"**. It might ask you to confirm, press **Yes**
7. Download the **"*.msixbundle"** file (depending to your architecture: x64, x86, arm etc.)
8. The App Installer will pop up, press **"Install"**, or **"Update"** if you've installed Lech YT-DLP before

# Contributing
There is a Türkçe saying for this, Google it:
> _"Bir elin nesi var, iki elin sesi var."_

<p>Just open a pull request, your help is <strong>always welcomed (●'◡'●)</strong>💕</p>

<br>

# FAQ (Frequently Asked Questions)
I will try to answer some of the most common questions here. If you have any other questions, please feel free to ask.

## Why I need to pass my cookies here?
1. Some videos are age-restricted or region-restricted.
2. If you want to download videos from a private playlist or channel.
3. You can download a few videos without cookies, but after a while, you will hit download limits.
4. Some videos require authentication to access.
5. Some videos are only available to users with certain permissions (e.g., subscribers, members, etc.).

✨ **By passing your cookies, you can bypass these restrictions and download the videos you want.**

- But don't worry, your cookies are _**created by you and stored on your local machine**_
LechYTDLP is **only using your cookies to pass YT-DLP** but if you have trust issues. See yourself in the [source code.](https://github.com/search?q=repo%3Alechixy%2FLechYTDLP+Cookie&type=code)

## How to get my cookies?
In future, I will create a browser extension to make it easier for you to get your cookies.

But for now, you can check out following method to get your cookies:

[See YT-DLP official documentation (recommended)](https://github.com/yt-dlp/yt-dlp/wiki/FAQ#how-do-i-pass-cookies-to-yt-dlp)

## Why I am getting "No JavaScript runtime" warning?
It means that you haven't installed a JavaScript runtime on your machine.

To download from YouTube, yt-dlp needs to solve JavaScript challenges presented by YouTube using an external JavaScript runtime.

Sometimes formats can be extracted without a JavaScript runtime, but in some cases, especially when downloading from YouTube, a JavaScript runtime is required to solve the challenges and extract the video formats.

**💖 See this topic on [YT-DLP official documentation](https://github.com/yt-dlp/yt-dlp/wiki/EJS) because they have download links to Deno, Node, Bun or QuickJS.**

After you have installed one of the JavaScript runtimes open Lech YT-DLP and in app go to:

> _**Settings > Customize YT-DLP > JavaScript Runtime**_

Select the JavaScript runtime you have installed. After that, you should be able to download videos without any issues.

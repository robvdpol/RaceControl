# Race Control
[![GitHub issues](https://img.shields.io/github/issues/robvdpol/RaceControl)](https://github.com/robvdpol/RaceControl/issues?q=is%3Aopen+is%3Aissue)
[![GitHub closed issues](https://img.shields.io/github/issues-closed/robvdpol/RaceControl)](https://github.com/robvdpol/RaceControl/issues?q=is%3Aissue+is%3Aclosed)
[![GitHub All Releases](https://img.shields.io/github/downloads/robvdpol/RaceControl/total)](https://github.com/robvdpol/RaceControl/releases)
[![GitHub release (latest by date)](https://img.shields.io/github/v/release/robvdpol/RaceControl)](https://github.com/robvdpol/RaceControl/releases/latest)
[![GitHub](https://img.shields.io/github/license/robvdpol/RaceControl)](https://github.com/robvdpol/RaceControl/blob/master/LICENSE.md)

Race Control is an open source [F1TV](https://f1tv.formula1.com) desktop client for Windows. It can be used to watch F1TV content (both live streams and video on demand) in the highest quality available, using one of three supported video players. The goal of this project is to improve the overall F1TV experience by implementing features not found in the official F1TV website and apps, while still being easy to set up and use.

## Features
* Easy to set up and use, no editing of config files needed
* Fast and user-friendly user interface
* Watch F1TV live streams in the highest quality available
* Supports three different video players (internal player, VLC and MPV)
* Open as many streams as you like, using the layout that you like
* Save your custom video player layout, so you don't have to position windows manually every time you watch a session
* Cast to your Chromecast with a single click of a button
* Experimental synchronization of streams (replays only at the moment)

## Installation
#### Microsoft Store
* Go to the Microsoft Store and search for 'F1 Race Control' or use the [direct link](https://www.microsoft.com/en-us/p/f1-race-control/9mwc9pj4jfww).
* Install and launch the app.

#### Manual installation
* Download the [latest release](https://github.com/robvdpol/RaceControl/releases/latest) and start the installer. If a SmartScreen warning pops up, select 'More info' and click 'Run anyway'.
* If the [.NET 5.0 Desktop Runtime (x64)](https://dotnet.microsoft.com/download/dotnet-core/5.0/runtime) needs to be installed on your system you will be notified about it.
* Follow the steps in the setup wizard to complete the installation.
* Start Race Control using either the desktop shortcut or the start menu entry.

## Keyboard shortcuts
The internal player supports the following keyboard shortcuts:
| Function                               | Key                 |
|----------------------------------------|---------------------|
| Close player                           | Escape              |
| Close all players                      | Shift + Escape      |
| Toggle pause                           | Space               |
| Toggle pause for all players           | Shift + Space       |
| Toggle mute                            | M                   |
| Toggle fullscreen                      | F                   |
| Toggle fullscreen for specific player  | 1 - 9               |
| Move player to corner (3x3 layout)     | F1 - F9             |
| Synchronize session                    | S                   |
| Jump forward 10 sec                    | Right arrow         |
| Jump backward 10 sec                   | Left arrow          |
| Jump forward 1 min                     | Up arrow            |
| Jump backward 1 min                    | Down arrow          |
| Jump forward 5 sec                     | Shift + Right arrow |
| Jump backward 5 sec                    | Shift + Left arrow  |
| Jump forward 10 min                    | Shift + Up arrow    |
| Jump backward 10 min                   | Shift + Down arrow  |

The main window supports the following keyboard shortcuts:
| Function                       | Key                 |
|--------------------------------|---------------------|
| Save current layout            | Ctrl + S            |
| Open layout in internal player | Ctrl + I            |
| Open layout in MPV             | Ctrl + M            |

## FAQ
#### Why do I get a login popup? What credentials do I use to login?
You need an F1TV account to watch F1TV streams. If you want to watch race replays, an F1TV Access subscription is required. If you want to watch live sessions, an F1TV Pro subscription is required. You can create an account and subscribe at http://f1tv.formula1.com.

#### What happens with my credentials when I login?
Your F1TV login credentials are stored locally in the Windows Credential Manager, so you don't have to login every time you start the application. If you wish to switch to a different F1TV account, press the button 'Delete stored credentials' under the 'Options' section. Your stored credentials will be removed from your system and you will be asked to login again.

#### How can I move an internal player window?
You can move an internal player window by clicking and dragging on the actual video (not the menu bar!).

#### Where does Race Control store my settings and video layout? Will they be retained when I upgrade Race Control?
Your settings and video layout are stored in JSON-files, which are saved to *%LOCALAPPDATA%\RaceControl* (usually located at *C:\Users\USERNAME\AppData\Local\RaceControl*). When you upgrade Race Control your settings and video layout will be retained.

#### Why is the VLC button disabled?
You need to have VLC media player installed on your machine. The location of your VLC installation will be read from the Windows registry.

#### Do I need to install MPV separately to use it with Race Control?
No, Race Control comes with a recent version of MPV included in the release.

#### Can you add support for my favourite media player?
Maybe, please create an issue (feature request) and I will consider it. As a workaround, you can use the Copy-button to generate a tokenized link to the stream that is copied to your clipboard. Open the link with a media player of your choice to watch the stream. Note: the media player needs to properly support the HTTP Live Streaming (HLS) protocol.

## Contributing
To open and build the solution you need:
* Visual Studio 2019 (v16.8.0 or newer)
* [.NET 5.0 SDK](https://dotnet.microsoft.com/download/visual-studio-sdks)
* The extension [Microsoft Visual Studio Installer Projects](https://marketplace.visualstudio.com/items?itemName=VisualStudioClient.MicrosoftVisualStudio2017InstallerProjects) to open the RaceControl.Installer project

## Acknowledgements
* [VLC](https://www.videolan.org/vlc) for providing an excellent video player and .NET library
* [MPV](https://mpv.io) for providing an excellent video player
* [Flaticon](https://www.flaticon.com) for providing the application icon
* [Prism Library](https://prismlibrary.com)
* [Newtonsoft Json.NET](https://www.newtonsoft.com/json)
* [RestSharp](https://restsharp.dev)
* [NLog](https://nlog-project.org)
* [CredentialManagement.Standard NuGet package](https://www.nuget.org/packages/CredentialManagement.Standard)

## Screenshots
![preview image](https://imgur.com/Ngos9JZ.png)

![preview image](https://imgur.com/m46BzZj.png)

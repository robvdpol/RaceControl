# Race Control
Race Control is a standalone, open source [F1TV](https://f1tv.formula1.com) client for Windows. It can be used to watch F1TV content (both live streams and video on demand) in the highest quality available, using one of three supported video players. This project was started out of frustration over the mediocre official F1TV website and apps, and the lack of development of those platforms.

## Features
* Fast and user-friendly user interface
* Watch F1TV live streams in the highest quality available (currently 1080p)
* Supports three different video players (internal player, VLC and MPV)
* Open as many streams as you like, using the layout that you like
* Cast to your Chromecast with a single click of a button (internal player and VLC only)

## Installation
* Download the latest release and extract it to a location of your choice
* Make sure you have the .NET Core runtime (desktop) version 3.1 or above installed (https://dotnet.microsoft.com/download/dotnet-core/current/runtime)
* Run RaceControl.exe to start the application

## FAQ
#### Why do I get a login popup? What credentials do I use to login?
You need an F1TV account to watch F1TV VODs. If you want to watch race replays, an F1TV Access subscription is required. If you want to watch live sessions, an F1TV Pro subscription is required. You can create an account and subscribe at http://f1tv.formula1.com.

#### What happens with my credentials when I login?
You F1TV login credentials are stored locally in the Windows Credential Manager, so you don't have to login every time you start the application. If you wish to switch to a different F1TV account, open the Credential Manager, go to 'Windows Credentials' and under 'Generic Credentials' find the entry called 'RaceControlF1TV'. If you delete the entry, you will be asked to login again when you start the application.

#### Why is the VLC button disabled?
You need to have VLC media player installed on your machine. The location of your VLC installation will be detected automatically by Race Control.

## Acknowledgements
* [f1viewer](https://github.com/SoMuchForSubtlety/f1viewer) for showing how to use the F1TV API.
* [VLC](https://www.videolan.org/vlc) for providing an excellent video player and .NET library.
* [MPV](https://mpv.io) for providing a video player that can reliably play F1TV live streams without needing workarounds.
* [Streamlink](https://streamlink.github.io) for providing a flexible stream extractor/transporter.

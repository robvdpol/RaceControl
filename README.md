# Race Control
Race Control is a standalone, open source F1TV client for Windows.

## Features
* Fast and user-friendly user interface
* Watch F1TV streams in the highest quality available, either fullscreen or windowed
* No limit to the amount of streams that can be watched simultaneously
* Cast to your Chromecast with a single click of a button
* Experimental automatic synchronization of streams

## Installation
* Download the latest release and extract it to a location of your choice
* Make sure you have the .NET Core runtime (desktop) version 3.1 or above installed (https://dotnet.microsoft.com/download/dotnet-core/current/runtime)
* Run RaceControl.exe to start the application

## FAQ
#### Why do I get a login popup? What credentials do I use to login?
You need an F1TV account to watch F1TV VODs. If you want to watch race replays, an F1TV Access subscription is required. If you want to watch live sessions, an F1TV Pro subscription is required. You can create an account and subscribe at http://f1tv.formula1.com.

#### What happens with my credentials when I login?
You F1TV login credentials are stored locally in the Windows Credential Manager, so you don't have to login every time you start the application. If you wish to switch to a different F1TV account, open the Credential Manager, go to 'Windows Credentials' and under 'Generic Credentials' find the entry called 'RaceControlF1TV'. If you delete the entry, you will be asked to login again when you start the application.

## Acknowledgements
* [f1viewer](https://github.com/SoMuchForSubtlety/f1viewer) for showing how to use the F1TV API.
* [VLC](https://www.videolan.org/vlc) for providing an excellent video player and .NET library.
* [MPV](https://mpv.io) for providing a video player that can reliably play F1TV live streams without needing workarounds.
* [Streamlink](https://streamlink.github.io) for providing a flexible stream extractor/transporter.

using RaceControl.Common.Enums;
using RaceControl.Core.Settings;
using System;
using System.Collections.ObjectModel;

namespace RaceControl.Interfaces
{
    public interface IMediaPlayer : IDisposable
    {
        bool IsPlaying { get; }

        bool IsPaused { get; }

        long Time { get; set; }

        long Duration { get; }

        int MaxVolume { get; }

        int Volume { get; set; }

        bool IsMuted { get; }

        bool IsFullScreen { get; }

        int Zoom { get; set; }
        
        VideoQuality VideoQuality { get; set; }

        ObservableCollection<IAspectRatio> AspectRatios { get; }

        ObservableCollection<IAudioDevice> AudioDevices { get; }

        ObservableCollection<IMediaTrack> AudioTracks { get; }

        IAspectRatio AspectRatio { get; set; }

        IAudioDevice AudioDevice { get; set; }

        IMediaTrack AudioTrack { get; set; }

        void StartPlayback(string streamUrl, VideoDialogSettings settings);

        void ToggleFullScreen();

        void TogglePause();

        void ToggleMute(bool? mute);
    }
}
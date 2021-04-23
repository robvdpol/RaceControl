using RaceControl.Common.Enums;
using System;
using System.Collections.ObjectModel;

namespace RaceControl.Common.Interfaces
{
    public interface IMediaPlayer : IDisposable
    {
        int Volume { get; set; }

        long Time { get; set; }

        long Duration { get; }

        bool IsPlaying { get; }

        bool IsPaused { get; }

        bool IsMuted { get; }

        ObservableCollection<IAudioDevice> AudioDevices { get; }

        ObservableCollection<IMediaTrack> AudioTracks { get; }

        IAudioDevice AudioDevice { get; set; }

        IMediaTrack AudioTrack { get; set; }

        void StartPlayback(string streamUrl, string audioDevice, bool isMuted, int volume, VideoQuality videoQuality);

        void StopPlayback();

        void SetVideoQuality(VideoQuality videoQuality);

        void TogglePause();

        void ToggleMute(bool? mute);
    }
}
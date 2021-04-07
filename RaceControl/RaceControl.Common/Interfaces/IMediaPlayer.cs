using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RaceControl.Common.Interfaces
{
    public interface IMediaPlayer : IDisposable
    {
        long Time { get; set; }

        long Duration { get; }

        int Volume { get; set; }

        bool IsPaused { get; }

        bool IsMuted { get; }

        ObservableCollection<IAudioDevice> AudioDevices { get; }

        ObservableCollection<IMediaTrack> AudioTracks { get; }

        IAudioDevice AudioDevice { get; set; }

        IMediaTrack AudioTrack { get; set; }

        Task StartPlaybackAsync(string streamUrl);

        void StopPlayback();

        void TogglePause();

        void ToggleMute(bool? mute);
    }
}
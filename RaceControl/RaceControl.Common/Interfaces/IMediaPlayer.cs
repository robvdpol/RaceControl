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

        bool IsScanning { get; }

        bool IsCasting { get; }

        IAudioDevice AudioDevice { get; set; }

        ObservableCollection<IAudioDevice> AudioDevices { get; }

        ObservableCollection<IMediaTrack> AudioTracks { get; }

        ObservableCollection<IMediaRenderer> MediaRenderers { get; }

        Task StartPlaybackAsync(string streamUrl, IMediaRenderer mediaRenderer = null);

        void StopPlayback();

        void TogglePause();

        void ToggleMute(bool? mute);

        void SetAudioTrack(IMediaTrack audioTrack);

        Task ScanChromecastAsync();

        Task ChangeRendererAsync(IMediaRenderer mediaRenderer, string streamUrl = null);
    }
}
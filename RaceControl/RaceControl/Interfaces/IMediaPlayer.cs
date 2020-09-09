using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RaceControl.Interfaces
{
    public interface IMediaPlayer : IDisposable
    {
        long Time { get; set; }

        bool IsPaused { get; }

        bool IsMuted { get; }

        bool IsScanning { get; }

        bool IsCasting { get; }

        ObservableCollection<IMediaRenderer> MediaRenderers { get; }

        Task StartPlaybackAsync(string streamUrl, IMediaRenderer mediaRenderer = null);

        void TogglePause();

        void ToggleMute();

        void SetAudioTrack(int audioTrackId);

        Task ScanChromecastAsync();

        Task ChangeRendererAsync(IMediaRenderer mediaRenderer, string streamUrl);
    }
}
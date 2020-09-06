using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RaceControl.Interfaces
{
    public interface IMediaPlayer : IDisposable
    {
        MediaPlayer MediaPlayer { get; }

        long Time { get; set; }

        long Duration { get; }

        bool IsPaused { get; }

        bool IsMuted { get; }

        bool IsScanning { get; }

        bool IsCasting { get; }

        ObservableCollection<TrackDescription> AudioTrackDescriptions { get; }

        ObservableCollection<RendererItem> RendererItems { get; }

        Task StartPlaybackAsync(string streamUrl, RendererItem renderer = null);

        void StopPlayback();

        void TogglePause();

        void ToggleMute();

        void SetAudioTrack(int audioTrackId);

        Task ScanChromecastAsync();

        Task ChangeRendererAsync(RendererItem renderer, string streamUrl = null);
    }
}
using LibVLCSharp.Shared;
using System;
using System.Threading.Tasks;

namespace RaceControl.Interfaces
{
    public interface IMediaPlayer : IDisposable
    {
        long Time { get; set; }

        bool IsPaused { get; }

        bool IsScanning { get; }

        bool IsCasting { get; }

        Task StartPlaybackAsync(string streamUrl, bool isMuted, RendererItem renderer = null);

        void TogglePause();

        void ToggleMute();

        void SetAudioTrack(int audioTrackId);

        Task ScanChromecastAsync();

        Task ChangeRendererAsync(RendererItem renderer, string streamUrl = null);

        Action<bool> IsMutedChanged { get; set; }
    }
}
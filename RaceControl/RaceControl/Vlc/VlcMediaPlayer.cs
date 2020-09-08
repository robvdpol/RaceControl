using LibVLCSharp.Shared;
using LibVLCSharp.Shared.Structures;
using Prism.Mvvm;
using RaceControl.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RaceControl.Vlc
{
    public class VlcMediaPlayer : BindableBase, IMediaPlayer
    {
        private readonly LibVLC _libVLC;

        private string _streamUrl;
        private long _time;
        private long _duration;
        private bool _isPaused;
        private bool _isMuted;
        private bool _isScanning;
        private bool _isCasting;
        private ObservableCollection<TrackDescription> _audioTrackDescriptions;
        private ObservableCollection<RendererItem> _rendererItems;
        private bool _disposed;

        public VlcMediaPlayer(LibVLC libVLC, MediaPlayer mediaPlayer)
        {
            _libVLC = libVLC;
            MediaPlayer = mediaPlayer;
            MediaPlayer.Playing += MediaPlayer_Playing;
            MediaPlayer.Paused += MediaPlayer_Paused;
            MediaPlayer.Muted += MediaPlayer_Muted;
            MediaPlayer.Unmuted += MediaPlayer_Unmuted;
            MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            MediaPlayer.ESAdded += MediaPlayer_ESAdded;
            MediaPlayer.ESDeleted += MediaPlayer_ESDeleted;
        }

        public MediaPlayer MediaPlayer { get; }

        public long Time
        {
            get => _time;
            set
            {
                var time = Math.Max(value, 0);

                if (SetProperty(ref _time, time))
                {
                    MediaPlayer.Time = time;
                }
            }
        }

        public long Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        public bool IsPaused
        {
            get => _isPaused;
            set => SetProperty(ref _isPaused, value);
        }

        public bool IsMuted
        {
            get => _isMuted;
            set => SetProperty(ref _isMuted, value);
        }

        public bool IsScanning
        {
            get => _isScanning;
            set => SetProperty(ref _isScanning, value);
        }

        public bool IsCasting
        {
            get => _isCasting;
            set => SetProperty(ref _isCasting, value);
        }

        public ObservableCollection<TrackDescription> AudioTrackDescriptions
        {
            get => _audioTrackDescriptions ??= new ObservableCollection<TrackDescription>();
            set => SetProperty(ref _audioTrackDescriptions, value);
        }

        public ObservableCollection<RendererItem> RendererItems
        {
            get => _rendererItems ??= new ObservableCollection<RendererItem>();
            set => SetProperty(ref _rendererItems, value);
        }

        public async Task StartPlaybackAsync(string streamUrl, RendererItem renderer = null)
        {
            _streamUrl = streamUrl;
            AudioTrackDescriptions.Clear();
            MediaPlayer.SetRenderer(renderer);
            var media = new Media(_libVLC, _streamUrl, FromType.FromLocation);
            media.DurationChanged += Media_DurationChanged;
            await media.Parse();

            if (MediaPlayer.Play(media))
            {
                IsCasting = renderer != null;
            }
        }

        public void StopPlayback()
        {
            MediaPlayer.Stop();

            if (MediaPlayer.Media != null)
            {
                MediaPlayer.Media.Dispose();
                MediaPlayer.Media = null;
            }

            AudioTrackDescriptions.Clear();
        }

        public void TogglePause()
        {
            if (MediaPlayer.CanPause)
            {
                MediaPlayer.Pause();
            }
        }

        public void ToggleMute()
        {
            MediaPlayer.ToggleMute();
        }

        public void SetAudioTrack(int audioTrackId)
        {
            MediaPlayer.SetAudioTrack(audioTrackId);
        }

        public async Task ScanChromecastAsync()
        {
            RendererItems.Clear();

            using var rendererDiscoverer = new RendererDiscoverer(_libVLC);
            rendererDiscoverer.ItemAdded += RendererDiscoverer_ItemAdded;
            rendererDiscoverer.ItemDeleted += RendererDiscoverer_ItemDeleted;

            if (rendererDiscoverer.Start())
            {
                IsScanning = true;
                await Task.Delay(TimeSpan.FromSeconds(10));
                rendererDiscoverer.Stop();
                IsScanning = false;
            }

            rendererDiscoverer.ItemAdded -= RendererDiscoverer_ItemAdded;
            rendererDiscoverer.ItemDeleted -= RendererDiscoverer_ItemDeleted;
        }

        public async Task ChangeRendererAsync(RendererItem renderer, string streamUrl = null)
        {
            StopPlayback();
            await StartPlaybackAsync(streamUrl ?? _streamUrl, renderer);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                MediaPlayer.Media?.Dispose();
                MediaPlayer.Dispose();
            }

            _disposed = true;
        }

        private void MediaPlayer_Playing(object sender, EventArgs e)
        {
            IsPaused = false;
        }

        private void MediaPlayer_Paused(object sender, EventArgs e)
        {
            IsPaused = true;
        }

        private void MediaPlayer_Unmuted(object sender, EventArgs e)
        {
            IsMuted = false;
        }

        private void MediaPlayer_Muted(object sender, EventArgs e)
        {
            IsMuted = true;
        }

        private void MediaPlayer_TimeChanged(object sender, MediaPlayerTimeChangedEventArgs e)
        {
            SetProperty(ref _time, e.Time, nameof(Time));
        }

        private void MediaPlayer_ESAdded(object sender, MediaPlayerESAddedEventArgs e)
        {
            if (e.Id >= 0)
            {
                switch (e.Type)
                {
                    case TrackType.Audio:
                        var audioTrackDescription = MediaPlayer.AudioTrackDescription.First(p => p.Id == e.Id);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            AudioTrackDescriptions.Add(audioTrackDescription);
                        });
                        break;
                }
            }
        }

        private void MediaPlayer_ESDeleted(object sender, MediaPlayerESDeletedEventArgs e)
        {
            if (e.Id >= 0)
            {
                switch (e.Type)
                {
                    case TrackType.Audio:
                        var audioTrackDescription = AudioTrackDescriptions.First(p => p.Id == e.Id);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            AudioTrackDescriptions.Remove(audioTrackDescription);
                        });
                        break;
                }
            }
        }

        private void Media_DurationChanged(object sender, MediaDurationChangedEventArgs e)
        {
            Duration = e.Duration;
        }

        private void RendererDiscoverer_ItemAdded(object sender, RendererDiscovererItemAddedEventArgs e)
        {
            if (e.RendererItem.CanRenderVideo)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RendererItems.Add(e.RendererItem);
                });
            }
        }

        private void RendererDiscoverer_ItemDeleted(object sender, RendererDiscovererItemDeletedEventArgs e)
        {
            if (e.RendererItem.CanRenderVideo)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    RendererItems.Remove(e.RendererItem);
                });
            }
        }
    }
}
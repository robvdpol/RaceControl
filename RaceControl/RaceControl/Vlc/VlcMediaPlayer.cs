using LibVLCSharp.Shared;
using Prism.Mvvm;
using RaceControl.Common.Interfaces;
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
        private int _volume;
        private bool _isPaused;
        private bool _isMuted;
        private bool _isScanning;
        private bool _isCasting;
        private ObservableCollection<IMediaTrack> _audioTracks;
        private ObservableCollection<IMediaRenderer> _mediaRenderers;
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
            MediaPlayer.VolumeChanged += MediaPlayer_VolumeChanged;
            MediaPlayer.ESAdded += MediaPlayer_ESAdded;
            MediaPlayer.ESDeleted += MediaPlayer_ESDeleted;
        }

        public MediaPlayer MediaPlayer { get; }

        public long Time
        {
            get => _time;
            set
            {
                if (SetProperty(ref _time, Math.Max(value, 0)))
                {
                    MediaPlayer.Time = _time;
                }
            }
        }

        public long Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        public int Volume
        {
            get => _volume;
            set
            {
                if (SetProperty(ref _volume, Math.Min(Math.Max(value, 0), 100)))
                {
                    MediaPlayer.Volume = _volume;
                }
            }
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

        public ObservableCollection<IMediaTrack> AudioTracks
        {
            get => _audioTracks ??= new ObservableCollection<IMediaTrack>();
            set => SetProperty(ref _audioTracks, value);
        }

        public ObservableCollection<IMediaRenderer> MediaRenderers
        {
            get => _mediaRenderers ??= new ObservableCollection<IMediaRenderer>();
            set => SetProperty(ref _mediaRenderers, value);
        }

        public async Task StartPlaybackAsync(string streamUrl, IMediaRenderer mediaRenderer = null)
        {
            _streamUrl = streamUrl;

            using var media = new Media(_libVLC, _streamUrl, FromType.FromLocation);
            media.DurationChanged += (sender, e) => Duration = e.Duration;
            await media.Parse(MediaParseOptions.ParseNetwork | MediaParseOptions.FetchNetwork);
            MediaPlayer.SetRenderer(mediaRenderer?.Renderer as RendererItem);

            if (MediaPlayer.Play(media))
            {
                IsCasting = mediaRenderer != null;
            }
            else
            {
                IsCasting = false;
            }
        }

        public void StopPlayback()
        {
            MediaPlayer.Stop();
            AudioTracks.Clear();
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

        public void SetAudioTrack(IMediaTrack audioTrack)
        {
            MediaPlayer.SetAudioTrack(audioTrack.Id);
        }

        public async Task ScanChromecastAsync()
        {
            MediaRenderers.Clear();

            using var rendererDiscoverer = new RendererDiscoverer(_libVLC);
            rendererDiscoverer.ItemAdded += RendererDiscoverer_ItemAdded;

            if (rendererDiscoverer.Start())
            {
                IsScanning = true;
                await Task.Delay(TimeSpan.FromSeconds(10));
                IsScanning = false;
                rendererDiscoverer.Stop();
            }

            rendererDiscoverer.ItemAdded -= RendererDiscoverer_ItemAdded;
        }

        public async Task ChangeRendererAsync(IMediaRenderer mediaRenderer, string streamUrl = null)
        {
            StopPlayback();
            await StartPlaybackAsync(streamUrl ?? _streamUrl, mediaRenderer);
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

        private void MediaPlayer_VolumeChanged(object sender, MediaPlayerVolumeChangedEventArgs e)
        {
            SetProperty(ref _volume, Convert.ToInt32(e.Volume * 100), nameof(Volume));
        }

        private void MediaPlayer_ESAdded(object sender, MediaPlayerESAddedEventArgs e)
        {
            if (e.Id >= 0)
            {
                switch (e.Type)
                {
                    case TrackType.Audio:
                        var trackDescription = MediaPlayer.AudioTrackDescription.First(td => td.Id == e.Id);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            AudioTracks.Add(new VlcMediaTrack(trackDescription));
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
                        var audioTrack = AudioTracks.FirstOrDefault(at => at.Id == e.Id);

                        if (audioTrack != null)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                AudioTracks.Remove(audioTrack);
                            });
                        }

                        break;
                }
            }
        }

        private void RendererDiscoverer_ItemAdded(object sender, RendererDiscovererItemAddedEventArgs e)
        {
            if (e.RendererItem.CanRenderVideo)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MediaRenderers.Add(new VlcMediaRenderer(e.RendererItem));
                });
            }
        }
    }
}
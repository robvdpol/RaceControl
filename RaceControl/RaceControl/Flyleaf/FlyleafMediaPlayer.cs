using FlyleafLib;
using FlyleafLib.MediaPlayer;
using NLog;
using Prism.Mvvm;
using RaceControl.Common.Constants;
using RaceControl.Common.Enums;
using RaceControl.Core.Settings;
using RaceControl.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using FlyleafLibAspectRatio = FlyleafLib.AspectRatio;

namespace RaceControl.Flyleaf
{
    public class FlyleafMediaPlayer : BindableBase, IMediaPlayer
    {
        private readonly ILogger _logger;

        private bool _isStarted;
        private bool _isPlaying;
        private bool _isPaused;
        private long _time;
        private long _duration;
        private int _volume;
        private bool _isMuted;
        private bool _isFullScreen;
        private int _zoom;
        private VideoQuality _videoQuality;
        private ObservableCollection<IAspectRatio> _aspectRatios;
        private ObservableCollection<IAudioDevice> _audioDevices;
        private ObservableCollection<IMediaTrack> _audioTracks;
        private IAspectRatio _aspectRatio;
        private IAudioDevice _audioDevice;
        private IMediaTrack _audioTrack;
        private bool _videoInitialized;
        private bool _audioInitialized;
        private bool _disposed;

        public FlyleafMediaPlayer(ILogger logger, Player player)
        {
            _logger = logger;
            Player = player;
        }

        public Player Player { get; }

        public bool IsStarted
        {
            get => _isStarted;
            private set => SetProperty(ref _isStarted, value);
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            private set => SetProperty(ref _isPlaying, value);
        }

        public bool IsPaused
        {
            get => _isPaused;
            private set => SetProperty(ref _isPaused, value);
        }

        public long Time
        {
            get => _time;
            set
            {
                var time = TimeSpan.FromTicks(Math.Max(value, 0));
                Player.Seek((int)time.TotalMilliseconds);
            }
        }

        public long Duration
        {
            get => _duration;
            private set => SetProperty(ref _duration, value);
        }

        public int MaxVolume => 150;

        public int Volume
        {
            get => _volume;
            set
            {
                var volume = Math.Min(Math.Max(value, 0), MaxVolume);

                if (SetProperty(ref _volume, volume))
                {
                    IsMuted = false;
                    Player.audioPlayer.Volume = _volume;
                }
            }
        }

        public bool IsMuted
        {
            get => _isMuted;
            private set
            {
                if (SetProperty(ref _isMuted, value))
                {
                    Player.audioPlayer.Mute = _isMuted;
                }
            }
        }

        public bool IsFullScreen
        {
            get => _isFullScreen;
            private set => SetProperty(ref _isFullScreen, value);
        }

        public int Zoom
        {
            get => _zoom;
            set
            {
                var zoom = Math.Min(Math.Max(value, -250), 250);

                if (SetProperty(ref _zoom, zoom))
                {
                    Player.renderer.Zoom = _zoom;
                }
            }
        }

        public VideoQuality VideoQuality
        {
            get => _videoQuality;
            set
            {
                if (SetProperty(ref _videoQuality, value))
                {
                    SetVideoQuality(_videoQuality);
                }
            }
        }

        public ObservableCollection<IAspectRatio> AspectRatios => _aspectRatios ??= new ObservableCollection<IAspectRatio>();

        public ObservableCollection<IAudioDevice> AudioDevices => _audioDevices ??= new ObservableCollection<IAudioDevice>();

        public ObservableCollection<IMediaTrack> AudioTracks => _audioTracks ??= new ObservableCollection<IMediaTrack>();

        public IAspectRatio AspectRatio
        {
            get => _aspectRatio;
            set
            {
                if (SetProperty(ref _aspectRatio, value) && _aspectRatio != null)
                {
                    Player.Config.video.AspectRatio = new FlyleafLibAspectRatio(_aspectRatio.Value);
                }
            }
        }

        public IAudioDevice AudioDevice
        {
            get => _audioDevice;
            set
            {
                if (SetProperty(ref _audioDevice, value) && _audioDevice != null)
                {
                    Player.audioPlayer.Device = _audioDevice.Identifier;
                }
            }
        }

        public IMediaTrack AudioTrack
        {
            get => _audioTrack;
            set
            {
                if (SetProperty(ref _audioTrack, value) && _audioTrack != null)
                {
                    var audioStream = Player.curAudioPlugin.AudioStreams.FirstOrDefault(stream => new FlyleafAudioTrack(stream).Id == _audioTrack.Id);

                    if (audioStream != null)
                    {
                        Player.Open(audioStream);
                    }
                }
            }
        }

        public void StartPlayback(string streamUrl, VideoDialogSettings settings)
        {
            Player.OpenCompleted += (_, args) =>
            {
                if (args.success)
                {
                    PlayerOnOpenCompleted(args.type, settings);
                }
            };

            Player.PropertyChanged += PlayerOnPropertyChanged;

            if (settings.FullScreen)
            {
                ToggleFullScreen();
            }

            Player.Open(streamUrl);
        }

        public void ToggleFullScreen()
        {
            // Needed to prevent triggering another toggle from window state changed event
            IsFullScreen = !IsFullScreen;
            IsFullScreen = !IsFullScreen ? !Player.VideoView.NormalScreen() : Player.VideoView.FullScreen();
        }

        public void TogglePause()
        {
            if (IsPlaying)
            {
                Player.Pause();
            }
            else
            {
                Player.Play();
            }
        }

        public void ToggleMute(bool? mute)
        {
            IsMuted = mute.GetValueOrDefault(!IsMuted);
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
                // Prevent main application from hanging after closing an internal player
                Task.Run(() =>
                {
                    try
                    {
                        Player.Dispose();
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(ex, "A non-critical error occurred.");
                    }
                });
            }

            _disposed = true;
        }

        private void PlayerOnOpenCompleted(MediaType mediaType, VideoDialogSettings settings)
        {
            switch (mediaType)
            {
                case MediaType.Video:
                    if (!_videoInitialized)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            InitializeVideo(settings.VideoQuality, settings.Zoom, settings.AspectRatio);
                        });

                        _videoInitialized = true;
                    }

                    Player.Play();
                    break;

                case MediaType.Audio:
                    if (!_audioInitialized)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            InitializeAudio(settings.AudioDevice, settings.AudioTrack, settings.IsMuted, settings.Volume);
                        });

                        _audioInitialized = true;
                    }

                    break;
            }
        }

        private void PlayerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Player.Status))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsPlaying = Player.Status == Status.Playing;
                    IsPaused = Player.Status == Status.Paused;

                    if (IsPlaying)
                    {
                        IsStarted = true;
                    }
                });
            }
        }

        private void SessionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Player.Session.CurTime))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SetProperty(ref _time, Player.Session.CurTime, nameof(Time));
                });
            }
        }

        private void InitializeVideo(VideoQuality videoQuality, int zoom, string aspectRatio)
        {
            Player.Session.PropertyChanged += SessionOnPropertyChanged;
            AspectRatios.AddRange(FlyleafLibAspectRatio.AspectRatios.Where(ar => ar != FlyleafLibAspectRatio.Custom).Select(ar => new FlyleafAspectRatio(ar)));
            Duration = Player.Session.Movie.Duration;
            VideoQuality = videoQuality;

            if (!string.IsNullOrWhiteSpace(aspectRatio))
            {
                AspectRatio = AspectRatios.FirstOrDefault(ar => ar.Value == aspectRatio);
            }
            else
            {
                var ratio = AspectRatios.FirstOrDefault(ar => ar.Value == FlyleafLibAspectRatio.Keep.ValueStr);
                SetProperty(ref _aspectRatio, ratio, nameof(AspectRatio));
            }

            Zoom = zoom;
        }

        private void InitializeAudio(string audioDevice, string audioTrack, bool isMuted, int volume)
        {
            AudioDevices.AddRange(Master.AudioMaster.Devices.Select(device => new FlyleafAudioDevice(device)));
            AudioTracks.AddRange(Player.curAudioPlugin.AudioStreams.Select(stream => new FlyleafAudioTrack(stream)));
            Volume = volume;
            ToggleMute(isMuted);

            if (!string.IsNullOrWhiteSpace(audioDevice))
            {
                AudioDevice = AudioDevices.FirstOrDefault(d => d.Identifier == audioDevice);
            }
            else
            {
                var device = AudioDevices.FirstOrDefault(d => d.Identifier == Player.audioPlayer.Device);
                SetProperty(ref _audioDevice, device, nameof(AudioDevice));
            }

            var flyleafCode = LanguageCodes.GetFlyleafCode(audioTrack);

            AudioTrack = AudioTracks.FirstOrDefault(t => t.Id == flyleafCode) ??
                         AudioTracks.FirstOrDefault(t => t.Id == LanguageCodes.English) ??
                         AudioTracks.FirstOrDefault(t => t.Id == LanguageCodes.Undetermined);
        }

        private void SetVideoQuality(VideoQuality videoQuality)
        {
            if (Player.curVideoPlugin == null || !Player.curVideoPlugin.VideoStreams.Any())
            {
                return;
            }

            var maxHeight = Player.curVideoPlugin.VideoStreams.Max(stream => stream.Height);
            var minHeight = maxHeight;

            switch (videoQuality)
            {
                case VideoQuality.Medium:
                    minHeight = maxHeight / 3 * 2;
                    break;

                case VideoQuality.Low:
                    minHeight = maxHeight / 2;
                    break;

                case VideoQuality.Lowest:
                    minHeight = maxHeight / 3;
                    break;
            }

            var videoStream = Player.curVideoPlugin.VideoStreams.OrderBy(stream => stream.Height).FirstOrDefault(stream => stream.Height >= minHeight);

            if (videoStream != null)
            {
                Player.Open(videoStream);
            }
        }
    }
}
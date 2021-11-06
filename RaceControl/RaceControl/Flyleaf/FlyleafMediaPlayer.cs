using FlyleafLib;
using FlyleafLib.MediaPlayer;
using NLog;
using Prism.Mvvm;
using RaceControl.Common.Constants;
using RaceControl.Common.Enums;
using RaceControl.Core.Settings;
using RaceControl.Interfaces;
using RaceControl.Services.Interfaces.F1TV.Api;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using FlyleafLibAspectRatio = FlyleafLib.AspectRatio;

namespace RaceControl.Flyleaf
{
    public class FlyleafMediaPlayer : BindableBase, IMediaPlayer
    {
        private readonly ILogger _logger;

        private bool _resyncVideo;
        private bool _resyncAudio;
        private bool _isStarting;
        private bool _isStarted;
        private bool _isPlaying;
        private bool _isPaused;
        private bool _isRecording;
        private int _volume = -1;
        private bool _isMuted;
        private int _zoom;
        private VideoQuality _videoQuality = VideoQuality.Default;
        private ObservableCollection<IAspectRatio> _aspectRatios;
        private ObservableCollection<IAudioDevice> _audioDevices;
        private ObservableCollection<IMediaTrack> _audioTracks;
        private IAspectRatio _aspectRatio;
        private IAudioDevice _audioDevice;
        private IMediaTrack _audioTrack;

        public FlyleafMediaPlayer(ILogger logger, Player player)
        {
            _logger = logger;
            Player = player;
        }

        public Player Player { get; }

        public bool IsStarting
        {
            get => _isStarting;
            private set => SetProperty(ref _isStarting, value);
        }

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

        public bool IsRecording
        {
            get => _isRecording;
            set => SetProperty(ref _isRecording, value);
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
                    Player.Volume = _volume;
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
                    Player.Mute = _isMuted;
                }
            }
        }

        public int Zoom
        {
            get => _zoom;
            set
            {
                var zoom = Math.Min(Math.Max(value, -250), 250);

                if (SetProperty(ref _zoom, zoom))
                {
                    Player.Zoom = _zoom;
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
                    OpenVideoStream(_videoQuality, _resyncVideo);
                    _resyncVideo = true;
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
                    Player.Config.Video.AspectRatio = new FlyleafLibAspectRatio(_aspectRatio.Value);
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
                    Player.AudioDevice = _audioDevice.Identifier;
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
                    OpenAudioStream(_audioTrack, _resyncAudio);
                    _resyncAudio = true;
                }
            }
        }

        public void StartPlayback(string streamUrl, PlayToken playToken, VideoDialogSettings settings)
        {
            IsStarting = true;

            Player.OpenCompleted += (_, args) =>
            {
                if (args.Success)
                {
                    PlayerOnOpenCompleted(settings);
                }
            };

            Player.OpenStreamCompleted += (_, args) =>
            {
                if (args.Success)
                {
                    PlayerOnOpenStreamCompleted();
                }
            };

            Player.PropertyChanged += PlayerOnPropertyChanged;

            if (playToken != null)
            {
                Player.Config.Demuxer.FormatOpt.Add("headers", playToken.GetCookieString());
            }

            Player.OpenAsync(streamUrl, true, false, false, false);
        }

        public void StartRecording(string filename)
        {
            if (!Player.CanPlay)
            {
                return;
            }

            if (!Player.IsRecording)
            {
                Player.StartRecording(ref filename);
                IsRecording = Player.IsRecording;
            }
        }

        public void StopRecording()
        {
            if (Player.IsRecording)
            {
                Player.StopRecording();
                IsRecording = Player.IsRecording;
            }
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

        public long GetCurrentTime()
        {
            return Player.CurTime;
        }

        public void SetCurrentTime(long time)
        {
            Player.CurTime = time;
        }

        public void FastForward(long seconds)
        {
            Player.CurTime += TimeSpan.FromSeconds(seconds).Ticks;
        }

        private void PlayerOnOpenCompleted(VideoDialogSettings settings)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    InitializeVideo(settings);
                    InitializeAudio(settings);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "An error occurred while initializing audio or video.");
                }
            });
        }

        private void PlayerOnOpenStreamCompleted()
        {
            try
            {
                Player.Play();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "An error occurred while playing stream.");
            }
        }

        private void PlayerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Player.Status))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    IsStarting = Player.Status == Status.Opening;
                    IsPlaying = Player.Status == Status.Playing;
                    IsPaused = Player.Status == Status.Paused;

                    if (IsPlaying)
                    {
                        IsStarted = true;
                    }
                });
            }
        }

        private void InitializeVideo(VideoDialogSettings settings)
        {
            AspectRatios.AddRange(FlyleafLibAspectRatio.AspectRatios.Where(ar => ar != FlyleafLibAspectRatio.Custom).Select(ar => new FlyleafAspectRatio(ar)));

            if (!string.IsNullOrWhiteSpace(settings.AspectRatio))
            {
                AspectRatio = AspectRatios.FirstOrDefault(ar => ar.Value == settings.AspectRatio);
            }
            else
            {
                var ratio = AspectRatios.FirstOrDefault(ar => ar.Value == FlyleafLibAspectRatio.Keep.ValueStr);
                SetProperty(ref _aspectRatio, ratio, nameof(AspectRatio));
            }

            Zoom = settings.Zoom;
            VideoQuality = settings.VideoQuality;
        }

        private void InitializeAudio(VideoDialogSettings settings)
        {
            AudioDevices.AddRange(Master.AudioMaster.Devices.Select(device => new FlyleafAudioDevice(device)));
            AudioTracks.AddRange(Player.Audio.Streams.Select(stream => new FlyleafAudioTrack(stream)).OrderBy(t => t.Name));
            Volume = settings.Volume;
            ToggleMute(settings.IsMuted);

            if (!string.IsNullOrWhiteSpace(settings.AudioDevice))
            {
                AudioDevice = AudioDevices.FirstOrDefault(d => d.Identifier == settings.AudioDevice);
            }
            else
            {
                var device = AudioDevices.FirstOrDefault(d => d.Identifier == Player.AudioDevice);
                SetProperty(ref _audioDevice, device, nameof(AudioDevice));
            }

            var flyleafCode = LanguageCodes.GetFlyleafCode(settings.AudioTrack);

            AudioTrack = AudioTracks.FirstOrDefault(t => t.Id == flyleafCode) ??
                         AudioTracks.FirstOrDefault(t => t.Id == LanguageCodes.English) ??
                         AudioTracks.FirstOrDefault(t => t.Id == LanguageCodes.Undetermined);
        }

        private void OpenVideoStream(VideoQuality videoQuality, bool resync)
        {
            var videoStream = Player.Video?.Streams.GetVideoStreamForQuality(videoQuality);

            if (videoStream != null)
            {
                Player.OpenAsync(videoStream, resync, false);
            }
        }

        private void OpenAudioStream(IMediaTrack audioTrack, bool resync)
        {
            var audioStream = Player.Audio?.Streams.FirstOrDefault(stream => new FlyleafAudioTrack(stream).Id == audioTrack.Id);

            if (audioStream != null)
            {
                Player.OpenAsync(audioStream, resync, false);
            }
        }
    }
}
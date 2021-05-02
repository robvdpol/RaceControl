﻿using FlyleafLib;
using FlyleafLib.MediaPlayer;
using NLog;
using Prism.Mvvm;
using RaceControl.Common.Enums;
using RaceControl.Core.Settings;
using RaceControl.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RaceControl.Flyleaf
{
    public class FlyleafMediaPlayer : BindableBase, IMediaPlayer
    {
        private readonly ILogger _logger;

        private long _time;
        private long _duration;
        private int _volume;
        private int _zoom;
        private bool _isFullScreen;
        private bool _isPlaying;
        private bool _isPaused;
        private bool _isMuted;
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
        private EventHandler<Player.OpenCompletedArgs> _openCompletedEventHandler;

        public FlyleafMediaPlayer(ILogger logger, Player player)
        {
            _logger = logger;

            Player = player;
            Player.PropertyChanged += PlayerOnPropertyChanged;
        }

        public Player Player { get; }

        public bool IsMuted => _isMuted;

        public int MaxVolume => 150;

        public int Volume
        {
            get => _volume;
            set => Player.audioPlayer.Volume = Math.Min(Math.Max(value, 0), MaxVolume);
        }

        public int Zoom
        {
            get => _zoom;
            set
            {
                if (SetProperty(ref _zoom, value))
                {
                    Player.renderer.Zoom = _zoom;
                }
            }
        }

        public long Time
        {
            get => _time;
            set => Player.Session.CurTime = Math.Max(value, 0);
        }

        public long Duration
        {
            get => _duration;
            private set => SetProperty(ref _duration, value);
        }

        public bool IsFullScreen
        {
            get => _isFullScreen;
            private set => SetProperty(ref _isFullScreen, value);
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
                    Player.Config.video.AspectRatio = new AspectRatio(_aspectRatio.Value);
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
                if (value != null)
                {
                    var audioStream = Player.curAudioPlugin.AudioStreams.FirstOrDefault(stream => new FlyleafAudioTrack(stream).Id == value.Id);

                    if (audioStream != null)
                    {
                        Player.Open(audioStream);
                    }
                }
                else
                {
                    SetProperty(ref _audioTrack, null);
                }
            }
        }

        public void StartPlayback(string streamUrl, VideoDialogSettings settings)
        {
            // Unsubscribe the previous event, if any.
            if (_openCompletedEventHandler != null)
            {
                Player.OpenCompleted -= _openCompletedEventHandler;
            }

            // Create and remember a new event handler instance with the given settings instance.
            _openCompletedEventHandler = (_, args) =>
            {
                if (args.success)
                {
                    PlayerOnOpenCompleted(args.type, settings);
                }
            };

            Player.OpenCompleted += _openCompletedEventHandler;

            if (settings.FullScreen)
            {
                SetFullScreen();
            }

            Player.Open(streamUrl);
        }

        public void StopPlayback()
        {
            Player.Stop();
            _videoInitialized = false;

            AudioTrack = null;
            _audioInitialized = false;
        }

        public void SetFullScreen()
        {
            if (!IsFullScreen)
            {
                IsFullScreen = Player.VideoView.FullScreen();
            }
        }

        public void SetNormalScreen()
        {
            if (IsFullScreen)
            {
                IsFullScreen = !Player.VideoView.NormalScreen();
            }
        }

        public void TogglePause()
        {
            if (Player.IsPlaying)
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
            if (mute == null || mute.Value != Player.audioPlayer.Mute)
            {
                Player.audioPlayer.Mute = !Player.audioPlayer.Mute;
            }
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
                    catch (PlatformNotSupportedException ex)
                    {
                        _logger.Warn(ex, "A non-critical error occurred.");
                    }
                });
            }

            _disposed = true;
        }


        private void PlayerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Player.Status))
            {
                IsPlaying = Player.Status == Status.Playing;
                IsPaused = Player.Status == Status.Paused;

                if (IsPlaying && Zoom != 0)
                {
                    Player.renderer.Zoom = Zoom;
                }
            }
        }

        private void PlayerOnOpenCompleted(MediaType mediaType, VideoDialogSettings settings)
        {
            switch (mediaType)
            {
                case MediaType.Video:
                    if (!_videoInitialized)
                    {
                        _videoInitialized = true;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            InitializeVideo(settings.VideoQuality, settings.Zoom, settings.AspectRatio, settings.StartTime);
                        });
                    }

                    Player.Play();
                    break;

                case MediaType.Audio:
                    if (!_audioInitialized)
                    {
                        _audioInitialized = true;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            InitializeAudio(settings.AudioDevice, settings.AudioTrack, settings.IsMuted, settings.Volume);
                        });
                    }

                    var audioStream = Player.Session.CurAudioStream;

                    if (audioStream != null)
                    {
                        var audioTrackId = new FlyleafAudioTrack(audioStream).Id;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            SetProperty(ref _audioTrack, AudioTracks.FirstOrDefault(track => track.Id == audioTrackId), nameof(AudioTrack));
                        });
                    }

                    break;
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

        private void AudioPlayerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Player.audioPlayer.Volume))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SetProperty(ref _volume, Player.audioPlayer.Volume, nameof(Volume));
                });
            }

            if (e.PropertyName == nameof(Player.audioPlayer.Mute))
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    SetProperty(ref _isMuted, Player.audioPlayer.Mute, nameof(IsMuted));
                });
            }
        }

        private void InitializeVideo(VideoQuality videoQuality, int zoom, string aspectRatio, long startTime)
        {
            AspectRatios.Clear();
            AspectRatios.AddRange(FlyleafLib.AspectRatio.AspectRatios.Where(ar => ar != FlyleafLib.AspectRatio.Custom).Select(ar => new FlyleafAspectRatio(ar)));
            
            Player.Session.PropertyChanged -= SessionOnPropertyChanged;
            Player.Session.PropertyChanged += SessionOnPropertyChanged;
            
            Duration = Player.Session.Movie.Duration;
            VideoQuality = videoQuality;

            // Actual zooming will be performed when player starts playing
            SetProperty(ref _zoom, zoom, nameof(Zoom));

            if (string.IsNullOrWhiteSpace(aspectRatio))
            {
                aspectRatio = FlyleafLib.AspectRatio.Keep.ValueStr;
            }

            var ratio = AspectRatios.FirstOrDefault(ar => ar.Value == aspectRatio);

            if (ratio != null)
            {
                AspectRatio = ratio;
            }

            if (startTime > 0)
            {
                Player.Session.CurTime = startTime;
                Player.Seek((int)startTime);
            }
        }

        private void InitializeAudio(string audioDevice, string audioTrack, bool isMuted, int volume)
        {
            AudioDevices.Clear();
            AudioDevices.AddRange(Master.AudioMaster.Devices.Select(device => new FlyleafAudioDevice(device)));

            AudioTracks.Clear();
            AudioTracks.AddRange(Player.curAudioPlugin.AudioStreams.Select(stream => new FlyleafAudioTrack(stream)));
            
            Player.audioPlayer.PropertyChanged -= AudioPlayerOnPropertyChanged;
            Player.audioPlayer.PropertyChanged += AudioPlayerOnPropertyChanged;
            
            Volume = volume;
            ToggleMute(isMuted);

            if (!string.IsNullOrWhiteSpace(audioDevice))
            {
                var device = AudioDevices.FirstOrDefault(d => d.Identifier == audioDevice);

                if (device != null)
                {
                    AudioDevice = device;
                }
            }
            else
            {
                var device = AudioDevices.FirstOrDefault(d => d.Identifier == Player.audioPlayer.Device);

                if (device != null)
                {
                    SetProperty(ref _audioDevice, device, nameof(AudioDevice));
                }
            }

            if (!string.IsNullOrWhiteSpace(audioTrack))
            {
                var track = AudioTracks.FirstOrDefault(t => t.Id == audioTrack);

                if (track != null)
                {
                    AudioTrack = track;
                }
            }
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
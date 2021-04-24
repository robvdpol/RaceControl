using FlyleafLib;
using FlyleafLib.MediaPlayer;
using Prism.Mvvm;
using RaceControl.Common.Enums;
using RaceControl.Common.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace RaceControl.Flyleaf
{
    public class FlyleafMediaPlayer : BindableBase, IMediaPlayer
    {
        private long _time;
        private long _duration;
        private int _volume;
        private bool _isPlaying;
        private bool _isPaused;
        private bool _isMuted;
        private ObservableCollection<IAudioDevice> _audioDevices;
        private ObservableCollection<IMediaTrack> _audioTracks;
        private IMediaTrack _audioTrack;
        private bool _videoInitialized;
        private bool _audioInitialized;
        private bool _disposed;

        public FlyleafMediaPlayer(Player player)
        {
            Player = player;
        }

        public Player Player { get; }

        public int MaxVolume => 150;

        public int Volume
        {
            get => _volume;
            set => Player.audioPlayer.Volume = Math.Min(Math.Max(value, 0), MaxVolume);
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

        public bool IsMuted
        {
            get => _isMuted;
            private set => SetProperty(ref _isMuted, value);
        }

        public ObservableCollection<IAudioDevice> AudioDevices => _audioDevices ??= new ObservableCollection<IAudioDevice>();
        public ObservableCollection<IMediaTrack> AudioTracks => _audioTracks ??= new ObservableCollection<IMediaTrack>();
        public IAudioDevice AudioDevice { get; set; }

        public IMediaTrack AudioTrack
        {
            get => _audioTrack;
            set
            {
                if (SetProperty(ref _audioTrack, value) && _audioTrack != null)
                {
                    var audioStream = Player.curAudioPlugin.AudioStreams[_audioTrack.Id];
                    Player.Open(audioStream);
                }
            }
        }

        public void StartPlayback(string streamUrl, string audioDevice, bool isMuted, int volume, VideoQuality videoQuality)
        {
            Player.PropertyChanged += PlayerOnPropertyChanged;
            Player.OpenCompleted += (_, args) =>
            {
                PlayerOnOpenCompleted(args, audioDevice, isMuted, volume, videoQuality);
            };
            Player.Open(streamUrl);
        }

        public void StopPlayback()
        {
            Player.Stop();
        }

        public void SetVideoQuality(VideoQuality videoQuality)
        {
            int minHeight;

            switch (videoQuality)
            {
                case VideoQuality.Medium:
                    minHeight = 720;
                    break;

                case VideoQuality.Low:
                    minHeight = 540;
                    break;

                case VideoQuality.Lowest:
                    minHeight = 360;
                    break;

                default:
                    minHeight = 1080;
                    break;
            }

            var videoStream = Player.curVideoPlugin.VideoStreams.OrderBy(s => s.Height).FirstOrDefault(s => s.Height >= minHeight);

            if (videoStream != null && Player.Session.CurVideoStream != videoStream)
            {
                Player.Open(videoStream);
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
                Player.Dispose();
            }

            _disposed = true;
        }

        private void PlayerOnOpenCompleted(Player.OpenCompletedArgs e, string audioDevice, bool isMuted, int volume, VideoQuality videoQuality)
        {
            if (!e.success)
            {
                return;
            }

            switch (e.type)
            {
                case MediaType.Video:
                    if (!_videoInitialized)
                    {
                        _videoInitialized = true;
                        Duration = Player.Session.Movie.Duration;
                        Player.Session.PropertyChanged += SessionOnPropertyChanged;
                        SetVideoQuality(videoQuality);
                    }

                    Player.Play();
                    break;

                case MediaType.Audio:
                    if (!_audioInitialized)
                    {
                        _audioInitialized = true;
                        Player.audioPlayer.PropertyChanged += AudioPlayerOnPropertyChanged;
                        Volume = volume;
                        ToggleMute(isMuted);
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            AudioTracks.Clear();
                            AudioTracks.AddRange(Player.curAudioPlugin.AudioStreams.Select((stream, index) => new FlyleafAudioTrack(index, stream)));
                        });
                    }

                    break;
            }
        }

        private void PlayerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Player.Status))
            {
                UpdatePlayerStatus();
            }
        }

        private void AudioPlayerOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Player.audioPlayer.Volume))
            {
                SetProperty(ref _volume, Player.audioPlayer.Volume, nameof(Volume));
            }

            if (e.PropertyName == nameof(Player.audioPlayer.Mute))
            {
                IsMuted = Player.audioPlayer.Mute;
            }
        }

        private void SessionOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Player.Session.CurTime))
            {
                SetProperty(ref _time, Player.Session.CurTime, nameof(Time));
            }
        }

        private void UpdatePlayerStatus()
        {
            IsPlaying = Player.Status == Status.Playing;
            IsPaused = Player.Status == Status.Paused;
        }
    }
}
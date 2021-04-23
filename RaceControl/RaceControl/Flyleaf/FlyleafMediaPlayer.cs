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
        private bool _disposed;

        public FlyleafMediaPlayer(Player player)
        {
            Player = player;
            Player.OpenCompleted += PlayerOnOpenCompleted;
            Player.PropertyChanged += PlayerOnPropertyChanged;
        }

        public Player Player { get; }

        public int Volume
        {
            get => _volume;
            set => Player.audioPlayer.Volume = value;
        }

        public long Time
        {
            get => _time;
            set => Player.Session.CurTime = value;
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

        public void StartPlayback(string streamUrl, VideoQuality videoQuality)
        {
            Player.Open(streamUrl);
        }

        public void StopPlayback()
        {
            Player.Stop();
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
            if (mute == null || mute.Value != IsMuted)
            {
                Player.audioPlayer.Mute = !IsMuted;
                IsMuted = Player.audioPlayer.Mute;
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

        private void PlayerOnOpenCompleted(object sender, Player.OpenCompletedArgs e)
        {
            if (!e.success)
            {
                return;
            }

            if (e.type == MediaType.Video)
            {
                Player.Play();
                Player.audioPlayer.PropertyChanged += AudioPlayerOnPropertyChanged;
                Player.Session.PropertyChanged += SessionOnPropertyChanged;
                Duration = Player.Session.Movie.Duration;
            }

            if (e.type == MediaType.Audio)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AudioTracks.Clear();
                    AudioTracks.AddRange(Player.curAudioPlugin.AudioStreams.Select((stream, index) => new FlyleafAudioTrack(index, stream)));
                });
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
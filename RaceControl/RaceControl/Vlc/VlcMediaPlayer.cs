using LibVLCSharp.Shared;
using Prism.Mvvm;
using RaceControl.Common.Enums;
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

        private long _time;
        private long _duration;
        private int _volume;
        private bool _isPlaying;
        private bool _isPaused;
        private bool _isMuted;
        private ObservableCollection<IAudioDevice> _audioDevices;
        private ObservableCollection<IMediaTrack> _audioTracks;
        private IAudioDevice _audioDevice;
        private IMediaTrack _audioTrack;
        private bool _disposed;

        public VlcMediaPlayer(LibVLC libVLC, MediaPlayer mediaPlayer)
        {
            _libVLC = libVLC;
            MediaPlayer = mediaPlayer;
            MediaPlayer.Playing += MediaPlayer_Playing;
            MediaPlayer.Paused += MediaPlayer_Paused;
            MediaPlayer.Stopped += MediaPlayer_Stopped;
            MediaPlayer.Muted += MediaPlayer_Muted;
            MediaPlayer.Unmuted += MediaPlayer_Unmuted;
            MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
            MediaPlayer.VolumeChanged += MediaPlayer_VolumeChanged;
            MediaPlayer.ESAdded += MediaPlayer_ESAdded;
            MediaPlayer.ESDeleted += MediaPlayer_ESDeleted;
            MediaPlayer.ESSelected += MediaPlayer_ESSelected;
            MediaPlayer.AudioDevice += MediaPlayer_AudioDevice;
            AudioDevices.AddRange(MediaPlayer.AudioOutputDeviceEnum.Select(device => new VlcAudioDevice(device)));
        }

        public MediaPlayer MediaPlayer { get; }

        public long Time
        {
            get => _time;
            set => MediaPlayer.Time = value;
        }

        public long Duration
        {
            get => _duration;
            private set => SetProperty(ref _duration, value);
        }

        public int Volume
        {
            get => _volume;
            set => MediaPlayer.Volume = value;
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

        public IAudioDevice AudioDevice
        {
            get => _audioDevice ??= AudioDevices.FirstOrDefault(ad => ad.Identifier == (MediaPlayer.OutputDevice ?? string.Empty));
            set
            {
                if (value != null)
                {
                    MediaPlayer.SetOutputDevice(value.Identifier);
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
                    MediaPlayer.SetAudioTrack(value.Id);
                }
            }
        }

        public async Task StartPlaybackAsync(string streamUrl, VideoQuality videoQuality)
        {
            using var media = CreateMedia(streamUrl, videoQuality);
            media.DurationChanged += (_, e) => Duration = e.Duration;
            await media.Parse(MediaParseOptions.ParseNetwork | MediaParseOptions.FetchNetwork);
            MediaPlayer.Play(media);
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

        public void ToggleMute(bool? mute)
        {
            if (mute == null || mute.Value != IsMuted)
            {
                MediaPlayer.ToggleMute();
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
                MediaPlayer.Dispose();
            }

            _disposed = true;
        }

        private void MediaPlayer_Playing(object sender, EventArgs e)
        {
            IsPlaying = true;
            IsPaused = false;
        }

        private void MediaPlayer_Paused(object sender, EventArgs e)
        {
            IsPlaying = false;
            IsPaused = true;
        }

        private void MediaPlayer_Stopped(object sender, EventArgs e)
        {
            IsPlaying = false;
            IsPaused = false;
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
            var volume = Math.Max(Convert.ToInt32(e.Volume * 100), 0);

            SetProperty(ref _volume, volume, nameof(Volume));
        }

        private void MediaPlayer_ESAdded(object sender, MediaPlayerESAddedEventArgs e)
        {
            if (e.Id < 0)
            {
                return;
            }

            switch (e.Type)
            {
                case TrackType.Audio:
                    var audioTrack = MediaPlayer.AudioTrackDescription.First(track => track.Id == e.Id);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        AudioTracks.Add(new VlcMediaTrack(audioTrack));
                    });

                    break;
            }
        }

        private void MediaPlayer_ESDeleted(object sender, MediaPlayerESDeletedEventArgs e)
        {
            if (e.Id < 0)
            {
                return;
            }

            switch (e.Type)
            {
                case TrackType.Audio:
                    var audioTrack = AudioTracks.FirstOrDefault(track => track.Id == e.Id);

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

        private void MediaPlayer_ESSelected(object sender, MediaPlayerESSelectedEventArgs e)
        {
            if (e.Id < 0)
            {
                return;
            }

            switch (e.Type)
            {
                case TrackType.Audio:
                    var audioTrack = AudioTracks.FirstOrDefault(track => track.Id == e.Id);

                    if (audioTrack != null)
                    {
                        SetProperty(ref _audioTrack, audioTrack, nameof(AudioTrack));
                    }

                    break;
            }
        }

        private void MediaPlayer_AudioDevice(object sender, MediaPlayerAudioDeviceEventArgs e)
        {
            var audioDevice = AudioDevices.FirstOrDefault(ad => ad.Identifier == e.AudioDevice);

            if (audioDevice != null)
            {
                SetProperty(ref _audioDevice, audioDevice, nameof(AudioDevice));
            }
        }

        private Media CreateMedia(string streamUrl, VideoQuality videoQuality)
        {
            switch (videoQuality)
            {
                case VideoQuality.Lowest:
                    return new Media(_libVLC, streamUrl, FromType.FromLocation, ":adaptive-maxwidth=640");

                case VideoQuality.Low:
                    return new Media(_libVLC, streamUrl, FromType.FromLocation, ":adaptive-maxwidth=960");

                case VideoQuality.Medium:
                    return new Media(_libVLC, streamUrl, FromType.FromLocation, ":adaptive-maxwidth=1280");

                default:
                    return new Media(_libVLC, streamUrl, FromType.FromLocation);
            }
        }
    }
}
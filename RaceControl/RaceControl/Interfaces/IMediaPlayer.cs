﻿using RaceControl.Common.Enums;
using RaceControl.Core.Settings;
using System;
using System.Collections.ObjectModel;

namespace RaceControl.Interfaces
{
    public interface IMediaPlayer : IDisposable
    {
        bool IsMuted { get; }

        int MaxVolume { get; }

        int Volume { get; set; }

        int Zoom { get; set; }

        long Time { get; set; }

        long Duration { get; }

        bool IsFullScreen { get; }

        bool IsPlaying { get; }

        bool IsPaused { get; }

        VideoQuality VideoQuality { get; set; }

        ObservableCollection<IAspectRatio> AspectRatios { get; }

        ObservableCollection<IAudioDevice> AudioDevices { get; }

        ObservableCollection<IMediaTrack> AudioTracks { get; }

        IAspectRatio AspectRatio { get; set; }

        IAudioDevice AudioDevice { get; set; }

        IMediaTrack AudioTrack { get; set; }

        void StartPlayback(string streamUrl, VideoDialogSettings settings);
        
        void StopPlayback();

        void SetFullScreen();

        void SetNormalScreen();

        void TogglePause();

        void ToggleMute(bool? mute);
    }
}
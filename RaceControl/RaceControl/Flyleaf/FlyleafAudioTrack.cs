﻿using FlyleafLib;
using FlyleafLib.Plugins.MediaStream;
using RaceControl.Common.Interfaces;

namespace RaceControl.Flyleaf
{
    public class FlyleafAudioTrack : IMediaTrack
    {
        public FlyleafAudioTrack(StreamBase audioStream)
        {
            var language = audioStream.Language ?? Language.Get(null);
            Id = language.IdSubLanguage;
            Name = language.LanguageName;
        }

        public string Id { get; }
        public string Name { get; }
    }
}
using FlyleafLib;
using FlyleafLib.MediaFramework.MediaStream;
using RaceControl.Common.Utils;
using RaceControl.Interfaces;

namespace RaceControl.Flyleaf
{
    public class FlyleafAudioTrack : IMediaTrack
    {
        public FlyleafAudioTrack(StreamBase audioStream)
        {
            var language = audioStream.Language ?? Language.Get(null);
            Id = language.IdSubLanguage;
            Name = language.OriginalInput != null ? language.OriginalInput.FirstCharToUpper() : language.LanguageName;
        }

        public string Id { get; }
        public string Name { get; }
    }
}
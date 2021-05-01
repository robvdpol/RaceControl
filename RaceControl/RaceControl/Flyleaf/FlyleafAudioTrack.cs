using FlyleafLib;
using FlyleafLib.MediaStream;
using RaceControl.Interfaces;

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
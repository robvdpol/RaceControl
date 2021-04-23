using FlyleafLib.Plugins.MediaStream;
using RaceControl.Common.Interfaces;

namespace RaceControl.Flyleaf
{
    public class FlyleafAudioTrack : IMediaTrack
    {
        public FlyleafAudioTrack(int id, AudioStream audioStream)
        {
            Id = id;
            Name = audioStream.GetDump();
        }

        public int Id { get; }
        public string Name { get; }
    }
}
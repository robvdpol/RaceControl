using FlyleafLib.Plugins.MediaStream;
using RaceControl.Common.Interfaces;

namespace RaceControl.Flyleaf
{
    public class FlyleafVideoTrack : IMediaTrack
    {
        public FlyleafVideoTrack(int id, VideoStream videoStream)
        {
            Id = id;
            Name = videoStream.GetDump();
        }

        public int Id { get; }
        public string Name { get; }
    }
}
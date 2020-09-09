using LibVLCSharp.Shared.Structures;
using RaceControl.Common.Interfaces;

namespace RaceControl.Vlc
{
    public class VlcMediaTrack : IMediaTrack
    {
        private readonly TrackDescription _trackDescription;

        public VlcMediaTrack(TrackDescription trackDescription)
        {
            _trackDescription = trackDescription;
        }

        public int Id => _trackDescription.Id;

        public string Name => _trackDescription.Name;
    }
}
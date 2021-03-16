using LibVLCSharp;
using RaceControl.Common.Interfaces;

namespace RaceControl.Vlc
{
    public class VlcMediaTrack : IMediaTrack
    {
        private readonly MediaTrack _mediaTrack;

        public VlcMediaTrack(MediaTrack mediaTrack)
        {
            _mediaTrack = mediaTrack;
        }

        public string Id => _mediaTrack.Id;

        public string Name => _mediaTrack.Name;
    }
}
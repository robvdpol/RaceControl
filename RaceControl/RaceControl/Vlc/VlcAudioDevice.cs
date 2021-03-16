using LibVLCSharp;
using RaceControl.Common.Interfaces;

namespace RaceControl.Vlc
{
    public class VlcAudioDevice : IAudioDevice
    {
        private readonly AudioOutputDevice _device;

        public VlcAudioDevice(AudioOutputDevice device)
        {
            _device = device;
        }

        public string Identifier => _device.DeviceIdentifier;

        public string Description => _device.Description;
    }
}
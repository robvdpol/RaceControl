namespace RaceControl.Flyleaf;

public class FlyleafAudioDevice : IAudioDevice
{
    public FlyleafAudioDevice(string device)
    {
        Identifier = device;
        Description = device;
    }

    public string Identifier { get; }
    public string Description { get; }
}
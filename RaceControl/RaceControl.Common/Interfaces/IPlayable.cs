using RaceControl.Common.Enum;

namespace RaceControl.Common.Interfaces
{
    public interface IPlayable
    {
        ContentType ContentType { get; }

        string ContentUrl { get; }
    }
}
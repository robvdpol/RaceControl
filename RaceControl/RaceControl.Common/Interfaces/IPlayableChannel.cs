using RaceControl.Common.Enums;

namespace RaceControl.Common.Interfaces
{
    public interface IPlayableChannel : IPlayableContent
    {
        ChannelType ChannelType { get; }
    }
}

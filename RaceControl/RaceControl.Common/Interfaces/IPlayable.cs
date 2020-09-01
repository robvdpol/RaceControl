using RaceControl.Common.Enum;
using System.Collections.Generic;

namespace RaceControl.Common.Interfaces
{
    public interface IPlayable
    {
        ContentType ContentType { get; }

        string ContentUrl { get; }

        List<string> DriverOccurrenceUrls { get; }
    }
}
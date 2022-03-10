using Prism.Events;
using RaceControl.Common.Enums;

namespace RaceControl.Events
{
    public class SaveLayoutEvent : PubSubEvent<ContentType>
    {
    }
}
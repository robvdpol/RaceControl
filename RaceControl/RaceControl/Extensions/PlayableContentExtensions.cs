using RaceControl.Common.Constants;
using RaceControl.Common.Enums;
using RaceControl.Common.Interfaces;

namespace RaceControl.Extensions
{
    public static class PlayableContentExtensions
    {
        public static string DetermineAudioLanguage(this IPlayableContent playableContent, string defaultAudioLanguage)
        {
            if (playableContent.ContentType != ContentType.Channel || playableContent.Name == ChannelNames.PitLane)
            {
                return LanguageCodes.English;
            }

            if (playableContent.Name is ChannelNames.Wif or ChannelNames.Tracker or ChannelNames.Data)
            {
                return !string.IsNullOrWhiteSpace(defaultAudioLanguage) ? defaultAudioLanguage : LanguageCodes.English;
            }

            return LanguageCodes.Undetermined;
        }
    }
}
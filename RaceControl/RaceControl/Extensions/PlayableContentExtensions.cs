using RaceControl.Common.Constants;
using RaceControl.Common.Enums;
using RaceControl.Common.Interfaces;

namespace RaceControl.Extensions
{
    public static class PlayableContentExtensions
    {
        public static string DetermineAudioLanguage(this IPlayableContent playableContent, string defaultAudioLanguage)
        {
            if (playableContent.ContentType != ContentType.Channel || playableContent.Name is ChannelNames.Wif or ChannelNames.Tracker or ChannelNames.Data or ChannelNames.PitLane)
            {
                return !string.IsNullOrWhiteSpace(defaultAudioLanguage) ? defaultAudioLanguage : LanguageCodes.English;
            }

            return LanguageCodes.Onboard;
        }
    }
}
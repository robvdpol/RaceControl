using System;
using System.Collections.ObjectModel;

namespace RaceControl.Core.Settings
{
    public interface ISettings
    {
        string SubscriptionToken { get; set; }

        string SubscriptionStatus { get; set; }

        DateTime? LastLogin { get; set; }

        string DefaultAudioLanguage { get; set; }

        bool DisableThumbnailTooltips { get; set; }

        bool DisableLiveSessionNotification { get; set; }

        bool DisableMpvNoBorder { get; set; }

        bool EnableMpvAutoSync { get; set; }

        string AdditionalMpvParameters { get; set; }

        string CustomMpvPath { get; set; }

        string LatestRelease { get; set; }

        string SelectedNetworkInterface { get; set; }

        ObservableCollection<string> SelectedSeries { get; set; }

        void Load();

        void Save();

        void ClearSubscriptionToken();

        void UpdateSubscriptionToken(string subscriptionToken, string subscriptionStatus);

        bool HasValidSubscriptionToken();
    }
}
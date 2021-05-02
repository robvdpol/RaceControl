﻿using System.Collections.ObjectModel;

namespace RaceControl.Core.Settings
{
    public interface ISettings
    {
        bool DisableMpvNoBorder { get; set; }

        string AdditionalMpvParameters { get; set; }

        string LatestRelease { get; set; }

        ObservableCollection<string> SelectedSeries { get; set; }

        void Load();

        void Save();
    }
}
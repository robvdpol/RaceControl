using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RaceControl.Core.Settings
{
    public interface IVideoDialogLayout
    {
        ObservableCollection<VideoDialogSettings> Instances { get; }

        void Add(IEnumerable<VideoDialogSettings> instances);

        void Clear();

        void Load();

        bool Save();
    }
}
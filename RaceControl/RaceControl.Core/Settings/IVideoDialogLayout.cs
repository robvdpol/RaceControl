using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RaceControl.Core.Settings
{
    public interface IVideoDialogLayout
    {
        ObservableCollection<VideoDialogInstance> Instances { get; }

        void Add(IEnumerable<VideoDialogInstance> instances);

        void Clear();

        void Load();

        bool Save();
    }
}
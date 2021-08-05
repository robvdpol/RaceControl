using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RaceControl.Common.Interfaces
{
    public interface IChannelCollection : INotifyPropertyChanged
    {
        IPlayableChannel CurrentChannel { get; set; }

        public ObservableCollection<IPlayableChannel> Globals { get; }
        public ObservableCollection<IPlayableChannel> Graphs { get; }
        public ObservableCollection<IPlayableChannel> Onboards { get; }
    }
}

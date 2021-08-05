using Prism.Mvvm;
using RaceControl.Common.Enums;
using RaceControl.Common.Interfaces;
using System.Collections.ObjectModel;
using System.Linq;

namespace RaceControl
{
    public class ChannelCollection : BindableBase, IChannelCollection
    {
        public ChannelCollection(ObservableCollection<IPlayableChannel> allChannels, IPlayableChannel currentChannel)
        {
            CurrentChannel = currentChannel;

            Onboards = new ObservableCollection<IPlayableChannel>(allChannels.Where(c => c.ChannelType == ChannelType.Onboard));
            Graphs = new ObservableCollection<IPlayableChannel>(allChannels.Where(c => c.ChannelType == ChannelType.Graph));
            Globals = new ObservableCollection<IPlayableChannel>(allChannels.Except(Onboards).Except(Graphs));
        }

        private IPlayableChannel _currentChannel;
        public IPlayableChannel CurrentChannel {
            get => _currentChannel;
            set => SetProperty(ref _currentChannel, value);
        }

        public ObservableCollection<IPlayableChannel> Globals { get; }
        public ObservableCollection<IPlayableChannel> Graphs { get; }
        public ObservableCollection<IPlayableChannel> Onboards { get; }
    }
}

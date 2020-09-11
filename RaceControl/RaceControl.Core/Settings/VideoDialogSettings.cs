using Prism.Mvvm;
using System.Windows;

namespace RaceControl.Core.Settings
{
    public class VideoDialogSettings : BindableBase
    {
        private double _top;
        private double _left;
        private double _width;
        private double _height;
        private ResizeMode _resizeMode;
        private WindowState _windowState;
        private bool _topmost;
        private bool _isMuted;
        private int _volume;
        private string _channelName;

        public double Top
        {
            get => _top;
            set => SetProperty(ref _top, value);
        }

        public double Left
        {
            get => _left;
            set => SetProperty(ref _left, value);
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public ResizeMode ResizeMode
        {
            get => _resizeMode;
            set => SetProperty(ref _resizeMode, value);
        }

        public WindowState WindowState
        {
            get => _windowState;
            set => SetProperty(ref _windowState, value);
        }

        public bool Topmost
        {
            get => _topmost;
            set => SetProperty(ref _topmost, value);
        }

        public bool IsMuted
        {
            get => _isMuted;
            set => SetProperty(ref _isMuted, value);
        }

        public int Volume
        {
            get => _volume;
            set => SetProperty(ref _volume, value);
        }

        public string ChannelName
        {
            get => _channelName;
            set => SetProperty(ref _channelName, value);
        }

        public static VideoDialogSettings GetDefaultSettings()
        {
            return new VideoDialogSettings
            {
                ResizeMode = ResizeMode.CanResize,
                WindowState = WindowState.Normal,
                Width = 960,
                Height = 550,
                IsMuted = false,
                Volume = 100
            };
        }
    }
}
namespace RaceControl.Core.Settings;

public class VideoDialogSettings : BindableBase
{
    private double _top;
    private double _left;
    private double _width;
    private double _height;
    private bool _fullScreen;
    private ResizeMode _resizeMode;
    private VideoQuality _videoQuality;
    private bool _topmost;
    private bool _isMuted;
    private int _zoom;
    private int _volume;
    private string _aspectRatio;
    private string _audioDevice;
    private string _audioTrack;
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

    public bool FullScreen
    {
        get => _fullScreen;
        set => SetProperty(ref _fullScreen, value);
    }

    public ResizeMode ResizeMode
    {
        get => _resizeMode;
        set => SetProperty(ref _resizeMode, value);
    }

    public VideoQuality VideoQuality
    {
        get => _videoQuality;
        set => SetProperty(ref _videoQuality, value);
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

    public int Zoom
    {
        get => _zoom;
        set => SetProperty(ref _zoom, value);
    }

    public string AspectRatio
    {
        get => _aspectRatio;
        set => SetProperty(ref _aspectRatio, value);
    }

    public string AudioDevice
    {
        get => _audioDevice;
        set => SetProperty(ref _audioDevice, value);
    }

    public string AudioTrack
    {
        get => _audioTrack;
        set => SetProperty(ref _audioTrack, value);
    }

    public string ChannelName
    {
        get => _channelName;
        set => SetProperty(ref _channelName, value);
    }

    public static VideoDialogSettings GetDefaultSettings()
    {
        return new()
        {
            FullScreen = false,
            ResizeMode = ResizeMode.CanResize,
            VideoQuality = VideoQuality.High,
            Width = 960,
            Height = 550,
            Volume = 100
        };
    }
}
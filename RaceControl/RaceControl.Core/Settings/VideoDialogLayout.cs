namespace RaceControl.Core.Settings;

public class VideoDialogLayout : BindableBase, IVideoDialogLayout
{
    private static readonly string Filename = FolderUtils.GetLocalApplicationDataFilename("RaceControl.layout.json");

    private readonly ILogger _logger;
    private readonly JsonSerializer _serializer;

    private ObservableCollection<VideoDialogSettings> _instances;

    public VideoDialogLayout(ILogger logger, JsonSerializer serializer)
    {
        _logger = logger;
        _serializer = serializer;
    }

    public ObservableCollection<VideoDialogSettings> Instances
    {
        get => _instances ??= new ObservableCollection<VideoDialogSettings>();
        set => SetProperty(ref _instances, value);
    }

    public bool Load(string filename = null)
    {
        filename ??= Filename;

        if (!File.Exists(filename))
        {
            return false;
        }

        Instances.Clear();

        try
        {
            using var file = File.OpenText(filename);
            _serializer.Populate(file, this);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while loading video dialog layout.");

            return false;
        }

        _logger.Info("Video dialog layout loaded.");

        return true;
    }

    public bool Save()
    {
        try
        {
            using var file = File.CreateText(Filename);
            _serializer.Serialize(file, this);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "An error occurred while saving video dialog layout.");

            return false;
        }

        _logger.Info("Video dialog layout saved.");

        return true;
    }

    public bool Import(string filename)
    {
        return Load(filename) && Save();
    }
}
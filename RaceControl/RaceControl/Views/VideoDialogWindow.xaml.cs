namespace RaceControl.Views;

public partial class VideoDialogWindow : IDialogWindow
{
    public IDialogResult Result { get; set; }

    public VideoDialogWindow()
    {
        InitializeComponent();
    }
}
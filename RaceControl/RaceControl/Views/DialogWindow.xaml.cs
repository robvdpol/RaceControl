namespace RaceControl.Views;

public partial class DialogWindow : IDialogWindow
{
    public IDialogResult Result { get; set; }

    public DialogWindow()
    {
        InitializeComponent();
    }
}
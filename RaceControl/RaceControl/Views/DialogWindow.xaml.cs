namespace RaceControl.Views;

public partial class DialogWindow : IDialogWindow
{
    public IDialogResult Result { get; set; }
    public object Content { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Window Owner { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public object DataContext { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Style Style { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public DialogWindow()
    {
        InitializeComponent();
    }

    public event RoutedEventHandler Loaded;
    public event EventHandler Closed;
    public event CancelEventHandler Closing;

    public void Close()
    {
        throw new NotImplementedException();
    }

    public void Show()
    {
        throw new NotImplementedException();
    }

    public bool? ShowDialog()
    {
        throw new NotImplementedException();
    }
}

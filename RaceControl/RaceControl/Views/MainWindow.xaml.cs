using System.Windows.Forms;

namespace RaceControl.Views;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);

        var version = AssemblyUtils.GetApplicationVersion();
        Title = $"Race Control {version.Major}.{version.Minor}.{version.Build} - An open source F1TV client";
    }

    private void MainWindowLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is ICloseWindow vm)
        {
            vm.Close += Close;
        }

        SetWindowsPositionAndSize();
    }

    private void SetWindowsPositionAndSize()
    {
        var screen = Screen.FromHandle(new WindowInteropHelper(this).Handle);
        var screenScale = ScreenHelper.GetScreenScale();
        var workingArea = screen.WorkingArea;

        var screenHeight = workingArea.Height / screenScale;
        var screenWidth = workingArea.Width / screenScale;
        var screenTop = workingArea.Top / screenScale;
        var screenLeft = workingArea.Left / screenScale;

        var windowHeight = screenHeight * .8;
        var windowWidth = screenWidth * .75;
        var windowTop = screenTop + (screenHeight - windowHeight) / 2;
        var windowLeft = screenLeft + (screenWidth - windowWidth) / 2;

        Top = windowTop;
        Left = windowLeft;
        Height = windowHeight;
        Width = windowWidth;
    }
}
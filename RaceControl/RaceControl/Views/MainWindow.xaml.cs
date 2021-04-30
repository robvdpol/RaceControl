using RaceControl.Common.Utils;
using RaceControl.Core.Helpers;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace RaceControl.Views
{
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
}
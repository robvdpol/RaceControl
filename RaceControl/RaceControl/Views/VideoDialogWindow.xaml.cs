using Prism.Services.Dialogs;
using RaceControl.ViewModels;

namespace RaceControl.Views
{
    public partial class VideoDialogWindow : IDialogWindow
    {
        public IDialogResult Result { get; set; }

        public VideoDialogWindow()
        {
            InitializeComponent();
        }

        private void WindowStateChanged(object sender, System.EventArgs e)
        {
            // Prevent maximizing the video window by snapping it to the top of the screen; it doesn't have a toolbar nor
            // is it visible on the task bar, so users can't restore it by dragging it down or clicking the "restore" icon(#119).
            
            // This event is raised by the FlyLeaf player as well when maximizing, on this window but with different content
            // (namely a WindowsFormsHost with the FlyLeaf player itself), then skip the overruling of the change in state.
            
            if (WindowState == System.Windows.WindowState.Maximized
                && this.Content is VideoDialog videoDialog
                && videoDialog.DataContext is VideoDialogViewModel viewModel
                && !viewModel.MediaPlayer.IsFullScreen)
            {
                WindowState = System.Windows.WindowState.Normal;
                viewModel.MediaPlayer.ToggleFullScreen();
            }
        }
    }
}
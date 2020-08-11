using Prism.Services.Dialogs;
using System.Windows;

namespace RaceControl.Views
{
    public partial class DialogWindow : Window, IDialogWindow
    {
        public IDialogResult Result { get; set; }

        public DialogWindow()
        {
            InitializeComponent();
        }
    }
}
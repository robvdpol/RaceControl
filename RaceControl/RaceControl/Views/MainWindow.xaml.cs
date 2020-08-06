using System;
using System.Reflection;
using System.Windows;

namespace RaceControl.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            var version = Assembly.GetExecutingAssembly().GetName().Version;
            Title = $"Race Control v{version.Major}.{version.Minor} - An open source F1TV client";
        }
    }
}
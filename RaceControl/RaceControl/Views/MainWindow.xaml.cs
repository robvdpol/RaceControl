using RaceControl.Common.Utils;
using System;
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

            var version = AssemblyUtils.GetApplicationVersion();
            Title = $"Race Control {version.Major}.{version.Minor} - An open source F1TV client";
        }
    }
}
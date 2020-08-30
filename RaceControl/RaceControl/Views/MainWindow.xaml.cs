using RaceControl.Common.Utils;
using System;

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
    }
}
using RaceControl.Core.Settings;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace RaceControl.Core.Helpers
{
    public static class ScreenHelper
    {
        public static Screen GetScreen(VideoDialogSettings settings)
        {
            return Screen.FromRectangle(new Rectangle((int)settings.Left, (int)settings.Top, (int)settings.Width, (int)settings.Height));
        }

        public static int GetScreenIndex(VideoDialogSettings settings)
        {
            var screen = GetScreen(settings);

            for (var i = 0; i < Screen.AllScreens.Length; i++)
            {
                if (Equals(screen, Screen.AllScreens[i]))
                {
                    return i;
                }
            }

            return default;
        }

        public static double GetScreenScale()
        {
            return Math.Max(Screen.PrimaryScreen.WorkingArea.Width / SystemParameters.PrimaryScreenWidth, Screen.PrimaryScreen.WorkingArea.Height / SystemParameters.PrimaryScreenHeight);
        }
    }
}
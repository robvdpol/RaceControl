using RaceControl.Core.Settings;
using System.Drawing;
using System.Windows.Forms;

namespace RaceControl.Core.Helpers;

public static class ScreenHelper
{
    public static Screen GetScreen(VideoDialogSettings settings)
    {
        var screenScale = GetScreenScale();
        var windowRectangle = new Rectangle((int)(settings.Left * screenScale), (int)(settings.Top * screenScale), (int)(settings.Width * screenScale), (int)(settings.Height * screenScale));

        return Screen.FromRectangle(windowRectangle);
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

    public static void GetRelativeCoordinates(int screenIndex, double absoluteLeft, double absoluteTop, out double relativeLeft, out double relativeTop)
    {
        var screen = Screen.AllScreens[screenIndex];
        var screenScale = GetScreenScale();
        relativeLeft = absoluteLeft * screenScale - screen.Bounds.Left;
        relativeTop = absoluteTop * screenScale - screen.Bounds.Top;
    }

    public static double GetScreenScale()
    {
        return Math.Max(Screen.PrimaryScreen.WorkingArea.Width / SystemParameters.PrimaryScreenWidth, Screen.PrimaryScreen.WorkingArea.Height / SystemParameters.PrimaryScreenHeight);
    }
}
using RaceControl.Common.Enums;

namespace RaceControl.Extensions
{
    public static class WindowLocationExtensions
    {
        public static bool IsSmall(this WindowLocation windowLocation)
        {
            return windowLocation != WindowLocation.TopLeft
                && windowLocation != WindowLocation.TopRight
                && windowLocation != WindowLocation.BottomLeft
                && windowLocation != WindowLocation.BottomRight;
        }

        public static double GetWindowWidthOrHeight(this WindowLocation windowLocation, int screenWidthOrHeight, double screenScale)
        {
            var factor = windowLocation.IsSmall() ? 3D : 2D;

            return screenWidthOrHeight / factor / screenScale;
        }

        public static void GetWindowTopAndLeft(this WindowLocation windowLocation, double screenTop, double screenLeft, double windowWidth, double windowHeight, out double top, out double left)
        {
            top = default;
            left = default;

            switch (windowLocation)
            {
                case WindowLocation.TopLeft:
                    top = screenTop;
                    left = screenLeft;
                    break;

                case WindowLocation.TopRight:
                    top = screenTop;
                    left = screenLeft + windowWidth;
                    break;

                case WindowLocation.BottomLeft:
                    top = screenTop + windowHeight;
                    left = screenLeft;
                    break;

                case WindowLocation.BottomRight:
                    top = screenTop + windowHeight;
                    left = screenLeft + windowWidth;
                    break;

                case WindowLocation.TopLeftSmall:
                    top = screenTop;
                    left = screenLeft;
                    break;

                case WindowLocation.TopSmall:
                    top = screenTop;
                    left = screenLeft + windowWidth;
                    break;

                case WindowLocation.TopRightSmall:
                    top = screenTop;
                    left = screenLeft + windowWidth + windowWidth;
                    break;

                case WindowLocation.LeftSmall:
                    top = screenTop + windowHeight;
                    left = screenLeft;
                    break;

                case WindowLocation.CenterSmall:
                    top = screenTop + windowHeight;
                    left = screenLeft + windowWidth;
                    break;

                case WindowLocation.RightSmall:
                    top = screenTop + windowHeight;
                    left = screenLeft + windowWidth + windowWidth;
                    break;

                case WindowLocation.BottomLeftSmall:
                    top = screenTop + windowHeight + windowHeight;
                    left = screenLeft;
                    break;

                case WindowLocation.BottomSmall:
                    top = screenTop + windowHeight + windowHeight;
                    left = screenLeft + windowWidth;
                    break;

                case WindowLocation.BottomRightSmall:
                    top = screenTop + windowHeight + windowHeight;
                    left = screenLeft + windowWidth + windowWidth;
                    break;
            }
        }
    }
}
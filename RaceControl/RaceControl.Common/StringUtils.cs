namespace RaceControl.Common
{
    public static class StringUtils
    {
        public static string GetUID(this string url)
        {
            if (url.EndsWith("/"))
            {
                url = url.Substring(0, url.Length - 1);
            }

            var idx = url.LastIndexOf('/');

            return url.Substring(idx + 1);
        }
    }
}
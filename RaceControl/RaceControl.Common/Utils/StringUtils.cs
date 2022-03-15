namespace RaceControl.Common.Utils;

public static class StringUtils
{
    public static string RemoveInvalidFileNameChars(this string filename, string replaceWith = "")
    {
        return string.Join(replaceWith, filename.Split(Path.GetInvalidFileNameChars()));
    }

    public static string GetYesNoString(this bool value)
    {
        return value ? "yes" : "no";
    }

    public static string FirstCharToUpper(this string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        return string.Concat(value[..1].ToUpper(), value[1..]);
    }
}
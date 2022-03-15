using GoogleCast.Channels;
using System.Reflection;

namespace RaceControl.Extensions;

public static class CastExtensions
{
    public static void SetApplicationId(this IMediaChannel mediaChannel, string applicationId)
    {
        var field = mediaChannel.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic).FirstOrDefault(f => f.Name.StartsWith("<ApplicationId>"));

        if (field != null)
        {
            field.SetValue(mediaChannel, applicationId);
        }
    }
}
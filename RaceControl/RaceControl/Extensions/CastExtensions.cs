using GoogleCast;
using GoogleCast.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RaceControl.Extensions
{
    public static class CastExtensions
    {

        public static void UpdateMediaChannelAppId(this IMediaChannel mediaChannel)
        {
            var type = Assembly.GetAssembly(typeof(Sender)).GetTypes().First(t => t.FullName == "GoogleCast.Channels.MediaChannel");
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);
            var field = fields.First(f => f.Name.Contains("ApplicationId"));
            if (field != null)
            {
                field.SetValue(mediaChannel, "B3E81094");
            }
        }

    }
}

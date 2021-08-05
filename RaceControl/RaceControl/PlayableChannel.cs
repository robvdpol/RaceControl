﻿using RaceControl.Common.Constants;
using RaceControl.Common.Enums;
using RaceControl.Common.Interfaces;
using RaceControl.Services.Interfaces.F1TV.Entities;

namespace RaceControl
{
    public class PlayableChannel : PlayableContent, IPlayableChannel
    {
        public ChannelType ChannelType { get; }

        public PlayableChannel(Session session, Channel channel)
        {
            var displayName = GetDisplayName(channel);

            Title = $"{session.LongName} - {displayName}";
            Name = channel.Name;
            DisplayName = displayName;
            ContentType = GetContentType(channel);
            ChannelType = GetChannelType(channel);
            ContentUrl = channel.PlaybackUrl;
            IsLive = session.IsLive;
            SyncUID = session.UID;
            SeriesUID = session.SeriesUID;
        }

        private static ChannelType GetChannelType(Channel channel)
        {
            return channel.ChannelType switch
            {
                "obc" => ChannelType.Onboard,
                "additional" when channel.Name == ChannelNames.Tracker || channel.Name == ChannelNames.Data => ChannelType.Graph,
                _ => ChannelType.Global
            };
        }

        private static string GetDisplayName(Channel channel)
        {
            switch (channel.Name)
            {
                case ChannelNames.Wif:
                    return "World Feed";

                case ChannelNames.PitLane:
                    return "Pit Lane";

                case ChannelNames.Tracker:
                    return "Driver Tracker";

                case ChannelNames.Data:
                    return "Live Timing";

                default:
                    return channel.Name;
            }
        }

        private static ContentType GetContentType(Channel channel)
        {
            return channel.ChannelType == ChannelTypes.Backup ? ContentType.Backup : ContentType.Channel;
        }
    }
}
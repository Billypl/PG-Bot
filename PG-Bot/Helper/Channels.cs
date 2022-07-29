using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace PG_Bot.Helper
{
    public class Channels
    {
        public static async Task<DiscordChannel?> getCategoryByName(DiscordGuild guild, string categoryName)
        {
            var channels = (await guild.GetChannelsAsync()).ToList();
            foreach (var channel in channels)
                if (channel.IsCategory)
                    if(channel.Name.Contains(categoryName, StringComparison.OrdinalIgnoreCase))
                        return channel;
            return null;
        }
    }
}

using System;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.IO;

namespace PG_Bot.Helper
{
    public static class Emojis
    {
        public static Dictionary<string, DiscordEmoji> Emoji = new(StringComparer.OrdinalIgnoreCase);
        public static void loadEmojis()
        {
            var emojisNames = File.ReadAllLines("../../../Helper/EmojisNames.txt");
            foreach (var name in emojisNames)
                    Emoji.Add(name, DiscordEmoji.FromName(Scripts.Bot.Client, name));
        }
    }
}

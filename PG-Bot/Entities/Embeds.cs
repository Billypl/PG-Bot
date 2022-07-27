using System.Collections.ObjectModel;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.EventHandling;
using PG_Bot.Helper;
using static DSharpPlus.Entities.DiscordEmbedBuilder;

namespace PG_Bot.Entities
{
    public class Embeds
    {
        public static DiscordEmbedBuilder serverJoinEmbed(DiscordMember member)
        {
            return new DiscordEmbedBuilder
            {
                Color = DiscordColor.Red,
                Title = "New member joined!",
                Description = $"\"{member.Username}\" has joined our server!",
                Thumbnail = new EmbedThumbnail { Url = member.AvatarUrl },
                Timestamp = DateTime.Now
            };
        }
        public static DiscordEmbedBuilder pollEmbed(string pollTitle, DiscordMember author, TimeSpan duration)
        {
            var pollEmbed = new DiscordEmbedBuilder
            {
                Title = pollTitle,
                Description = $"Poll ends at: \n " +
                              $"Date: {endDate(duration, "d")} \n " +
                              $"Hour: {endDate(duration, "HH:mm")}",
                Color = DiscordColor.Cyan,
                Author = new EmbedAuthor { Name = author.DisplayName },
                Thumbnail = new EmbedThumbnail { Url = author.AvatarUrl }
            };
            return pollEmbed;
        }
        private static string endDate(TimeSpan duration, string dateFormat)
        {
            return (DateTime.Now + duration).ToString(dateFormat);
        }
        public static DiscordEmbedBuilder pollResultEmbed(string pollTitle, DiscordMember author, ReadOnlyCollection<PollEmoji> result)
        {
            var resultEmbed = new DiscordEmbedBuilder
            {
                Title = $"Results of \"{pollTitle}\" poll",
                Author = new EmbedAuthor { Name = author.DisplayName },
                Description = string.Join("\n", result.Select(x => $"{x.Emoji}: {x.Total}")),
                Thumbnail = new EmbedThumbnail { Url = author.AvatarUrl }
            };
            return resultEmbed;
        }
        public static DiscordEmbedBuilder askingForRoleEmbed(CommandContext ctx, string roleName)
        {
            return new DiscordEmbedBuilder
            {
                Title = $"Would you like to be a {roleName}?",
                Description = $"{Emojis.Emoji[":thumbsUp:"]} grant role, \n\n" +
                              $"{Emojis.Emoji[":thumbsDown:"]} revoke role \n\n" +
                              $"{Emojis.Emoji[":x:"]} cancel action",
                Color = DiscordColor.Green,
                Thumbnail = new EmbedThumbnail { Url = ctx.User.AvatarUrl }
            };
        }
        public static DiscordEmbedBuilder chooseStudyFieldEmbed(string fieldName)
        {
            return new DiscordEmbedBuilder
            {
                Title = fieldName,
                Color = DiscordColor.Green,
                Description = "Jesteś studentem tego kierunku?"
                //Thumbnail = new EmbedThumbnail {  }
                //TODO: make color and thumbnail the color of the division 
            };
        }
    }
}

using System.Collections.ObjectModel;
using System.Runtime.InteropServices.ComTypes;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.EventHandling;
using PG_Bot.Commands;
using PG_Bot.Helper;
using PG_Bot.Scripts;
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

        /// TODO: server stats
        public static DiscordEmbedBuilder divisionsStatsEmbed()
        {
            return new DiscordEmbedBuilder
            {
                Title = "Statystyki serwera: KIERUNKI",
                Color = DiscordColor.Blue,
                Description = fillDivisionsStats2().Result
            };
        }

        private static async Task<string> fillDivisionsStats()
        {
            var divisionNames = Bot.DivisionChoosing.divisionNames;
            string stats = "";

            var members = await Bot.DivisionChoosing.Guild.GetAllMembersAsync();


            foreach (var name in divisionNames)
            {
                var divisionRole = Roles.getRoleByName(Bot.DivisionChoosing.Guild.Roles, name);
                var divisionCount = 0;

                foreach (var member in members)
                    if(member.Roles.Contains(divisionRole))
                        divisionCount++;

                stats += $"{name}: {divisionCount}\n";
            }
            return stats;
        }
        private static async Task<string> fillDivisionsStats2()
        {
            var divisionNames = Bot.DivisionChoosing.divisionNames;
            string stats = "";

            var members = await Bot.DivisionChoosing.Guild.GetAllMembersAsync();
            var departments = Bot.DivisionChoosing.departaments;

            stats += "*Wszyscy członkowie serwera:* " + members.Count() + "\n";
            stats += "*Wszyscy z przypisanym kierunkiem:* " + getMembersCountWithDivisionAssigned(members) + "\n";

            stats += await generateDepartmentStats(departments.WA, "**Wydział: WA**" , members);
            stats += await generateDepartmentStats(departments.WCh, "**Wydział: WCh**", members);
            stats += await generateDepartmentStats(departments.WETI, "**Wydział: WETI**", members);
            stats += await generateDepartmentStats(departments.WEiA, "**Wydział: WEiA**", members);
            stats += await generateDepartmentStats(departments.WFTiMS, "**Wydział: WFTiMS**", members);
            stats += await generateDepartmentStats(departments.WILiŚ, "**Wydział: WILiŚ**", members);
            stats += await generateDepartmentStats(departments.WIMiO, "**Wydział: WIMiO**", members);
            stats += await generateDepartmentStats(departments.WZiE, "**Wydział: WZiE**", members);
            stats += await generateDepartmentStats(departments.crossDepartment, "**Międzywydziałowe**", members);

            return stats;
        }

        private static int getMembersCountWithDivisionAssigned(IReadOnlyCollection<DiscordMember> members)
        {
            int membersCountWithDivisionAssigned = 0;
            foreach (var member in members)
                if (member.Roles.Count() != 0)
                    membersCountWithDivisionAssigned++;
            return membersCountWithDivisionAssigned;
        }

        private static async Task<string> generateDepartmentStats(List<string> divisionsNames, string departmentName, IReadOnlyCollection<DiscordMember> members)
        {
            var departmentMembersCount = 0;
            string departmentStats = "";

            foreach (var divisionName in divisionsNames)
            {
                var divisionMembersCount = 0;
                departmentStats += divisionName + ": ";


                if (departmentName != "**Międzywydziałowe**")
                {
                    var tempDepName = getStringFromCharToChar(departmentName, ":", "**");
                    divisionMembersCount = await generateDivisionStats(members, divisionName + " " + "("+tempDepName+")");

                }
                else
                    divisionMembersCount = await generateDivisionStats(members, divisionName);
                
                
                departmentMembersCount += divisionMembersCount;
                departmentStats += divisionMembersCount.ToString();
                departmentStats += "\n";
            }
            var stats = "";
            stats += "\n" + departmentName;
            stats += " (" + departmentMembersCount + ")" + "\n" + departmentStats;
            return stats;
        }

        private static async Task<int> generateDivisionStats(IReadOnlyCollection<DiscordMember> members, string divisionName)
        {
            var divisionRole = Roles.getRoleByName(Bot.DivisionChoosing.Guild.Roles, divisionName);
            var divisionMembersCount = 0;

            foreach (var member in members)
                if (member.Roles.Contains(divisionRole))
                    divisionMembersCount++;

            return divisionMembersCount;
        }

        public static string getStringFromCharToChar(string str, string starting, string ending)
        {
            var startingIndex = str.IndexOf(starting);
            var endIndex = str.LastIndexOf(ending);
            if (startingIndex >= endIndex)
                return null;
            var nameLength = endIndex - startingIndex - 2;
            string name = str.Substring(startingIndex + 2, nameLength);
            return name;
        }

    }
}

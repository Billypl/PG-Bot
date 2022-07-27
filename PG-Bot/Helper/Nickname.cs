using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace PG_Bot.Helper
{
    public class Nickname
    {   
        public static async Task modify(DiscordGuild guild, ulong userID, string nick)
        {
            var member = await guild.GetMemberAsync(userID);
            await member.ModifyAsync(x => x.Nickname = nick);
        }
        public static string formatWithNum(IReadOnlyDictionary<ulong, DiscordMember> members, string memberName)
        {
            var number = getHighestMemberNumber(members) + 1;
            string formattedNumber = (number > 9) ? number.ToString() : ("0" + number);
            string nickname = $"#{formattedNumber}-{memberName}";
            return nickname;
        }
        private static int getHighestMemberNumber(IReadOnlyDictionary<ulong, DiscordMember> members)
        {
            var memberNumbers =
                (from member in members.Values
                    where member.DisplayName.StartsWith("#")
                    select Convert.ToInt32(member.DisplayName.Substring(1, 2))).ToList();
            memberNumbers.Sort();
            return memberNumbers.Last();
        }
    }
}

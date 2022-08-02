using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using PG_Bot.Data;

namespace PG_Bot.Helper
{
    public static class Roles
    {
        private static readonly string[] authorizedRolesNames = { "Tester", "Bot-Dev" };
        //TODO: change IEnumerable to IReadOnlyDictionary
        public static bool hasNeededPermissions(DiscordMember member, IEnumerable<DiscordRole> roles)
        {
            return authorizedRolesNames.Any
                (roleName => member.Roles.Contains(getRoleByName(roles, roleName)));
        }
        public static DiscordRole? getRoleByName(IEnumerable<DiscordRole> roles, string roleName)
        {
            return roles.SingleOrDefault(
                r => string.Equals(
                    r.Name, roleName, StringComparison.CurrentCultureIgnoreCase));
        }


        private static ulong getTheIdFromField(string text)
        {
            int subLength = text.IndexOf('\n') - text.IndexOf(':');
            string ID = text.Substring(text.IndexOf(':') + 1, subLength);
            return ulong.Parse(ID);
        }
    }
}
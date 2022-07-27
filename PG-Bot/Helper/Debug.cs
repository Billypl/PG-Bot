using DSharpPlus.CommandsNext;
using System;
using System.Threading.Tasks;

namespace PG_Bot.Helper
{
    public class Debug
    {
        public async Task printRoles(CommandContext ctx)
        {
            foreach (var roleName in ctx.Guild.Roles)
                Console.WriteLine(roleName.Value);

            await ctx.Channel.SendMessageAsync("Roles printed to the console");
        }

        public async Task printMembers(CommandContext ctx)
        {
            await ctx.Guild.RequestMembersAsync();

            foreach (var roleName in ctx.Guild.Roles)
                Console.WriteLine(roleName.Value);

            await ctx.Channel.SendMessageAsync("Members printed to the console");
        }
    }
}

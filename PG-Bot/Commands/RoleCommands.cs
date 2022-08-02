using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System.Threading.Tasks;
using PG_Bot.Entities;
using PG_Bot.Helper;
using static PG_Bot.Helper.Roles;

namespace PG_Bot.Commands
{
    public class RoleCommands : BaseCommandModule
    {
        // useful for giving users roles according to their preferences
        // i.e. give c# or .NET role to view only those categories
        [Command("roles")]
        [Description("Manages user roles (admin-only for now)")]
        public async Task ManageRoles(CommandContext ctx, params string[] roleFormalName)
        {
            if (!Roles.hasNeededPermissions(ctx.Member, ctx.Guild.Roles.Values))
                return;
            string roleName = string.Join(" ", roleFormalName);
            DiscordRole role = getRoleByName(ctx.Guild.Roles.Values, roleName);

            if (!await canAssignRole(ctx, role)) return;

            var askingMessage = await ctx.Channel.SendMessageAsync(Embeds.askingForRoleEmbed(ctx, roleName));
            await createReactions(askingMessage);
            var reactionResult = collectUserReaction(ctx, askingMessage).Result;

            await grantOrRevokeRole(ctx, role, reactionResult);

            await askingMessage.DeleteAsync();
        }

        private static async Task<bool> canAssignRole(CommandContext ctx, DiscordRole role)
        {
            if (!hasNeededPermissions(ctx.Member, ctx.Guild.Roles.Values))
            {
                await ctx.Message.RespondAsync("You don't have permission to run this command!");
                return false;
            }
            if (role is null)
            {
                await ctx.Message.RespondAsync("Couldn't find the role");
                return false;
            }
            return true;
        }
        private static async Task<InteractivityResult<MessageReactionAddEventArgs>> collectUserReaction(CommandContext ctx, DiscordMessage askingMessage)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var reactionResult = await interactivity.WaitForReactionAsync(
                x =>
                    x.Message == askingMessage &&
                    x.Channel == ctx.Channel &&
                    (x.Emoji == Emojis.Emoji[":thumbsUp:"]|| x.Emoji == Emojis.Emoji[":thumbsDown:"] || x.Emoji == Emojis.Emoji[":x:"]) &&
                    x.User == ctx.User);

            return reactionResult;
        }
        private static async Task grantOrRevokeRole(CommandContext ctx, DiscordRole role, InteractivityResult<MessageReactionAddEventArgs> reactionResult)
        {
            if (reactionResult.Result is null)
                await ctx.Message.RespondAsync("Time to react passed");
            else if (reactionResult.Result.Emoji == Emojis.Emoji[":thumbsUp:"])
            {
                await ctx.Member.GrantRoleAsync(role);
                await ctx.Message.RespondAsync("Role granted!");
            }
            else if (reactionResult.Result.Emoji == Emojis.Emoji[":thumbsDown:"])
            {
                await ctx.Member.RevokeRoleAsync(role);
                await ctx.Message.RespondAsync("Role revoked!");
            }
            else if (reactionResult.Result.Emoji == Emojis.Emoji[":x:"])
                await ctx.Message.RespondAsync("Action has been cancelled");
        }
        private static async Task createReactions(DiscordMessage createdMessage)
        {
            await createdMessage.CreateReactionAsync(Emojis.Emoji[":thumbsUp:"]);
            await createdMessage.CreateReactionAsync(Emojis.Emoji[":thumbsDown:"]);
            await createdMessage.CreateReactionAsync(Emojis.Emoji[":x:"]);
        }
    }
}


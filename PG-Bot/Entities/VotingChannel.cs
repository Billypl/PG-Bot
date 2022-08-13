using System.Text.Json;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.VisualBasic;
using PG_Bot.Commands;
using PG_Bot.Data;
using PG_Bot.Entities;
using PG_Bot.Helper;
using PG_Bot.Scripts;
using static PG_Bot.Scripts.Bot;

namespace PG_Bot.Entities
{
    public class VotingChannelCommands : BaseCommandModule
    {

        [Command("addVoteMsg")]
        public async Task addMessage(CommandContext ctx, string category)
        {
            await Bot.VotingChannel.addMessage(ctx, category);
        }

        [Command("addVoteOption")]
        public async Task addChoice(CommandContext ctx, string categoryName, string choiceTitle, DiscordEmoji emoji)
        {
            await Bot.VotingChannel.addChoice(ctx, categoryName, choiceTitle, emoji);
        }

        [Command("removeVoteOption")]
        public async Task removeVoteOption(CommandContext ctx, string categoryName, string choiceTitle)
        {
            await Bot.VotingChannel.removeChoice(ctx, categoryName, choiceTitle);
        }
        [Command("removeVoteMsg")]
        public async Task removeVoteMsg(CommandContext ctx, string categoryName)
        {
            await Bot.VotingChannel.removeMessage(ctx, categoryName);
        }
    }


    public class VotingChannel 
    {
        public DiscordChannel Channel;
        public DiscordGuild Guild;
        public List<DiscordMessage> Messages;

        public VotingChannel()
        {
            Guild = Bot.Client.Guilds[IDs.PG_GUILD];
            Channel = Guild.GetChannel(IDs.VOTING_CHANNEL);
            Messages = getVotingMessages();
        }

        public List<DiscordMessage> getVotingMessages()
        {
            var messages = Channel.GetMessagesAsync().Result;
            return messages.Where(
                message => message.Author.IsBot && 
                           message.Embeds is not null &&
                           message.Embeds.Count != 0 &&
                           message.Embeds[0].Color.Value.Equals(DiscordColor.Orange)
                           ).ToList();
        }

        public async Task addMessage(CommandContext ctx, string category)
        {
            if (!Roles.hasNeededPermissions(ctx.Member!, Guild.Roles)) return;

            var msg = await ctx.Channel.SendMessageAsync(Embeds.votingMessage(category));
            Messages.Add(msg);
        }

        public async Task addChoice(CommandContext ctx, string categoryName, string choiceTitle, DiscordEmoji emoji)
        {
            if (!Roles.hasNeededPermissions(ctx.Member!, Guild.Roles)) return;

            var msgIndex = getMessageIndexByTitle(categoryName);

            if (!(await canChoiceBeAdded(msgIndex, ctx, choiceTitle, emoji)))
                return;

            await updateEmbedAfterAddingChoice(msgIndex, choiceTitle, emoji);
            await Messages[msgIndex].CreateReactionAsync(emoji);

            await ctx.Guild.CreateRoleAsync(choiceTitle, mentionable: true);
        }

        private async Task updateEmbedAfterAddingChoice(int msgIndex, string choiceTitle, DiscordEmoji emoji)
        {
            var msg = Messages[msgIndex];
            var oldEmbed = msg.Embeds[0];

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder(oldEmbed);
            embed.AddField(choiceTitle, emoji, true);

            msg = await msg.ModifyAsync(builder => builder.Embed = embed);
            Messages[msgIndex] = msg;
        }

        public async Task removeChoice(CommandContext ctx, string categoryName, string choiceTitle)
        {
            for (int i = 0; i < Messages.Count; i++)
            {
                if (!isRightMessage(Messages[i], categoryName))
                    continue;
                var msg = Messages[i];
                var oldEmbed = Messages[i].Embeds[0];
                DiscordEmbedBuilder embed = new DiscordEmbedBuilder(Embeds.votingMessage(oldEmbed.Title));

                foreach (var field in oldEmbed.Fields)
                {
                    if (field.Name == choiceTitle)
                    {
                        var role = Roles.getRoleByName(ctx.Guild.Roles, field.Name);
                        await ctx.Guild.Roles[role.Id].DeleteAsync();
                        await msg.DeleteReactionsEmojiAsync(DiscordEmoji.FromUnicode(field.Value));
                    }
                    else
                        embed.AddField(field.Name, field.Value, true);
                }
                msg = await msg.ModifyAsync(builder => builder.Embed = embed);
                Messages[i] = msg;
            }
        }

        public async Task removeMessage(CommandContext ctx, string categoryName)
        {
            var msgIndex = getMessageIndexByTitle(categoryName);
            var msg = Messages[msgIndex];

            foreach (var field in msg.Embeds[0].Fields)
                await removeChoice(ctx, categoryName, field.Name);
            await msg.DeleteAsync();
            Messages.RemoveAt(msgIndex);
        }

        private bool isRightMessage(DiscordMessage msg, string categoryName) { return msg.Embeds[0].Title == categoryName; }

        private async Task<bool> canChoiceBeAdded(int msgIndex, CommandContext ctx, string choiceTitle, DiscordEmoji emoji)
        {
            var msg = Messages[msgIndex];
            var oldEmbed = msg.Embeds[0];


            if (msgIndex == -1)
            {
                await ctx.Channel.SendMessageAsync("Nie znaleziono wiadomości z podanym tytułem");
                return false;
            }

            if (oldEmbed.Fields is null)
                return true;

            foreach (var field in oldEmbed.Fields)
            {
                if (field.Name == choiceTitle)
                {
                    await ctx.Channel.SendMessageAsync("Dana kategoria już istnieje");
                    return false;
                }
                if (field.Value == emoji)
                {
                    await ctx.Channel.SendMessageAsync("Reakcja się powtarza!");
                    return false;
                }
            }
            return true;
        }

        public int getMessageIndexByTitle(string title)
        {
            for (int i = 0; i < Messages.Count; i++)
                if (Messages[i].Embeds[0].Title == title)
                    return i;
            return -1;
        }

        public async Task onVoteMessageReacted(MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot || e.Channel != Channel || Messages.All(message => message != e.Message))
                return;

            var msg = await e.Channel.GetMessageAsync(e.Message.Id);

            foreach (var field in msg.Embeds[0].Fields)
            {
                if (field.Value == e.Emoji)
                {
                    var member = (DiscordMember)(e.User);
                    var role = Roles.getRoleByName(e.Guild.Roles, field.Name);
                    if(role is null) return;

                    await member.GrantRoleAsync(role);
                    return;
                }
            }
        }

        public async Task onVoteMessageReactionRemoved(MessageReactionRemoveEventArgs e)
        {
            if (e.User.IsBot || e.Channel != Channel || Messages.All(message => message != e.Message))
                return;

            var msg = await e.Channel.GetMessageAsync(e.Message.Id);

            foreach (var field in msg.Embeds[0].Fields)
            {
                if (field.Value == e.Emoji)
                {
                    var member = (DiscordMember)(e.User);
                    var role = Roles.getRoleByName(e.Guild.Roles, field.Name);
                    if (role is null) 
                        return;

                    await member.RevokeRoleAsync(role);
                    return;
                }
            }
        }
    }
}

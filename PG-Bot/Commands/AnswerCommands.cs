using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PG_Bot.Entities;
using PG_Bot.Helper;
using PG_Bot.Scripts;

namespace PG_Bot.Commands
{
    public class AnswerCommands : BaseCommandModule
    {
        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Pong");
        }

        [Command("add")]
        [Description("Adds two numbers")]
        public async Task Add(
            CommandContext ctx, 
            [Description("First number")] int n1, 
            [Description("Second number")] int n2
            )
        {
            var result = n1 + n2;
            await ctx.Channel.SendMessageAsync(result.ToString());
        }

        [Command("respond")]
        [Description("Responds with content of message following this command")]
        public async Task Response(CommandContext ctx)
        {
            var interactivity = ctx.Client.GetInteractivity();
            var message = await interactivity.WaitForMessageAsync(x => x.Channel == ctx.Channel);
            if (message.TimedOut)
                return;
            await ctx.Channel.SendMessageAsync(message.Result.Content);
        }


        [Command("poll")]
        [Description("Creates poll, with specified duration and title")]
        public async Task Poll(CommandContext ctx, 
            [Description("Duration of poll, after number specify time unit (s/m/h/d)")] TimeSpan duration, 
            [Description("Title for your poll")] params string[] title)
        {
            await ctx.Message.DeleteAsync();
            
            string pollTitle = string.Join(" ", title);

            DiscordMessage pollMessage = 
                await ctx.Channel.SendMessageAsync(
                    Embeds.pollEmbed(pollTitle, ctx.Member, duration));

            List<DiscordEmoji> reactionEmojis = pollEmojis(ctx);
            var result = await pollMessage.DoPollAsync(reactionEmojis, null, duration);
            var resultEmbed = Embeds.pollResultEmbed(pollTitle, ctx.Member, result);
            await ctx.Channel.SendMessageAsync(resultEmbed);
            
            await pollMessage.DeleteAsync();
        }
        
        private List<DiscordEmoji> pollEmojis(CommandContext ctx)
        {
            var emojis = new List<DiscordEmoji>
            {
                Emojis.Emoji[":white_check_mark:"],
                Emojis.Emoji[":x:"]
            };
            return emojis;
        }
    }
}

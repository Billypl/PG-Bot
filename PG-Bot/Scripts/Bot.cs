﻿using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using System.Threading.Tasks;
using PG_Bot.Commands;
using PG_Bot.Configs;
using PG_Bot.Data;
using PG_Bot.Helper;

namespace PG_Bot.Scripts
{
    public class Bot
    {
        public static DiscordClient Client { get; set; }
        public static CommandsNextExtension Commands { get; set; }
        public static InteractivityExtension Interactivity { get; set; }

        public static DivisionChoosingAttributes DivisionChoosing;

        public async Task RunAsync()
        {
            ConfigManager.assignConfigs();
            Client.Ready += OnClientReady;
            Client.GuildDownloadCompleted += OnGuildDownloadCompleted;
            Client.MessageReactionAdded += OnMessageReactionAdded;
            Client.MessageCreated += OnMessageCreated;
            Client.GuildMemberAdded += ClientOnGuildMemberAdded;

            Commands.RegisterCommands<Commands.AnswerCommands>();
            Commands.RegisterCommands<Commands.RoleCommands>();
            Commands.RegisterCommands<Commands.DivisionCommands>();
            Helper.Emojis.loadEmojis();

            await Client.ConnectAsync();

            await Task.Delay(-1); // prevents auto-disconnecting
        }

        
        

        private async Task ClientOnGuildMemberAdded(DiscordClient sender, GuildMemberAddEventArgs e)
        {

            var membersCount = (await e.Guild.GetAllMembersAsync()).Count();
            if (membersCount == 420)
                await e.Guild.GetChannel(IDs.ANNOUNCMENTS_CHANNEL).SendMessageAsync(
                    Emojis.Emoji[":partying_face:"] +
                    " Nasz serwer ma 420 członków " +
                    Emojis.Emoji[":partying_face:"] +
                    Emojis.Emoji[":smoking:"]);
            if (membersCount == 690)
                await e.Guild.GetChannel(IDs.ANNOUNCMENTS_CHANNEL).SendMessageAsync(
                    Emojis.Emoji[":partying_face:"] +
                    " Dobiliśmy do 690 członków " +
                    Emojis.Emoji[":smirk:"] + "\n\n\n \t\t\t\t\t\t ||**nice.**||");

            await DivisionChoosing.refreshStatsChannel();
        }

        private async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            await piwoCommand(e);
        }

        private async Task piwoCommand(MessageCreateEventArgs e)
        {
            string[] piwoKeyWords = { "piwo", "piwa", "piwko", "piwko", "piwunio", "piweczko", "piwie", "napój bogów", "chlanie", "chlańsko", "najebać", "najebię", "najebie"};
            foreach (string piwo in piwoKeyWords)
            {
                if (e.Message.Author.IsBot == false &&
                    e.Message.Content.Contains(piwo, StringComparison.OrdinalIgnoreCase))
                {
                    await e.Message.CreateReactionAsync(Emojis.Emoji[":beer:"]);
                    return;
                }
            }
        }

        private async Task OnMessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
        {
            await DivisionChoosing.choosedDivisionMessage(e);
        }

        private Task OnGuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
        {
            DivisionChoosing = new DivisionChoosingAttributes();
            return Task.CompletedTask;
        }

        private Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }

    }
}
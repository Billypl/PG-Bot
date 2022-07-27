using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using System.Threading.Tasks;
using PG_Bot.Commands;
using PG_Bot.Configs;


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

            Commands.RegisterCommands<Commands.AnswerCommands>();
            Commands.RegisterCommands<Commands.RoleCommands>();
            Commands.RegisterCommands<Commands.DivisionCommands>();
            Helper.Emojis.loadEmojis();

            await Client.ConnectAsync();

            await Task.Delay(-1); // prevents auto-disconnecting
        }

        private Task OnMessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
        {
            DivisionChoosing.choosedDivisionMessage(e);
            return Task.CompletedTask;
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
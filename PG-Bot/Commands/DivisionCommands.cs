using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Security.Authentication.ExtendedProtection;
using System.Text.Json;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.Models;
using PG_Bot.Data;
using PG_Bot.Entities;
using PG_Bot.Helper;
using PG_Bot.Scripts;

namespace PG_Bot.Commands
{
    public class DivisionCommands : BaseCommandModule
    {
        [Command("createDivision")]
        public async Task createDivision(CommandContext ctx, DiscordEmoji emoji, params string[] divisionNameParams)
        {
            string divisionName = string.Join(" ", divisionNameParams);
            if (Roles.hasNeededPermissions(ctx.Member, ctx.Guild.Roles.Values))
            {
                var message = await ctx.Channel.SendMessageAsync(emoji + $" Kierunek {divisionName} w trakcie tworzenia...");
                await createNewDivision(ctx.Guild, emoji, divisionName);
                await message.ModifyAsync(emoji + $" Kierunek {divisionName} stworzony!");
            }
        }

        public static async Task createNewDivision(DiscordGuild guild, DiscordEmoji emoji, params string[] divisionNameParam)
        {
            string divisionName = string.Join(" ", divisionNameParam);
            Bot.DivisionChoosing.addDivisionName(divisionName);
            var divisionRole = await guild.CreateRoleAsync(divisionName, hoist: true);
            Bot.DivisionChoosing.Messages = await Bot.DivisionChoosing.CreateVerifyMessages();


            var divisionCategory = await guild.CreateChannelCategoryAsync(emoji + " " + divisionName + " " + emoji, createPermissions(divisionRole, guild));
            await guild.CreateTextChannelAsync($"{Emojis.Emoji[":loudspeaker:"]}ogłoszenia", divisionCategory);
            await guild.CreateTextChannelAsync($"{Emojis.Emoji[":speech_balloon:"]}czat", divisionCategory);
            await guild.CreateVoiceChannelAsync($"{Emojis.Emoji[":sound:"]}gadu gadu", divisionCategory);
        }
        private static List<DiscordOverwriteBuilder> createPermissions(DiscordRole divisionRole, DiscordGuild guild)
        {
            DiscordRole everyone = Roles.getRoleByName(guild.Roles.Values, "@everyone");
            var everyonePermissions = new DiscordOverwriteBuilder(everyone)
            {
                Denied = Permissions.All,
            };
            var specialPermissions = new DiscordOverwriteBuilder(divisionRole)
            {
                Allowed = Permissions.AccessChannels | Permissions.AddReactions | Permissions.ReadMessageHistory | Permissions.SendMessages | Permissions.Speak | Permissions.Stream | Permissions.UseVoice
            };
            return new List<DiscordOverwriteBuilder>() { everyonePermissions, specialPermissions };
        }

        [Command("modifyDivision")]
        public async Task modifyDivision(CommandContext ctx, string currentDivisionName, params string[] newDivisionNameParams)
        {
            string newDivisionName = string.Join(" ", newDivisionNameParams);

            if (Roles.hasNeededPermissions(ctx.Member, ctx.Guild.Roles.Values))
            {
                var message = await ctx.Channel.SendMessageAsync($" Kierunek {currentDivisionName} w trakcie modyfikacji...");

                //var channels = (await ctx.Guild.GetChannelsAsync()).ToList();
                //var divisionChannel = getDivisionChannel(channels, currentDivisionName);
                //if (divisionChannel is null)
                //{
                //    await message.ModifyAsync($" Kierunek {newDivisionName} nie istnieje!");
                //    return;
                //}
                //await divisionChannel.ModifyAsync(model => model.Name = newDivisionName);

                var divisionRole = Roles.getRoleByName(ctx.Guild.Roles.Values, currentDivisionName);
                await divisionRole.ModifyAsync(model => model.Name = newDivisionName);

                Bot.DivisionChoosing.changeDivisionMessage(ctx, currentDivisionName, newDivisionName);

                await message.ModifyAsync( $" Kierunek {newDivisionName} zmodyfikowany!");
            }
        }


        private DiscordChannel? getDivisionChannel(List<DiscordChannel> channels, string divisionName)
        {
            foreach (var channel in channels)
                if (channel.IsCategory && channel.Name.Contains(divisionName.ToUpper()))
                    return channel;
            return null;
        }

    }
    //channel.ModifyAsync(model => model.Name = currentDivisionName)


    public class DivisionConfig
    {
        public List<string> divisionsNames { get; set; }
    }

    public class DivisionChoosingAttributes                                         
    {
        public DiscordChannel Channel;
        public DiscordGuild Guild;
        public List<DiscordMessage> Messages;
        static List<string> divisionNames;
        public DivisionChoosingAttributes()
        {
            Guild = Bot.Client.Guilds[IDs.PG_GUILD];
            Channel = Guild.GetChannel(IDs.FIELD_OF_STUDY_CHANNEL);
            Messages = CreateVerifyMessages().Result;
        }

        private void getDivisionNames()
        {
            var divisionFile = File.ReadAllText("../../../Configs/DivisionConfig.json");
            var divisionCfg = JsonSerializer.Deserialize<DivisionConfig>(divisionFile);
            divisionNames = divisionCfg!.divisionsNames;
        }

        public void addDivisionName(string divisionName)
        {
            var path = "../../../Configs/DivisionConfig.json";
            var file = File.ReadAllText(path);
            var json = JsonSerializer.Deserialize<DivisionConfig>(file);

            JsonSerializerOptions opt = new JsonSerializerOptions()
            {
                AllowTrailingCommas = true,
                WriteIndented = true
            };

            json.divisionsNames.Add(divisionName);
            var txt = JsonSerializer.Serialize(json, opt);
            File.WriteAllText(path, txt);
        }

        public async Task<List<DiscordMessage>> CreateVerifyMessages()
        {
            getDivisionNames();

            var divisionMessages = await getClassMessages();
            var divisionMessagesNames = divisionMessages.Select(message => message.Embeds.FirstOrDefault()!.Title).ToList();

            await generateMissingClassMessages(divisionNames, divisionMessagesNames);

            divisionMessages = await getClassMessages();


            foreach (var message in divisionMessages)
                await message.CreateReactionAsync(Emojis.Emoji[":white_check_mark:"]);

            return divisionMessages;
        }
        private async Task<List<DiscordMessage>> getClassMessages()
        {
            var messages = await Channel.GetMessagesAsync();
            var studyFieldMessages = messages.Where(message => message.Author.IsBot).ToList();
            return studyFieldMessages;
        }

        private async Task generateMissingClassMessages(List<string> divisionNames, List<string> divisionMessagesNames)
        {
            foreach (var divisionName in divisionNames)
            {
                if (divisionMessagesNames.Any(name => name == divisionName))
                    continue;

                await createMissingFieldMessage(divisionName);
            }

        }
        private async Task createMissingFieldMessage(string fieldName)
        {
            await Channel.SendMessageAsync(Embeds.chooseStudyFieldEmbed(fieldName));
        }

        public void changeDivisionMessage(CommandContext ctx, string currentDivisionMessageName, string newDivisionMessageName)
        {
            foreach (var message in Messages)
            {
                var refreshedMessage = Channel.GetMessageAsync(message.Id).Result;
                if (refreshedMessage.Embeds.FirstOrDefault()!.Title == currentDivisionMessageName)
                    refreshedMessage.ModifyAsync((DiscordEmbed)(Embeds.chooseStudyFieldEmbed(newDivisionMessageName)));
                divisionNames.Remove(currentDivisionMessageName);
                divisionNames.Add(newDivisionMessageName);
            }
        }

        public void choosedDivisionMessage(MessageReactionAddEventArgs e)
        {
            if (e.User.IsBot != true && Messages.Any(message => message == e.Message))
            {
                var message = e.Channel.GetMessageAsync(e.Message.Id).Result;
                var divisionName = message.Embeds[0].Title;
                var divisionRole = Roles.getRoleByName(e.Guild.Roles.Values, divisionName);

                var member = ((DiscordMember)(e.User));

                revokeAllDivisionRoles(member, message);
                removeAllMemberDivisionReactions(divisionName, member, e.Channel);
                member.GrantRoleAsync(divisionRole);
            }
        }
        private void removeAllMemberDivisionReactions(string divisionName, DiscordMember member, DiscordChannel channel)
        {
            var messages = channel.GetMessagesAsync().Result.ToList();
            foreach (var message in messages)
                if (message.Author.IsBot && message.Embeds[0].Title != divisionName)
                    message.DeleteReactionAsync(Emojis.Emoji[":white_check_mark:"], member);
        }
        private void revokeAllDivisionRoles(DiscordMember member, DiscordMessage message)
        {
            foreach (var role in member.Roles)
                if (divisionNames.Contains(role.Name))
                    member.RevokeRoleAsync(role);
        }

    }
}

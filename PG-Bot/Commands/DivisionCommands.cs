using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
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
        [Command("refreshStats")]
        public async Task refreshStats(CommandContext ctx)
        {
            var mes = await ctx.Channel.SendMessageAsync("Odświeżanie statystyk...");
            await refreshStatsChannel(ctx);
            await mes.ModifyAsync("Statystyki zostały odświeżone!");
        }

        [Command("query")]
        public async Task makeQuery(CommandContext ctx)
        {
            if(!Roles.hasNeededPermissions(ctx.Member, ctx.Guild.Roles.Values)) 
                return;

            var membersCount = (await ctx.Guild.GetAllMembersAsync()).Count();
            await ctx.Guild.GetChannel(1002639288116183140).SendMessageAsync("Ilość członków: " + membersCount.ToString());

            //nie do końca działa
            //await assingDepartmentRoles(ctx);
        }

        //private async Task changePermissionsOnDivisionsChannels(CommandContext ctx)
        //{
        //    var channels = (await ctx.Guild.GetChannelsAsync()).ToList();
        //    foreach (var channel in channels)
        //    {
        //        var category = await Channels.getCategoryByName(ctx.Guild, channel.Name);

        //        if (category is null)
        //        {
        //            await ctx.Channel.SendMessageAsync($"Nie ma kategorii {channel.Name}!");
        //            continue;
        //        }

        //        var divisionName = getRightDivisonName(category);

        //        if (divisionName is null)
        //        {
        //            await ctx.Channel.SendMessageAsync($"Nie ma kategorii {channel.Name}!");
        //            continue;
        //        }
                
        //        if (category.IsCategory)
        //        {
        //            var childChannels = category.Children.ToList();

        //            DiscordRole divisionRole;
        //            if (divisionName is not null)
        //            {
        //                divisionRole = Roles.getRoleByName(ctx.Guild.Roles.Values, divisionName);
        //                await category.ModifyAsync(model => model.PermissionOverwrites = createPermissionsFix(divisionRole, ctx.Guild));
        //                foreach (var childChannel in childChannels)
        //                {
        //                    await childChannel.ModifyAsync(model =>
        //                        model.PermissionOverwrites = createPermissionsFix(divisionRole, ctx.Guild));
        //                }
        //            }

        //        }
        //    }
        //    await ctx.Channel.SendMessageAsync("Zmieniono!");
        //}
        

        //private static List<DiscordOverwriteBuilder> createPermissionsFix(DiscordRole divisionRole, DiscordGuild guild)
        //{
        //    DiscordRole everyone = Roles.getRoleByName(guild.Roles.Values, "@everyone");
        //    var everyonePermissions = new DiscordOverwriteBuilder(everyone)
        //    {
        //        Denied = Permissions.AccessChannels
        //    };
        //    var specialPermissions = new DiscordOverwriteBuilder(divisionRole)
        //    {
        //        Allowed = Permissions.AccessChannels,
        //    };
        //    return new List<DiscordOverwriteBuilder>() { everyonePermissions, specialPermissions };
        //}

        private string? getRightDivisonName(DiscordChannel channel)
        {
            foreach (var divisionName in Bot.DivisionChoosing.divisionNames)
                if (channel.Name.Contains(divisionName))
                    return divisionName;
            return null;
        }


        private async Task refreshStatsChannel(CommandContext ctx)
        {
            var statsChannelMessages = ctx.Guild.GetChannel(IDs.STATS_CHANNEL).GetMessagesAsync().Result.ToList();
            foreach (var statMessage in statsChannelMessages)
            {
                if (statMessage.Embeds[0].Title.Contains("KIERUNKI"))
                    await statMessage.ModifyAsync((DiscordEmbed)Embeds.divisionsStatsEmbed());
            }
        }

        private async Task assingDepartmentRoles(CommandContext ctx)
        {
            var members = (await ctx.Guild.GetAllMembersAsync()).ToList();
            foreach (var member in members)
            {
                var departmentName = await getMemberDepartment(ctx, member.Id);
                bool containsDepartment = Bot.DivisionChoosing.departmentsNames.Contains(departmentName);
                Console.WriteLine(member.Nickname);
                if (departmentName is null || !containsDepartment)
                    continue;
                Console.WriteLine("////" + departmentName);
                var departmentRole = Roles.getRoleByName(ctx.Guild.Roles.Values, departmentName);
                await member.GrantRoleAsync(departmentRole);
            }
        }

        private async Task<string?> getMemberDepartment(CommandContext ctx, ulong personID)
        {
            var member = await ctx.Guild.GetMemberAsync(personID);
            foreach (var role in member.Roles.ToList())
            {
                if (!role.Name.Contains("("))
                    continue;
                return getDepartmentName(role.Name);
            }
            return null;
        }

        public static string getDepartmentName(string role)
        {
            var startingIndex = role.IndexOf("(");
            var endIndex = role.IndexOf(")");
            if (startingIndex >= endIndex)
                return null;
            var nameLength = endIndex - startingIndex + 1;
            string name = role.Substring(startingIndex, nameLength);
            return name;
        }

        //[Command("createDivision")]
        public async Task createDivision(CommandContext ctx, DiscordEmoji emoji, params string[] divisionNameParams)
        {
            if (!Roles.hasNeededPermissions(ctx.Member, ctx.Guild.Roles.Values)) 
                return;

            string divisionName = string.Join(" ", divisionNameParams);
            var message = await ctx.Channel.SendMessageAsync(emoji + $" Kierunek {divisionName} w trakcie tworzenia...");

            await createNewDivision(ctx.Guild, emoji, divisionName);

            await message.ModifyAsync(emoji + $" Kierunek {divisionName} stworzony!");
        }

        private static async Task createNewDivision(DiscordGuild guild, DiscordEmoji emoji, params string[] divisionNameParam)
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
                Denied = Permissions.AccessChannels
            };
            var specialPermissions = new DiscordOverwriteBuilder(divisionRole)
            {
                Allowed = Permissions.AccessChannels,
            };
            return new List<DiscordOverwriteBuilder>() { everyonePermissions, specialPermissions };
        }

        //[Command("modifyDivision")]
        public async Task modifyDivision(CommandContext ctx, string currentDivisionName, DiscordEmoji divisionEmoji, params string[] newDivisionNameParams)
        {
            if (!Roles.hasNeededPermissions(ctx.Member, ctx.Guild.Roles.Values))
                return;
            
            string newDivisionName = string.Join(" ", newDivisionNameParams);
            var message = await ctx.Channel.SendMessageAsync($" Kierunek {currentDivisionName} w trakcie modyfikacji...");

            await changeDivisionChannelName(ctx, message, currentDivisionName, divisionEmoji, newDivisionName);
            await changeDivisionRoleName(ctx, currentDivisionName, newDivisionName);
            Bot.DivisionChoosing.changeDivisionMessage(ctx, currentDivisionName, newDivisionName);

            await message.ModifyAsync( $" Kierunek {newDivisionName} zmodyfikowany!");
        }

        private async Task changeDivisionChannelName(CommandContext ctx, DiscordMessage message, string oldName, DiscordEmoji divisionEmoji, string newName)
        {
            var divisionChannel = await Channels.getCategoryByName(ctx.Guild, oldName);
            if (divisionChannel is null)
            {
                await message.ModifyAsync($" Kierunek {newName} nie istnieje!");
                return;
            }
            await divisionChannel.ModifyAsync(model => model.Name = divisionEmoji +  " " +  newName + " "+ divisionEmoji);

        }

        private async Task changeDivisionRoleName(CommandContext ctx, string oldName, string newName)
        {
            var divisionRole = Roles.getRoleByName(ctx.Guild.Roles.Values, oldName);
            await divisionRole.ModifyAsync(model => model.Name = newName);
        }

    }

    public class Departaments
    {
        public List<string> WA { get; set; }
        public List<string> WCh { get; set; }
        public List<string> WEiA { get; set; }
        public List<string> WETI { get; set; }
        public List<string> WFTiMS { get; set; }
        public List<string> WILiŚ { get; set; }
        public List<string> WIMiO { get; set; }
        public List<string> WZiE { get; set; }
        public List<string> crossDepartment { get; set; }
    }

    public class DivisionConfig
    {
        public List<string> divisionsNames { get; set; }
        public List<string> departmentsNames { get; set; }
        public Departaments departaments { get; set; }

    }

    public class DivisionChoosingAttributes                                         
    {
        public DiscordChannel Channel;
        public DiscordGuild Guild;
        public List<DiscordMessage> Messages;
        public List<string> divisionNames;
        public List<string> departmentsNames;
        public Departaments departaments;

        public DivisionChoosingAttributes()
        {
            getDivisionNames();
            Guild = Bot.Client.Guilds[IDs.PG_GUILD];
            Channel = Guild.GetChannel(IDs.FIELD_OF_STUDY_CHANNEL);
            Messages = CreateVerifyMessages().Result;
        }

        private void getDivisionNames()
        {
            var divisionFile = File.ReadAllText("../../../Configs/DivisionConfig.json");
            var divisionCfg = JsonSerializer.Deserialize<DivisionConfig>(divisionFile);
            divisionNames = divisionCfg!.divisionsNames;
            departmentsNames = divisionCfg!.departmentsNames;
            departaments = divisionCfg!.departaments;
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

            var divisionMessages = await getDivisionsMessages();
            var divisionMessagesNames = divisionMessages.Select(message => message.Embeds.FirstOrDefault()!.Title).ToList();

            await generateMissingClassMessages(divisionNames, divisionMessagesNames);

            divisionMessages = await getDivisionsMessages();


            foreach (var message in divisionMessages)
            {
                if (message.Reactions.Count == 0)
                    await message.CreateReactionAsync(Emojis.Emoji[":white_check_mark:"]);
            }

            return divisionMessages;
        }
        private async Task<List<DiscordMessage>> getDivisionsMessages()
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


        public DiscordRole? getMemberDivisionRole(MessageReactionAddEventArgs e)
        {
            var roles = ((DiscordMember)(e.User)).Roles;
            foreach (var role in roles)
                foreach (var divisionName in divisionNames)
                    if (role.Name.Equals(divisionName, StringComparison.OrdinalIgnoreCase))
                        return role;
            return null;
        }

        public async Task choosedDivisionMessage(MessageReactionAddEventArgs e)
        {
            if (isReactorBot(e) || e.Channel != Channel || Messages.All(message => message != e.Message)) 
                return;

            var currentDivisionRole = getMemberDivisionRole(e);

            var message = e.Channel.GetMessageAsync(e.Message.Id).Result;
            var divisionName = message.Embeds[0].Title;
            var departmentName = DivisionCommands.getDepartmentName(divisionName);
            var divisionRole = Roles.getRoleByName(e.Guild.Roles.Values, divisionName);
            var departmentRole = Roles.getRoleByName(e.Guild.Roles.Values, departmentName);

            var member = ((DiscordMember)(e.User));

            revokeAllDivisionRoles(member);
            await removeAllMemberDivisionReactions(divisionName, member, e.Channel, currentDivisionRole);
            await member.GrantRoleAsync(divisionRole);
            if(departmentsNames.Contains(departmentName, StringComparer.OrdinalIgnoreCase))
                await member.GrantRoleAsync(departmentRole);

            await refreshStatsChannel();
            await Bot.DivisionChoosing.Guild.Channels[IDs.DIVISION_LOG_CHANNEL].SendMessageAsync($"Użytkownik *{((DiscordMember)(e.User)).DisplayName}* wybrał *{divisionName}*");
        }

        public async Task refreshStatsChannel()
        {
            var statsChannelMessages = Guild.GetChannel(IDs.STATS_CHANNEL).GetMessagesAsync().Result.ToList();
            foreach (var statMessage in statsChannelMessages)
            {
                if (statMessage.Embeds[0].Title.Contains("KIERUNKI"))
                    await statMessage.ModifyAsync((DiscordEmbed)Embeds.divisionsStatsEmbed());
            }
        }

        private bool isReactorBot(MessageReactionAddEventArgs e) { return e.User.IsBot; }
        private async Task removeAllMemberDivisionReactions(string divisionName, DiscordMember member, DiscordChannel channel, DiscordRole? currentDivisionRole)
        {
            if (currentDivisionRole is null)
                return;

            var messages = channel.GetMessagesAsync().Result.ToList();
            foreach (var message in messages)
                if (message.Author.IsBot && message.Embeds[0].Title == currentDivisionRole.Name)
                    await message.DeleteReactionAsync(Emojis.Emoji[":white_check_mark:"], member);
        }
        private void revokeAllDivisionRoles(DiscordMember member)
        {
            foreach (var role in member.Roles)
                if (divisionNames.Contains(role.Name, StringComparer.OrdinalIgnoreCase) || departmentsNames.Contains(role.Name, StringComparer.OrdinalIgnoreCase))
                    member.RevokeRoleAsync(role);
        }
    }
}

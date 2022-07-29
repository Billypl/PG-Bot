//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;
//using DSharpPlus;
//using DSharpPlus.CommandsNext;
//using DSharpPlus.Entities;
//using DSharpPlus.EventArgs;
//using PG_Bot.Commands;
//using PG_Bot.Data;
//using PG_Bot.Helper;
//using PG_Bot.Scripts;

//namespace PG_Bot.Entities
//{

//    public class VotingChannel
//    {
//        public DiscordGuild Guild;
//        public DiscordChannel Channel;

//        public List<DiscordMessage> VoteMessages
//        {
//            get
//            {
//                var IDs = getVoteMessagesIDs();
//                foreach (var ID in IDs)
//                {
                    
//                }
//            }
//            set
//            {

//            }
//        }
//        public List<ulong> MessageIDs;
//        private const string IDsPath = "../../../Entities/VotingChannels.json";

//        public VotingChannel(ulong channelID)
//        {
//            Guild = Bot.Client.Guilds[IDs.PG_GUILD];
//            Channel = Guild.GetChannel(channelID);
//            MessageIDs = getVoteMessagesIDs();
//            VoteMessages = CreateVerifyMessages().Result;
//        }

//        public List<ulong> getVoteMessagesIDs()
//        {
//            var file = File.ReadAllText(IDsPath);
//            var IDs = JsonSerializer.Deserialize<List<ulong>>(file);
//            return MessageIDs;
//        }

//        public void addVoteMessageID(ulong voteMessageID)
//        {
//            var json = getVoteMessagesIDs();

//            json.Add(voteMessageID);
//            var txt = JsonSerializer.Serialize(json, new JsonSerializerOptions() { WriteIndented = true });
//            File.WriteAllText(IDsPath, txt);
//        }



//        public async Task<List<DiscordMessage>> CreateVerifyMessages()
//        {
//            getVoteMessagesIDs();

//            var divisionMessages = await getDivisionMessages();
//            var divisionMessagesNames = divisionMessages.Select(message => message.Embeds.FirstOrDefault()!.Title).ToList();

//            await generateMissingClassMessages(divisionNames, divisionMessagesNames);

//            divisionMessages = await getDivisionMessages();


//            foreach (var message in divisionMessages)
//                await message.CreateReactionAsync(Emojis.Emoji[":white_check_mark:"]);

//            return divisionMessages;
//        }
//        private async Task<List<DiscordMessage>> getDivisionMessages()
//        {
//            var messages = await Channel.GetMessagesAsync();
//            var studyFieldMessages = messages.Where(message => message.Author.IsBot).ToList();
//            return studyFieldMessages;
//        }

//        private async Task generateMissingClassMessages(List<string> divisionNames, List<string> divisionMessagesNames)
//        {
//            foreach (var divisionName in divisionNames)
//            {
//                if (divisionMessagesNames.Any(name => name == divisionName))
//                    continue;

//                await createMissingFieldMessage(divisionName);
//            }

//        }
//        private async Task createMissingFieldMessage(string fieldName)
//        {
//            await Channel.SendMessageAsync(Embeds.chooseStudyFieldEmbed(fieldName));
//        }

//        public void changeDivisionMessage(CommandContext ctx, string currentDivisionMessageName, string newDivisionMessageName)
//        {
//            foreach (var message in VoteMessages)
//            {
//                var refreshedMessage = Channel.GetMessageAsync(message.Id).Result;
//                if (refreshedMessage.Embeds.FirstOrDefault()!.Title == currentDivisionMessageName)
//                    refreshedMessage.ModifyAsync((DiscordEmbed)(Embeds.chooseStudyFieldEmbed(newDivisionMessageName)));
//                divisionNames.Remove(currentDivisionMessageName);
//                divisionNames.Add(newDivisionMessageName);
//            }
//        }

//        public void choosedDivisionMessage(MessageReactionAddEventArgs e)
//        {
//            if (isReactorBot(e) || VoteMessages.All(message => message != e.Message))
//                return;

//            var message = e.Channel.GetMessageAsync(e.Message.Id).Result;
//            var divisionName = message.Embeds[0].Title;
//            var divisionRole = Roles.getRoleByName(e.Guild.Roles.Values, divisionName);

//            var member = ((DiscordMember)(e.User));

//            revokeAllDivisionRoles(member, message);
//            removeAllMemberDivisionReactions(divisionName, member, e.Channel);
//            member.GrantRoleAsync(divisionRole);

//        }
//        private bool isReactorBot(MessageReactionAddEventArgs e) { return e.User.IsBot; }
//        private void removeAllMemberDivisionReactions(string divisionName, DiscordMember member, DiscordChannel channel)
//        {
//            var messages = channel.GetMessagesAsync().Result.ToList();
//            foreach (var message in messages)
//                if (message.Author.IsBot && message.Embeds[0].Title != divisionName)
//                    message.DeleteReactionAsync(Emojis.Emoji[":white_check_mark:"], member);
//        }
//        private void revokeAllDivisionRoles(DiscordMember member, DiscordMessage message)
//        {
//            foreach (var role in member.Roles)
//                if (divisionNames.Contains(role.Name))
//                    member.RevokeRoleAsync(role);
//        }
//    }
//}

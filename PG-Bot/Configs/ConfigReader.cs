using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Interactivity;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace PG_Bot.Configs
//TODO: extract token to different file and add it to .gitignore
{
    public static class ConfigReader
    {
        public static void readConfigs()
        {
            JsonSerializerOptions options = createJsonOptions();

            ConfigManager.clientConfig = readClientConfig(options);
            ConfigManager.commandsConfig = readCommandsConfig();
            ConfigManager.interactivityConfig = readInteractivityConfig();
        }
        private static JsonSerializerOptions createJsonOptions()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }
        private static DiscordConfiguration readClientConfig(JsonSerializerOptions options)
        {
            string clientFile = File.ReadAllText("../../../Configs/ClientConfig.json");
            var clientCfg = JsonSerializer.Deserialize<DiscordConfiguration>(clientFile, options);
            clientCfg.Token = File.ReadAllText("../../../Configs/SecretToken.txt");
            return clientCfg;
        }

        private static CommandsNextConfiguration readCommandsConfig()
        {
            string commandsFile = File.ReadAllText("../../../Configs/CommandsConfig.json");
            var commandsCfg = JsonSerializer.Deserialize<CommandsNextConfiguration>(commandsFile);
            // prefixes don't deserialize properly, manual deserialization needed
            commandsCfg.StringPrefixes = getStringPrefixes(commandsFile); 
            return commandsCfg;
        }
        private static IEnumerable<string> getStringPrefixes(string commandsFile)
        {
            JsonNode commandNode = JsonNode.Parse(commandsFile);
            JsonNode prefixNode = commandNode["StringPrefixes"];
            string prefixes = prefixNode.ToJsonString();
            return JsonSerializer.Deserialize<IEnumerable<string>>(prefixes);
        }

        private static InteractivityConfiguration readInteractivityConfig()
        {
            string interactivityFile = File.ReadAllText("../../../Configs/InteractivityConfig.json");
            var interactivityCfg = JsonSerializer.Deserialize<InteractivityConfiguration>(interactivityFile);
            return interactivityCfg;
        }
    }
}

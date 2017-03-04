using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace UFWDiscordBot
{
    class UFWBot
    {
        DiscordClient _client;
        CommandService commands;

        public UFWBot()
        {
            _client = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = Log;
            });

            _client.UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.AllowMentionPrefix = true;
            });

            commands = _client.GetService<CommandService>();
            CommandsList();

            _client.ExecuteAndWait(async () => {
                await _client.Connect("Mjg2OTU1MzQ0MzI2ODE5ODUx.C5odcw.CxZFfrN4xHQ4TrbJfkvN1DpLM0s", TokenType.Bot);
            });
        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private void CommandsList()
        {
            say();
        }

        private void say()
        {
            commands.CreateCommand("say")
                .Description("Says Input")
                .Parameter("Message", ParameterType.Optional)
                .Do(async e =>
                {
                    await e.Channel.SendMessage($"{e.User.Name} says {e.GetArg("Message")}");
                });
        }
    }
}

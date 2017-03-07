using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Audio;

namespace UFWDiscordBot
{
    class UFWBot
    {
        DiscordClient _client;
        CommandService commands;
        Channel voiceChannel;
        bool inVoiceChannel;

        public UFWBot()
        {
            Initializer();

            commands = _client.GetService<CommandService>();
            CommandsList();

            _client.ExecuteAndWait(async () => {
                await _client.Connect("Mjg2OTU1MzQ0MzI2ODE5ODUx.C5odcw.CxZFfrN4xHQ4TrbJfkvN1DpLM0s", TokenType.Bot);
            });
        }

        private void Initializer()
        {
            inVoiceChannel = false;

            _client = new DiscordClient(x =>
            {
                x.LogLevel = LogSeverity.Info;
                x.LogHandler = Log;
            });

            _client.UsingAudio(x =>
            {
                x.Mode = AudioMode.Outgoing;
            });

            _client.UsingCommands(x =>
            {
                x.PrefixChar = '!';
                x.AllowMentionPrefix = true;
            });
        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private void CommandsList()
        {
            say();
            join();
            leave();
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

        private void join()
        {
            commands.CreateCommand("join")
                .Description("Joins the users voice channel")
                .Do(async e =>
                {
                    if (e.User.VoiceChannel != null)
                    {
                        voiceChannel = e.User.VoiceChannel;
                        await _client.GetService<AudioService>()
                            .Join(voiceChannel);
                        inVoiceChannel = true;
                    }
                    else
                        await e.Channel.SendMessage($"You must be in a voice channel.");
                });
        }

        private void leave()
        {
            commands.CreateCommand("leave")
                .Description("Leaves the voice channel")
                .Do(async e =>
                {
                    await _client.GetService<AudioService>()
                        .Leave(voiceChannel);
                    inVoiceChannel = false;
                });
        }
    }
}

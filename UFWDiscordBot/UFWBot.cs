using System;
using System.Threading;
using System.Diagnostics;
using Discord;
using Discord.Commands;
using Discord.Audio;
using NAudio;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using System.IO;
using System.Text.RegularExpressions;
using VideoLibrary;

namespace UFWDiscordBot
{
    class UFWBot
    {

        static DiscordClient _client; 
        CommandService commands;
        Channel voiceChannel;
        IAudioClient _vClient;
        bool inVoiceChannel;
        bool player;

        internal static AudioService Audio => _client.GetService<AudioService>();

        public UFWBot()
        {
            Initializer();

            commands = _client.GetService<CommandService>();
            EventList();
            CommandsList();

            _client.ExecuteAndWait(async () => {
                await _client.Connect("Mjg2OTU1MzQ0MzI2ODE5ODUx.C5odcw.CxZFfrN4xHQ4TrbJfkvN1DpLM0s", TokenType.Bot);
            });
        }

        private void Initializer()
        {
            inVoiceChannel = false;
            player = true;

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

        private void EventList()
        {
            userJoined();
            userLeft();
            userNameUpdated();
        }

        private void userJoined()
        {
            _client.UserJoined += async (s, e) => 
            {
                await e.Server.DefaultChannel.SendMessage($"{e.User.Name} joined the server.");
            };
        }

        private void userLeft()
        {
            _client.UserLeft += async (s, e) =>
            {
                await e.Server.DefaultChannel.SendMessage($"{e.User.Name} left the server.");
            };
        }

        private void userNameUpdated()
        {
            _client.UserUpdated += async (s, e) =>
            {
                if (e.Before.Nickname != e.After.Nickname)
                    await e.Server.DefaultChannel.SendMessage($"{e.Before.Name} has changed to {e.After.Nickname}");
            };
        }

        private void CommandsList()
        {
            say();
            join();
            leave();
            play();
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
                        _vClient = await _client.GetService<AudioService>()
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
                    await _vClient.Disconnect();

                });
        }

        private void play()
        {
            commands.CreateCommand("play")
                .Description("Play")
                .Parameter("URL", ParameterType.Required)
                .Do(async e =>
                {
                    Message[] messagesToDelete;
                    messagesToDelete = await e.Channel.DownloadMessages(1);
                    await e.Channel.DeleteMessages(messagesToDelete);
                    SendAudio(e.GetArg("URL"));
                });
        }

        static WaveStream Reader(string file)
        {
            for (byte i = 3; i != 0; --i) try
                {
                    return Path.GetExtension(file) == ".ogg"
                            ? (WaveStream)new NAudio.Vorbis.VorbisWaveReader(file)
                            : new MediaFoundationReader(file);
                }
                catch { }
            return null;
        }

        public void SendAudio(string filePath, Func<bool> cancel = null)
        {
            var video = YouTube.Default.GetVideo(filePath);
            var musicReader = Reader(video.Uri);

            if (musicReader == null)
            {
                return;
            }
            var channels = UFWBot.Audio.Config.Channels;
            var outFormat = new WaveFormat(48000, 16, channels);
            using (var resampler = new MediaFoundationResampler(musicReader, outFormat) { ResamplerQuality = 60 })
            {
                int blockSize = outFormat.AverageBytesPerSecond; // 1 second
                byte[] buffer = new byte[blockSize];
                while (cancel == null || !cancel())
                {
                    bool end = musicReader.Position + blockSize > musicReader.Length; // Stop at the end, work around the bug that has it Read twice.
                    if (resampler.Read(buffer, 0, blockSize) <= 0) break; // Break on failed read.
                    _vClient.Send(buffer, 0, blockSize);
                    if (end) break;
                }
            }
            musicReader.Dispose();

        }
    }
}

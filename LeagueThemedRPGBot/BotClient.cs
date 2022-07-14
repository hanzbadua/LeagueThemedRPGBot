using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LeagueThemedRPGBot.Commands;
using LeagueThemedRPGBot.Game;

namespace LeagueThemedRPGBot
{
    // Todo: Reimplement inventory methods to use reactions instead of responses, new weapons + armor, new init (starting classes + items)
    // actual balance

    // enumerates skills in another function, cleanup combat code to be more split into functions and modularized
    public class BotClient
    {
        private static async Task Main() => await new BotClient().RunBotAsync();

        public readonly EventId BotLoggingEvent = new (0, "BotLogging");
        public DiscordClient Client { get; set; }
        public CommandsNextExtension Commands { get; set; }

        public async Task RunBotAsync() {

            string token = await File.ReadAllTextAsync("token.txt");

            // create a bot client config incl. token
            var config = new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug,
                Intents = DiscordIntents.AllUnprivileged
            };

            // instantiate our bot client
            Client = new DiscordClient(config);

            // allow our bot client to be 'interactive' (use respond-by-message, respond-by-reaction)
            Client.UseInteractivity(new InteractivityConfiguration
            {
                PollBehaviour = PollBehaviour.DeleteEmojis,
                Timeout = TimeSpan.FromMinutes(1)
            });

            // register events related to our client
            Client.Ready += OnReady;
            Client.GuildAvailable += OnGuildAvailable;
            Client.ClientErrored += OnClientError;

            // dependency injection
            var services = new ServiceCollection()
                .AddSingleton<PlayerData>()
                .AddSingleton<Data>()
                .AddSingleton<Random>()
                .BuildServiceProvider();

            // create command configuration
            var commandConfig = new CommandsNextConfiguration {
                StringPrefixes = new[] { "$" },
                EnableDms = true,
                EnableMentionPrefix = true,
                Services = services
            };

            // register command config to our bot client
            Commands = Client.UseCommandsNext(commandConfig);

            // register events related to our commands
            Commands.CommandExecuted += OnCommandExecute;
            Commands.CommandErrored += OnCommandError;

            // register commands
            Commands.RegisterCommands<MainCommands>();
            Commands.RegisterCommands<DebugCommands>();
            
            // useless
            Commands.RegisterCommands<UselessAmusementCommands>();

            // use custom help formatter
            Commands.SetHelpFormatter<CustomHelpFormatter>();

            // connect the client + log in
            await Client.ConnectAsync();

            // run the bot client permanently in background, until close
            await Task.Delay(-1);
        }

        private async Task OnReady(DiscordClient dc, ReadyEventArgs args)
        {
            dc.Logger.LogInformation(BotLoggingEvent, "Client is ready to process events");
            return;
        }

        private async Task OnGuildAvailable(DiscordClient dc, GuildCreateEventArgs args)
        {
            dc.Logger.LogInformation(BotLoggingEvent, $"Available guild: {args.Guild.Name}");
            return;
        }

        private async Task OnClientError(DiscordClient dc, ClientErrorEventArgs args)
        {
            dc.Logger.LogError(BotLoggingEvent, args.Exception, "Exception occured");
            return;
        }

        private async Task OnCommandExecute(CommandsNextExtension cmds, CommandExecutionEventArgs args)
        {
            args.Context.Client.Logger.LogInformation(BotLoggingEvent, $"{args.Context.User.Username} successfully executed '{args.Command.QualifiedName}'");
            return;
        }

        private async Task OnCommandError(CommandsNextExtension cmds, CommandErrorEventArgs args)
        {
            args.Context.Client.Logger.LogError(BotLoggingEvent, $"{args.Context.User.Username} tried executing '{args.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {args.Exception.GetType()}: {args.Exception.Message ?? "<no message>"}", DateTime.Now);
            // command doesn't exist
            if (args.Exception is CommandNotFoundException)
            {
                await args.Context.Message.CreateReactionAsync(DiscordEmoji.FromName(cmds.Client, ":grey_question:"));
                /*
                await args.Context.RespondAsync(new DiscordEmbedBuilder()
                {
                    Title = "Command not found", 
                    Description = "This command appears to be invalid", 
                    Color = new DiscordColor(0xFF0000) // red
                });
                */
            }
            // permissions not valid
            else if (args.Exception is ChecksFailedException)
            {
                await args.Context.RespondAsync(new DiscordEmbedBuilder()
                {
                    Title = "Access denied",
                    Description = "You do not have the permissions required to use this command",
                    Color = new DiscordColor(0xFF0000) // red
                });
            }
        }
    }
}

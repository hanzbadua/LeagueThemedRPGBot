using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;

namespace LeagueThemedRPGBot
{
    public class CustomHelpFormatter : BaseHelpFormatter
    {
        // color used is blurple
        private readonly DiscordEmbedBuilder Message = new() { Color = new DiscordColor(88, 101, 242) };

        public CustomHelpFormatter(CommandContext ctx) : base(ctx)
        {
        }

        public override BaseHelpFormatter WithCommand(Command cmd)
        {
            if (cmd is CommandGroup)
            {
                Message.WithTitle($"Group `{cmd.Name}`")
                    .WithDescription(cmd.Description);
                return this;
            }

            Message.WithTitle($"Command `{cmd.Name}`")
                .WithDescription(cmd.Description);

            if (cmd.Overloads.Any())
            {
                string args = string.Empty;

                foreach (var i in cmd.Overloads)
                {
                    if (i.Arguments.Count == 0) continue;
                    foreach (var x in i.Arguments.Select(a => $"`{a.Name}` ({a.Description}){Environment.NewLine}"))
                    {
                        args += $"{x} ";
                    }
                }

                if (!string.IsNullOrEmpty(args))
                    Message.AddField("Arguments", args);
            }

            if (cmd.Aliases.Any())
            {
                string aliasesList = string.Empty;

                foreach (var i in cmd.Aliases)
                {
                    aliasesList += $"`{i}` ";
                }

                if (!string.IsNullOrEmpty(aliasesList))
                    Message.AddField("Aliases", aliasesList);
            }

            return this;
        }
        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            Message.WithTitle("Available commands: ");

            string cmds = string.Empty;
            string groups = string.Empty;

            foreach (var i in subcommands)
            {
                if (i is CommandGroup)
                {
                    groups += $"`{i.Name}` ";
                    continue;
                }

                cmds += $"`{i.Name}` ";
            }

            Message.WithDescription(cmds).AddField("Available groups", groups);

            return this;
        }

        public override CommandHelpMessage Build()
        {
            return new CommandHelpMessage(null, Message.Build());
        }
    }
}

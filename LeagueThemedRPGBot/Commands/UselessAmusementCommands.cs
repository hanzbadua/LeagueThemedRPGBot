using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace LeagueThemedRPGBot.Commands
{
    public class UselessAmusementCommands : BaseCommandModule
    {
        [Command("penis"), Hidden] public async Task Penis(CommandContext ctx) => await ctx.RespondAsync("hanz loves penis, yummy!");
        [Command("Karmo"), Hidden] public async Task Karmo(CommandContext ctx) => await ctx.RespondAsync("OO OO AA AA IM MONKEY!");
    }
}

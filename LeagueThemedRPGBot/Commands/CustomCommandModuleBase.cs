using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using LeagueThemedRPGBot.Game;

namespace LeagueThemedRPGBot.Commands
{
    public class CustomCommandModuleBase : BaseCommandModule
    {
        // not a command
        // append 'if (!await InitCheck(ctx)) return;' to the prologue of all game-related methods
        protected async Task<bool> InitCheck(CommandContext ctx)
        {
            if (!Globals.PlayerIsAlreadyInitialized(ctx.User.Id))
            {
                await ctx.RespondAsync("You don't seem to exist in the player database - have you initialized? `$init`");
                return false;
            }

            return true;
        }

        // Remember to InitCheck() before calling this!
        protected async Task<bool> InventoryIsEmpty(CommandContext ctx)
        {
            // Inventory is empty check
            if (!Globals.PlayerData[ctx.User.Id].Inventory.Any())
            {
                await ctx.RespondAsync("Your inventory is empty!");
                return true;
            }

            return false;
        }

        protected readonly DiscordColor DefBlue = new DiscordColor(0, 191, 255);
    }
}

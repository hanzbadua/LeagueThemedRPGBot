using System.Text.Json;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using LeagueThemedRPGBot.Game;

namespace LeagueThemedRPGBot.Commands
{
    [Group("Debug"), Description("Debug commands, not useable by normal users"), Hidden, RequireOwner]
    public class DebugCommands : CustomCommandModuleBase
    {
        [Command("exit"), Description("Safely exit the bot client and save data"), Hidden, RequireOwner]
        public async Task Exit(CommandContext ctx)
        {
            await ctx.RespondAsync($"Exiting safely and saving data...");
            Environment.Exit(0);
        }

        [Command("dumpdata"), Description("Safely exit the bot client and save data"), Hidden, RequireOwner]
        public async Task DumpData(CommandContext ctx)
        {
            var i = JsonSerializer.Serialize(Globals.PlayerData);
            await ctx.RespondAsync(i);
        }

        [Command("givetestitem"), Description("give test item"), Hidden, RequireOwner]
        public async Task GiveTestItem(CommandContext ctx)
        {
            if (!await InitCheck(ctx)) return;

            var i = new Item
            {
                Name = "Test Item",
                Description = "Idk",
                Rarity = ItemRarity.Legendary,
                Type = ItemType.Valuable
            };

            Globals.PlayerData[ctx.User.Id].Inventory.Add(i);

            await ctx.RespondAsync("Test item added");
        }

        [Command("givelongsword"), Description("give long sword"), Hidden, RequireOwner]
        public async Task GiveLongSword(CommandContext ctx)
        {
            if (!await InitCheck(ctx)) return;

            var i = new Item
            {
                Name = "Long Sword",
                Description = "It hurts.",
                Rarity = ItemRarity.Basic,
                Type = ItemType.Weapon,
                Stats = new()
                {
                    AttackDamage = 10
                }
            };

            Globals.PlayerData[ctx.User.Id].Inventory.Add(i);

            await ctx.RespondAsync("Long sword added");
        }

        [Command("ItemStructureGen"), Description("Generate item structure and send msg as json"), Hidden, RequireOwner]
        public async Task ItemStructureGen(CommandContext ctx)
        {

            var i = new Item
            {
                Name = "Name here",
                Description = "Description here",
                Rarity = ItemRarity.Basic,
                Type = ItemType.Weapon,
                Stats = new ItemStats
                {
                }
            };

            var j = JsonSerializer.Serialize(i);

            await ctx.RespondAsync(j);
        }
    }
}

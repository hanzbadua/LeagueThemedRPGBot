using System.Text.Json;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using LeagueThemedRPGBot.Game;

namespace LeagueThemedRPGBot.Commands
{
    // Hidden debug commands
    [Group("debug"), Description("for debugging purposes, useable by owner only"), RequireOwner]
    public class DebugCommands : GameCommandModuleBase
    {
        [Command("exit"), Description("Safely exit the bot client and save data"), RequireOwner]
        public async Task Exit(CommandContext ctx)
        {
            await ctx.RespondAsync($"Exiting safely and saving data...");
            Environment.Exit(0);
        }

        [Command("dumpdata"), Description("Safely exit the bot client and save data"), RequireOwner]
        public async Task DumpData(CommandContext ctx)
        {
            var i = JsonSerializer.Serialize(Players.Data);
            await ctx.RespondAsync(i);
        }

        [Command("givetestitem"), Description("give test item"), RequireOwner]
        public async Task GiveTestItem(CommandContext ctx)
        {
            if (!await PlayerIsInited(ctx)) return;

            var i = new Item
            {
                Name = "Test Item",
                Description = "Idk",
                Rarity = ItemRarity.Legendary,
                Type = ItemType.Valuable
            };

            Players.Data[ctx.User.Id].Inventory.Add(i);

            await ctx.RespondAsync("Test item added");
        }

        [Command("givelongsword"), Description("give long sword"), RequireOwner]
        public async Task GiveLongSword(CommandContext ctx)
        {
            if (!await PlayerIsInited(ctx)) return;

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

            Players.Data[ctx.User.Id].Inventory.Add(i);

            await ctx.RespondAsync("Long sword added");
        }

        [Command("giveboots"), Description("give boots"), RequireOwner]
        public async Task GiveBoots(CommandContext ctx)
        {
            if (!await PlayerIsInited(ctx)) return;

            var i = new Item
            {
                Name = "Big Boots",
                Description = "Go fast(er)!",
                Rarity = ItemRarity.Basic,
                Type = ItemType.Boots,
                Stats = new()
                {
                    ArmorPenFlat = 5
                }
            };

            Players.Data[ctx.User.Id].Inventory.Add(i);

            await ctx.RespondAsync("Boots added");
        }

        [Command("ItemStructureGen"), Description("Generate item structure and send msg as json"), RequireOwner]
        public async Task ItemStructureGen(CommandContext ctx)
        {

            var i = new Item
            {
                Name = "Name here",
                Description = "Description here",
                Rarity = ItemRarity.Basic,
                Type = ItemType.Weapon,
                Value = 10,
                Stats = new ItemStats
                {
                }
            };

            var j = JsonSerializer.Serialize(i);

            await ctx.RespondAsync(j);
        }
    }
}

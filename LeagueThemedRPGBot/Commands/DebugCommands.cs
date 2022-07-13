using System.Text.Json;
using System.Text.Json.Serialization;
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
            var i = JsonSerializer.Serialize(Players.Data, DataGlobals.SerializationOptions);
            await ctx.RespondAsync(i);
        }

        [Command("genweap"), Description("Generate weapon structure and send msg as json"), RequireOwner]
        public async Task GenWeap(CommandContext ctx)
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
                    Effect = ItemEffect.None
                }
            };

            var j = JsonSerializer.Serialize(i, DataGlobals.SerializationOptions);

            await ctx.RespondAsync(j);
        }

        [Command("genarmor"), Description("Generate armor structure and send msg as json"), RequireOwner]
        public async Task GenArmor(CommandContext ctx)
        {

            var i = new Item
            {
                Name = "Name here",
                Description = "Description here",
                Rarity = ItemRarity.Basic,
                Type = ItemType.Armor,
                Value = 10,
                Stats = new ItemStats
                {
                }
            };

            var j = JsonSerializer.Serialize(i, DataGlobals.SerializationOptions);

            await ctx.RespondAsync(j);
        }

        [Command("genboots"), Description("Generate armor structure and send msg as json"), RequireOwner]
        public async Task GenBoots(CommandContext ctx)
        {

            var i = new Item
            {
                Name = "Name here",
                Description = "Description here",
                Rarity = ItemRarity.Basic,
                Type = ItemType.Boots,
                Value = 10,
                Stats = new ItemStats
                {
                }
            };

            var j = JsonSerializer.Serialize(i, DataGlobals.SerializationOptions);

            await ctx.RespondAsync(j);
        }

        [Command("genskill"), Description("Generate skill structure and send msg as json"), RequireOwner]
        public async Task GenSkill(CommandContext ctx)
        {
            await ctx.RespondAsync(JsonSerializer.Serialize(new Skill(), DataGlobals.SerializationOptions));
        }
    }
}

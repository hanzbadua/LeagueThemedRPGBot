using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using LeagueThemedRPGBot.Game;

namespace LeagueThemedRPGBot.Commands
{
    // Ungrouped main commands
    public class MainCommands : CustomCommandModuleBase
    {
        [Command("init"), Description("Initialize your character")]
        public async Task Init(CommandContext ctx)
        {
            if (Globals.PlayerIsAlreadyInitialized(ctx.User.Id))
            {
                await ctx.RespondAsync("You have already initialized your character!");
                return;
            }

            Globals.PlayerData[ctx.User.Id] = new Player();
            await DataHandler.SaveData(DataHandler.PlayerDataLocation, Globals.PlayerData);

            await ctx.RespondAsync("Character initialized");
        }

        [Command("balance"), Aliases("bal"), Description("Check your current currency balance")]
        public async Task Balance(CommandContext ctx)
        {
            if (!await InitCheck(ctx)) return;

            await ctx.RespondAsync($"Current gold amount: {Globals.PlayerData[ctx.User.Id].Gold}");
        }

        [Command("inventory"), Aliases("inv"), Description("View the contents of your inventory")]
        public async Task Inventory(CommandContext ctx)
        {
            if (!await InitCheck(ctx)) return;

            // Inventory is empty check
            if (await InventoryIsEmpty(ctx))
            {
                await ctx.RespondAsync("Your inventory is empty!");
                return;
            }

            var msg = new DiscordEmbedBuilder
            {
                Title = "Inventory",
                Color = DefBlue
            };

            string contents = "";
            int index = 1;

            foreach (var i in Globals.PlayerData[ctx.User.Id].Inventory)
            {
                contents += $"{index}. {i.Name} ({Enum.GetName(i.Rarity)}, {Enum.GetName(i.Type)}){Environment.NewLine}";
                index++;
            }

            msg.AddField("Contents", contents);
            await ctx.RespondAsync(msg.Build());
        }

        [Command("inventory"), Description("View an item in your inventory via index")]
        public async Task Inventory(CommandContext ctx, int count)
        {
            int index = count - 1; // internal indexes start at 0, for humans it starts at 1, so sub by 1

            if (!await InitCheck(ctx)) return;

            // Inventory is empty check
            if (await InventoryIsEmpty(ctx))
            {
                await ctx.RespondAsync("Your inventory is empty!");
                return;
            }

            // Check if the item index is valid
            if (Globals.PlayerData[ctx.User.Id].Inventory.ElementAtOrDefault(index) is null)
            {
                await ctx.RespondAsync($"There is no valid item in inventory index {count}");
                return;
            }

            var item = Globals.PlayerData[ctx.User.Id].Inventory[index];
            var msg = new DiscordEmbedBuilder { Title = "Viewing item", Color = DefBlue };
            msg.AddField("Name", item.Name);
            msg.AddField("Description", item.Description);
            msg.AddField("Rarity", Enum.GetName(item.Rarity));
            msg.AddField("Type", Enum.GetName(item.Type));
            msg.AddField("Value", item.Value != 0 ? item.Value.ToString() : "Worthless");

            if ((item.Type == ItemType.Weapon || item.Type == ItemType.Armor || item.Type == ItemType.Boots) && item.Stats is not null)
            {
                if (item.Stats.MaxHealth != 0) msg.AddField("Max Health", item.Stats.MaxHealth.ToString());
                if (item.Stats.MaxMana != 0) msg.AddField("Max Mana", item.Stats.MaxMana.ToString());
                if (item.Stats.AttackDamage != 0) msg.AddField("Attack Damage", item.Stats.AttackDamage.ToString());
                if (item.Stats.AbilityPower != 0) msg.AddField("Ability Power", item.Stats.AbilityPower.ToString());
                if (item.Stats.CritChance != 0) msg.AddField("Crit Chance", item.Stats.CritChance.ToString());
                if (item.Stats.CritDamage != 0) msg.AddField("Crit Damage", item.Stats.CritDamage.ToString());
                if (item.Stats.ArmorPenPercent != 0) msg.AddField("Armor Penetration (%)", item.Stats.ArmorPenPercent.ToString());
                if (item.Stats.ArmorPenFlat != 0) msg.AddField("Lethality", item.Stats.ArmorPenFlat.ToString());
                if (item.Stats.MagicPenPercent != 0) msg.AddField("Magic Penetration (%)", item.Stats.MagicPenPercent.ToString());
                if (item.Stats.MagicPenFlat != 0) msg.AddField("Flat Magic Penetration", item.Stats.MagicPenFlat.ToString());
                if (item.Stats.Omnivamp != 0) msg.AddField("Omnivamp", item.Stats.Omnivamp.ToString());
                if (item.Stats.Armor != 0) msg.AddField("Armor", item.Stats.Armor.ToString());
                if (item.Stats.MagicResist != 0) msg.AddField("Magic Resist", item.Stats.MagicResist.ToString());
            }

            await ctx.RespondAsync(msg.Build());
        }
    }
}
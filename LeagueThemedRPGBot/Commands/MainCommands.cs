using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using LeagueThemedRPGBot.Game;

namespace LeagueThemedRPGBot.Commands
{
    // Ungrouped main game commands
    public class MainCommands : GameCommandModuleBase
    {
        [Command("init"), Description("Initialize your character")]
        public async Task Init(CommandContext ctx)
        {
            if (Player.PlayerIsAlreadyInitialized(ctx.User.Id))
            {
                await ctx.RespondAsync("You have already initialized your character!");
                return;
            }

            Player.Data[ctx.User.Id] = new Player();
            await Data.SaveData(Data.PlayerDataLocation, Player.Data);

            await ctx.RespondAsync("Character initialized");
        }

        [Command("balance"), Aliases("bal"), Description("Check your current currency balance")]
        public async Task Balance(CommandContext ctx)
        {
            if (!await PlayerIsInited(ctx)) return;

            await ctx.RespondAsync($"Current gold amount: {Player.Data[ctx.User.Id].Gold}");
        }

        [Command("stats"), Description("Check your current stats")]
        public async Task Stats(CommandContext ctx)
        {
            if (!await PlayerIsInited(ctx)) return;

            var p = Player.Data[ctx.User.Id];

            var msg = new DiscordEmbedBuilder
            {
                Title = "Stats",
                Color = DefBlue
            };

            const string na = "N/A";

            msg.AddField("Level, XP", $"{p.Level}, {p.XP}/{Player.CalculateXPForNextLevel(p.Level)}");
            msg.AddField("Health", $"{p.Health}/{p.MaxHealth}");
            msg.AddField("Mana", $"{p.Mana}/{p.MaxMana}");
            msg.AddField("Attack Damage", p.AttackDamage.ToString());
            msg.AddField("Ability Power", p.AbilityPower.ToString());
            msg.AddField("Crit Chance", $"{p.CritChance}%");
            msg.AddField("Crit Damage", $"{p.CritDamage}%");
            msg.AddField("Armor Pen", $"{p.ArmorPenFlat} flat | {p.ArmorPenPercent}%");
            msg.AddField("Magic Pen", $"{p.MagicPenFlat} flat | {p.MagicPenPercent}%");
            msg.AddField("Omnivamp", $"{p.Omnivamp}%");
            msg.AddField("Resistances", $"{p.Armor} armor | {p.MagicResist} magic resist");
            msg.AddField("Main Weapon", p.MainWeapon is not null ? p.MainWeapon.Name : na);
            msg.AddField("Offhand Weapon", p.OffhandWeapon is not null ? p.OffhandWeapon.Name : na);
            msg.AddField("Armor (1)", p.ArmorOne is not null ? p.ArmorOne.Name : na);
            msg.AddField("Armor (2)", p.ArmorTwo is not null ? p.ArmorTwo.Name : na);
            msg.AddField("Armor (3)", p.ArmorThree is not null ? p.ArmorThree.Name : na);
            msg.AddField("Boots", p.Boots is not null ? p.Boots.Name : na);
            msg.WithFooter($"to check your inventory, use '$inventory'{Environment.NewLine}to check your gold, use '$balance'");

            await ctx.RespondAsync(msg.Build());
        }

        [Command("inventory"), Aliases("inv"), Description("View the contents of your inventory")]
        public async Task Inventory(CommandContext ctx)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await InventoryIsEmpty(ctx)) return;
            
            var msg = new DiscordEmbedBuilder
            {
                Title = "Inventory",
                Color = DefBlue
            };

            string contents = "";
            int index = 1;

            foreach (var i in Player.Data[ctx.User.Id].Inventory)
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
            if (!await PlayerIsInited(ctx)) return;
            if (await InventoryIsEmpty(ctx)) return;
            int index = count - 1; // internal indexes start at 0, for humans it starts at 1, so sub by 1
            if (!await ItemIndexIsValid(ctx, index)) return;

            var item = Player.Data[ctx.User.Id].Inventory[index];
            var msg = new DiscordEmbedBuilder { Title = "Viewing item", Color = DefBlue };
            msg.AddField("Name", item.Name);
            msg.AddField("Description", item.Description);
            msg.AddField("Rarity", Enum.GetName(item.Rarity));
            msg.AddField("Type", Enum.GetName(item.Type));
            msg.AddField("Value", item.Value != 0 ? item.Value.ToString() : "Worthless");

            if ((item.Type == ItemType.Weapon || item.Type == ItemType.Armor || item.Type == ItemType.Boots) && item.Stats is not null)
            {
                if (item.Stats.MaxHealth != 0) 
                    msg.AddField("Max Health", item.Stats.MaxHealth.ToString());

                if (item.Stats.MaxMana != 0) 
                    msg.AddField("Max Mana", item.Stats.MaxMana.ToString());

                if (item.Stats.AttackDamage != 0) 
                    msg.AddField("Attack Damage", item.Stats.AttackDamage.ToString());

                if (item.Stats.AbilityPower != 0) 
                    msg.AddField("Ability Power", item.Stats.AbilityPower.ToString());

                if (item.Stats.CritChance != 0) 
                    msg.AddField("Crit Chance", item.Stats.CritChance.ToString());

                if (item.Stats.CritDamage != 0) 
                    msg.AddField("Crit Damage", item.Stats.CritDamage.ToString());

                if (item.Stats.ArmorPenPercent != 0 || item.Stats.ArmorPenFlat != 0) 
                    msg.AddField("Armor Pen (flat|%)", $"{item.Stats.ArmorPenFlat} | {item.Stats.ArmorPenPercent}%");

                if (item.Stats.MagicPenPercent != 0 || item.Stats.MagicPenFlat != 0) 
                    msg.AddField("Magic Pen (flat|%)", $"{item.Stats.MagicPenFlat} | {item.Stats.MagicPenPercent}%");

                if (item.Stats.Omnivamp != 0) 
                    msg.AddField("Omnivamp", item.Stats.Omnivamp.ToString());

                if (item.Stats.Armor != 0) 
                    msg.AddField("Armor", item.Stats.Armor.ToString());

                if (item.Stats.MagicResist != 0) 
                    msg.AddField("Magic Resist", item.Stats.MagicResist.ToString());
            }

            await ctx.RespondAsync(msg.Build());
        }

        [Command("equip"), Description("Equip an item")]
        public async Task Equip(CommandContext ctx)
        {
            await ctx.RespondAsync("You need to specify an item in your inventory to equip");
        }

        [Command("equip")]
        public async Task Equip(CommandContext ctx, int count)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await InventoryIsEmpty(ctx)) return;
            int index = count - 1; // internal indexes start at 0, for humans it starts at 1, so sub by 1
            if (!await ItemIndexIsValid(ctx, index)) return;

            var player = Player.Data[ctx.User.Id];
            var item = player.Inventory[index];

            if (item.Type != ItemType.Armor && item.Type != ItemType.Weapon && item.Type != ItemType.Boots) {
                await ctx.RespondAsync("This isn't a valid equippable item! Make sure it is either a weapon, armor, or boots");
                return;
            }

            if (item.Type == ItemType.Boots)
            {
                var current = player.Boots;

                if (player.Boots is null)
                {
                    await ctx.RespondAsync($"Equipping boots '{item.Name}'...");
                    Player.Data[ctx.User.Id].Boots = item;
                    Player.Data[ctx.User.Id] = Player.AddStatsFromItem(Player.Data[ctx.User.Id], item);
                    Player.Data[ctx.User.Id].Inventory.RemoveAt(index);
                    return;
                }
                else
                {
                    await AlreadyWearingEquipPattern(ctx, player, item, ItemReplacingSlotType.Boots, index);
                    return;
                }
            }
            else if (item.Type == ItemType.Weapon)
            {
                await ctx.RespondAsync($"Equipping weapon '{item.Name}'...");
                await ctx.RespondAsync($"Respond with *main* or *offhand* to choose what slot to equip {item.Name} in");
                var rr = await ctx.Message.GetNextMessageAsync(i => i.Content.ToLowerInvariant() == "main" || i.Content.ToLowerInvariant() == "offhand");
                if (!rr.TimedOut)
                {
                    if (rr.Result.Content.Contains("main", StringComparison.OrdinalIgnoreCase))
                    {
                        if (player.MainWeapon is null)
                        {
                            await ctx.RespondAsync($"Equipping {item.Name} as main weapon...");
                            Player.Data[ctx.User.Id].MainWeapon = item;
                            Player.Data[ctx.User.Id] = Player.AddStatsFromItem(Player.Data[ctx.User.Id], item);
                            Player.Data[ctx.User.Id].Inventory.RemoveAt(index);
                            return;
                        }
                        else
                        {
                            await AlreadyWearingEquipPattern(ctx, player, item, ItemReplacingSlotType.MainWeapon, index);
                            return;
                        }
                    }
                    else if (rr.Result.Content.Contains("offhand", StringComparison.OrdinalIgnoreCase))
                    {
                        if (player.OffhandWeapon is null)
                        {
                            await ctx.RespondAsync($"Equipping {item.Name} as offhand weapon...");
                            Player.Data[ctx.User.Id].OffhandWeapon = item;
                            Player.Data[ctx.User.Id] = Player.AddStatsFromItem(Player.Data[ctx.User.Id], item);
                            Player.Data[ctx.User.Id].Inventory.RemoveAt(index);
                            return;
                        }
                        else
                        {
                            await AlreadyWearingEquipPattern(ctx, player, item, ItemReplacingSlotType.OffhandWeapon, index);
                            return;
                        }
                    }
                } else
                    await ctx.RespondAsync("Timed out - no changes were made");
            }
            else if (item.Type == ItemType.Armor)
            {
                await ctx.RespondAsync($"Equipping armor '{item.Name}'...");
                await ctx.RespondAsync($"Respond with *one*, *two*, or *three* to choose what slot to equip {item.Name} in");
                var rr = await ctx.Message.GetNextMessageAsync(i => i.Content.ToLowerInvariant() == "one" || i.Content.ToLowerInvariant() == "two" || i.Content.ToLowerInvariant() == "three");
                if (!rr.TimedOut)
                {
                    if (rr.Result.Content.Contains("one", StringComparison.OrdinalIgnoreCase))
                    {
                        if (player.ArmorOne is null)
                        {
                            await ctx.RespondAsync($"Equipping {item.Name} in armor slot one...");
                            Player.Data[ctx.User.Id].ArmorOne = item;
                            Player.Data[ctx.User.Id] = Player.AddStatsFromItem(Player.Data[ctx.User.Id], item);
                            Player.Data[ctx.User.Id].Inventory.RemoveAt(index);
                            return;
                        }
                        else 
                        { 
                            await AlreadyWearingEquipPattern(ctx, player, item, ItemReplacingSlotType.ArmorOne, index);
                            return;
                        }
                    }
                    else if (rr.Result.Content.Contains("two", StringComparison.OrdinalIgnoreCase))
                    {
                        if (player.ArmorTwo is null)
                        {
                            await ctx.RespondAsync($"Equipping {item.Name} in armor slot two...");
                            Player.Data[ctx.User.Id].ArmorTwo = item;
                            Player.Data[ctx.User.Id] = Player.AddStatsFromItem(Player.Data[ctx.User.Id], item);
                            Player.Data[ctx.User.Id].Inventory.RemoveAt(index);
                            return;
                        }
                        else
                        {
                            await AlreadyWearingEquipPattern(ctx, player, item, ItemReplacingSlotType.ArmorTwo, index);
                            return;
                        }
                    }
                    else if (rr.Result.Content.Contains("three", StringComparison.OrdinalIgnoreCase))
                    {
                        if (player.ArmorThree is null)
                        {
                            await ctx.RespondAsync($"Equipping {item.Name} in armor slot three...");
                            Player.Data[ctx.User.Id].ArmorThree = item;
                            Player.Data[ctx.User.Id] = Player.AddStatsFromItem(Player.Data[ctx.User.Id], item);
                            Player.Data[ctx.User.Id].Inventory.RemoveAt(index);
                            return;
                        }
                        else
                        {
                            await AlreadyWearingEquipPattern(ctx, player, item, ItemReplacingSlotType.ArmorThree, index);
                            return;
                        }
                    }
                } else
                    await ctx.RespondAsync("Timed out - no changes were made");
            }
        }

        // NOTE: remove later
        [Command("penis"), Description("PENIS")] public async Task Penis(CommandContext c) => await c.RespondAsync("YUMMY!");

    }
}
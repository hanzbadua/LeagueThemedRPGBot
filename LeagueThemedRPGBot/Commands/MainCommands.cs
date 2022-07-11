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
            await Data.SaveFileData(Data.PlayerDataLocation, Player.Data);

            await ctx.RespondAsync("Character initialized");
        }

        [Command("balance"), Aliases("bal"), Description("Check your current currency balance")]
        public async Task Balance(CommandContext ctx)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;

            await ctx.RespondAsync($"Current gold amount: {Player.Data[ctx.User.Id].Gold}");
        }

        [Command("stats"), Description("Check your current stats")]
        public async Task Stats(CommandContext ctx)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;

            var p = Player.Data[ctx.User.Id];

            var msg = new DiscordEmbedBuilder
            {
                Title = "Stats",
                Color = DefBlue
            };

            msg.AddField("Level, XP", $"{p.Level}, {p.XP}/{p.CalculateXPForNextLevel()}");
            msg.AddField("Max Health", $"{p.MaxHealth}");
            msg.AddField("Max Mana", $"{p.MaxMana}");
            //msg.AddField("Health", $"{p.Health}/{p.MaxHealth}");
            //msg.AddField("Mana", $"{p.Mana}/{p.MaxMana}");
            msg.AddField("Attack Damage", p.AttackDamage.ToString());
            msg.AddField("Ability Power", p.AbilityPower.ToString());
            msg.AddField("Crit Chance", $"{p.CritChance}%");
            msg.AddField("Bonus Crit Damage", p.BonusCritDamage != 0 ? $"+{p.BonusCritDamage}%" : "0%");
            msg.AddField("Armor Pen", $"{p.ArmorPenFlat} flat | {p.ArmorPenPercent}%");
            msg.AddField("Magic Pen", $"{p.MagicPenFlat} flat | {p.MagicPenPercent}%");
            msg.AddField("Omnivamp", $"{p.Omnivamp}%");
            msg.AddField("Resistances", $"{p.Armor} armor | {p.MagicResist} magic resist");
            msg.WithFooter($"to check your inventory, use '$inventory'{Environment.NewLine}to check your gold, use '$balance'{Environment.NewLine}to check your equipped items, use '$equipped'");

            await ctx.RespondAsync(msg.Build());
        }

        [Command("equipped"), Description("Check your currently equipped items")]
        public async Task Equipped(CommandContext ctx)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;

            var p = Player.Data[ctx.User.Id];
            const string na = "N/A";

            var msg = new DiscordEmbedBuilder
            {
                Title = "Equipped Items",
                Color = DefBlue
            };

            msg.AddField("Main Weapon", p.MainWeapon is not null ? p.MainWeapon.Name : na)
                .AddField("Offhand Weapon", p.OffhandWeapon is not null ? p.OffhandWeapon.Name : na)
                .AddField("Armor (1)", p.ArmorOne is not null ? p.ArmorOne.Name : na)
                .AddField("Armor (2)", p.ArmorTwo is not null ? p.ArmorTwo.Name : na)
                .AddField("Armor (3)", p.ArmorThree is not null ? p.ArmorThree.Name : na)
                .AddField("Boots", p.Boots is not null ? p.Boots.Name : na);
            await ctx.RespondAsync(msg.Build());
        }

        [Command("inventory"), Aliases("inv"), Description("View the contents of your inventory")]
        public async Task Inventory(CommandContext ctx)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;
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
            if (await PlayerIsBusy(ctx)) return;
            if (await InventoryIsEmpty(ctx)) return;
            int index = count - 1; // internal indexes start at 0, for humans it starts at 1, so sub by 1
            if (!await ItemIndexIsValid(ctx, index)) return;

            var item = Player.Data[ctx.User.Id].Inventory[index];
            var msg = new DiscordEmbedBuilder { Title = "Viewing item", Color = DefBlue };
            msg.AddField("Name", item.Name)
                .AddField("Description", item.Description)
                .AddField("Rarity", Enum.GetName(item.Rarity))
                .AddField("Type", Enum.GetName(item.Type))
                .AddField("Value", item.Value != 0 ? item.Value.ToString() : "Worthless");

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
                    msg.AddField("Bonus Crit Damage", item.Stats.CritDamage.ToString());

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

        [Command("equip")]
        public async Task Equip(CommandContext ctx)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;
            await ctx.RespondAsync("You need to specify an item in your inventory to equip");
        }

        [Command("equip"), Description("Equip an item via inventory index")]
        public async Task Equip(CommandContext ctx, [Description("Index of the item to equip")] int count)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;
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
                    Player.Data[ctx.User.Id].AddStatsFromItem(item);
                    Player.Data[ctx.User.Id].Inventory.RemoveAt(index);
                }
                else
                {
                    Player.Data[ctx.User.Id].Busy = true;
                    await AlreadyWearingEquipPattern(ctx, player, item, ItemSlot.Boots, index);
                }
            }
            else if (item.Type == ItemType.Weapon)
            {
                await ctx.RespondAsync($"Equipping weapon '{item.Name}'...");
                await ctx.RespondAsync($"Respond with *main* or *offhand* to choose what slot to equip {item.Name} in");

                Player.Data[ctx.User.Id].Busy = true;

                var rr = await ctx.Message.GetNextMessageAsync(i => i.Content.ToLowerInvariant() == "main" || i.Content.ToLowerInvariant() == "offhand");
                if (!rr.TimedOut)
                {
                    if (rr.Result.Content.Contains("main", StringComparison.OrdinalIgnoreCase))
                    {
                        if (player.MainWeapon is null)
                        {
                            await ctx.RespondAsync($"Equipping {item.Name} as main weapon...");
                            Player.Data[ctx.User.Id].MainWeapon = item;
                            Player.Data[ctx.User.Id].AddStatsFromItem(item);
                            Player.Data[ctx.User.Id].Inventory.RemoveAt(index);
                        }
                        else
                        {
                            await AlreadyWearingEquipPattern(ctx, player, item, ItemSlot.MainWeapon, index);
                        }
                    }
                    else if (rr.Result.Content.Contains("offhand", StringComparison.OrdinalIgnoreCase))
                    {
                        if (player.OffhandWeapon is null)
                        {
                            await ctx.RespondAsync($"Equipping {item.Name} as offhand weapon...");
                            Player.Data[ctx.User.Id].OffhandWeapon = item;
                            Player.Data[ctx.User.Id].AddStatsFromItem(item);
                            Player.Data[ctx.User.Id].Inventory.RemoveAt(index);
                        }
                        else
                        {
                            await AlreadyWearingEquipPattern(ctx, player, item, ItemSlot.OffhandWeapon, index);
                        }
                    }
                } else
                    await ctx.RespondAsync("Timed out - no changes were made");
            }
            else if (item.Type == ItemType.Armor)
            {
                await ctx.RespondAsync($"Equipping armor '{item.Name}'...");
                await ctx.RespondAsync($"Respond with *one*, *two*, or *three* to choose what slot to equip {item.Name} in");

                Player.Data[ctx.User.Id].Busy = true;

                var rr = await ctx.Message.GetNextMessageAsync(i => i.Content.ToLowerInvariant() == "one" || i.Content.ToLowerInvariant() == "two" || i.Content.ToLowerInvariant() == "three");
                if (!rr.TimedOut)
                {
                    if (rr.Result.Content.Contains("one", StringComparison.OrdinalIgnoreCase))
                    {
                        if (player.ArmorOne is null)
                        {
                            await ctx.RespondAsync($"Equipping {item.Name} in armor slot one...");
                            Player.Data[ctx.User.Id].ArmorOne = item;
                            Player.Data[ctx.User.Id].AddStatsFromItem(item);
                            Player.Data[ctx.User.Id].Inventory.RemoveAt(index);
                        }
                        else
                        {
                            await AlreadyWearingEquipPattern(ctx, player, item, ItemSlot.ArmorOne, index);
                        }
                    }
                    else if (rr.Result.Content.Contains("two", StringComparison.OrdinalIgnoreCase))
                    {
                        if (player.ArmorTwo is null)
                        {
                            await ctx.RespondAsync($"Equipping {item.Name} in armor slot two...");
                            Player.Data[ctx.User.Id].ArmorTwo = item;
                            Player.Data[ctx.User.Id].AddStatsFromItem(item);
                            Player.Data[ctx.User.Id].Inventory.RemoveAt(index);
                        }
                        else
                        {
                            await AlreadyWearingEquipPattern(ctx, player, item, ItemSlot.ArmorTwo, index);
                        }
                    }
                    else if (rr.Result.Content.Contains("three", StringComparison.OrdinalIgnoreCase))
                    {
                        if (player.ArmorThree is null)
                        {
                            await ctx.RespondAsync($"Equipping {item.Name} in armor slot three...");
                            Player.Data[ctx.User.Id].ArmorThree = item;
                            Player.Data[ctx.User.Id].AddStatsFromItem(item);
                            Player.Data[ctx.User.Id].Inventory.RemoveAt(index);
                        }
                        else
                        {
                            await AlreadyWearingEquipPattern(ctx, player, item, ItemSlot.ArmorThree, index);
                        }
                    }
                } else
                    await ctx.RespondAsync("Timed out - no changes were made");
            }

            Player.Data[ctx.User.Id].Busy = false;
        }

        [Command("Unequip")]
        public async Task Unequip(CommandContext ctx)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;
            var msg = new DiscordEmbedBuilder
            {
                Title = "No Unequip Slot Specified",
                Color = DefRed,
                Description = "Valid unequip slots: `armorone`, `armortwo`, `armorthree`, `boots`, `mainweapon`, `offhandweapon`"
            };

            await ctx.RespondAsync(msg.Build());
        }

        [Command("Unequip"), Description("Unequip an item in a slot via slot name")]
        public async Task Unequip(CommandContext ctx, [RemainingText] [Description("Slot name to unequip")] string slot)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;

            slot = slot.RemoveWhitespace().ToLowerInvariant();
            var p = Player.Data[ctx.User.Id];

            if (slot == "armorone")
            {
                if (p.ArmorOne is null)
                {
                    await ctx.RespondAsync("There's nothing in slot `armorone` to unequip!");
                }
                else 
                {
                    await UnequipLogic(ctx, ItemSlot.ArmorOne);
                }
            }
            else if (slot == "armortwo")
            {
                if (p.ArmorTwo is null)
                {
                    await ctx.RespondAsync("There's nothing in slot `armortwo` to unequip!");
                }
                else
                {
                    await UnequipLogic(ctx, ItemSlot.ArmorTwo);
                }
            }
            else if (slot == "armorthree")
            {
                if (p.ArmorThree is null)
                {
                    await ctx.RespondAsync("There's nothing in slot `armorthree` to unequip!");
                }
                else
                {
                    await UnequipLogic(ctx, ItemSlot.ArmorThree);
                }
            }
            else if (slot == "mainweapon")
            {
                if (p.MainWeapon is null)
                {
                    await ctx.RespondAsync("There's nothing in slot `mainweapon` to unequip!");
                }
                else
                {
                    await UnequipLogic(ctx, ItemSlot.MainWeapon);
                }
            }
            else if (slot == "offhandweapon")
            {
                if (p.OffhandWeapon is null)
                {
                    await ctx.RespondAsync("There's nothing in slot `offhandweapon` to unequip!");
                }
                else
                {
                    await UnequipLogic(ctx, ItemSlot.OffhandWeapon);
                }
            }
            else if (slot == "boots")
            {
                if (p.Boots is null)
                {
                    await ctx.RespondAsync("There's nothing in slot `boots` to unequip!");
                }
                else
                {
                    await UnequipLogic(ctx, ItemSlot.Boots);
                }
            }
            else
            {
                await ctx.RespondAsync($"{slot} is not a valid unequip slot;{Environment.NewLine}Valid unequip slots: `armorone`, `armortwo`, `armorthree`, `boots`, `mainweapon`, `offhandweapon`"); 
            }
        }

        [Command("encounter"), Description("Maybe you'll find something worthwhile to fight")]
        public async Task Encounter(CommandContext ctx)
        {
            Player.Data[ctx.User.Id].Health = Player.Data[ctx.User.Id].MaxHealth;

            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;

            Player.Data[ctx.User.Id].Busy = true;
            await CombatRoutine(ctx, Enemy.GetScalingEnemy(Player.Data[ctx.User.Id].Level, EncounterTypes.Common));
            Player.Data[ctx.User.Id].Busy = false;

            Player.Data[ctx.User.Id].Health = Player.Data[ctx.User.Id].MaxHealth;
        }

        // NOTE: remove later because it is a test command
        [Command("addweapon"), Description("Add an item by name")]
        public async Task AddWeapon(CommandContext ctx, [RemainingText] string name)
        {
            if (!await PlayerIsInited(ctx)) 
                return;

            Player.Data[ctx.User.Id].Inventory.Add(Data.GetWeaponByName(name));
        }
    }
}
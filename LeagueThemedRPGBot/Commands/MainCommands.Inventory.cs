using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using LeagueThemedRPGBot.Game;

namespace LeagueThemedRPGBot.Commands
{
    // inventory related cmds
    // Desperately needs rewrite.
    public partial class MainCommands
    {
        [Command("inventory"), Aliases("inv"), Description("View the contents of your inventory")]
        public async Task Inventory(CommandContext ctx)
        {
            if (await PlayerIsNotInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;
            if (await InventoryIsEmpty(ctx)) return;

            string contents = "";
            int index = 1;

            foreach (var i in Players.Data[ctx.User.Id].Inventory)
            {
                contents += $"{index}. {i.Name} ({Enum.GetName(i.Rarity)}, {Enum.GetName(i.Type)}){NL}";
                index++;
            }

            var msg = new DiscordEmbedBuilder
            {
                Title = "Inventory",
                Color = DefBlue,
                Description = contents
            };

            await ctx.RespondAsync(msg.Build());
        }

        [Command("inventory"), Description("View an item in your inventory via index")]
        public async Task Inventory(CommandContext ctx, [Description("Inventory index of the item to view")] int count)
        {
            if (await PlayerIsNotInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;
            if (await InventoryIsEmpty(ctx)) return;
            int index = count - 1; // internal indexes start at 0, for humans it starts at 1, so sub by 1
            if (!await ItemIndexIsValid(ctx, index)) return;

            var item = Players.Data[ctx.User.Id].Inventory[index];
            var msg = new DiscordEmbedBuilder { Title = $"Viewing item: {item.Name}", Color = DefBlue, Description = item.Description}
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
            if (await PlayerIsNotInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;
            await ctx.RespondAsync("You need to specify an item in your inventory to equip");
        }

        [Command("equip"), Description("Equip an item via inventory index")]
        public async Task Equip(CommandContext ctx, [Description("Inventory index of the item to equip")] int count)
        {
            if (await PlayerIsNotInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;
            if (await InventoryIsEmpty(ctx)) return;
            int index = count - 1; // internal indexes start at 0, for humans it starts at 1, so sub by 1
            if (!await ItemIndexIsValid(ctx, index)) return;

            var player = Players.Data[ctx.User.Id];
            var item = player.Inventory[index];

            if (item.Type != ItemType.Armor && item.Type != ItemType.Weapon && item.Type != ItemType.Boots)
            {
                await ctx.RespondAsync("This isn't a valid equippable item! Make sure it is either a weapon, armor, or boots");
                return;
            }

            Players.Data[ctx.User.Id].Busy = true;

            if (item.Type == ItemType.Boots)
            {
                var current = player.Boots;

                if (player.Boots is null)
                {
                    await ctx.RespondAsync($"Equipping boots '{item.Name}'...");
                    Players.Data[ctx.User.Id].Boots = item;
                    Players.Data[ctx.User.Id].AddStatsFromItem(item);
                    Players.Data[ctx.User.Id].Inventory.RemoveAt(index);
                }
                else
                {
                    await AlreadyWearingEquipPattern(ctx, player, item, ItemSlot.Boots, index);
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
                            Players.Data[ctx.User.Id].MainWeapon = item;
                            Players.Data[ctx.User.Id].AddStatsFromItem(item);
                            Players.Data[ctx.User.Id].Inventory.RemoveAt(index);
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
                            Players.Data[ctx.User.Id].OffhandWeapon = item;
                            Players.Data[ctx.User.Id].AddStatsFromItem(item);
                            Players.Data[ctx.User.Id].Inventory.RemoveAt(index);
                        }
                        else
                        {
                            await AlreadyWearingEquipPattern(ctx, player, item, ItemSlot.OffhandWeapon, index);
                        }
                    }
                }
                else
                    await ctx.RespondAsync("Timed out - no changes were made");
            }
            else if (item.Type == ItemType.Armor)
            {
                await ctx.RespondAsync($"Equipping armor '{item.Name}'...");
                await ctx.RespondAsync($"Respond with *one*, *two*, or *three* to choose what slot to equip {item.Name} in");

                var rr = await ctx.Message.GetNextMessageAsync(i => i.Content.ToLowerInvariant() == "one" || i.Content.ToLowerInvariant() == "two");
                if (!rr.TimedOut)
                {
                    if (rr.Result.Content.Contains("one", StringComparison.OrdinalIgnoreCase))
                    {
                        if (player.Armor1 is null)
                        {
                            await ctx.RespondAsync($"Equipping {item.Name} in armor slot one...");
                            Players.Data[ctx.User.Id].Armor1 = item;
                            Players.Data[ctx.User.Id].AddStatsFromItem(item);
                            Players.Data[ctx.User.Id].Inventory.RemoveAt(index);
                        }
                        else
                        {
                            await AlreadyWearingEquipPattern(ctx, player, item, ItemSlot.ArmorOne, index);
                        }
                    }
                    else if (rr.Result.Content.Contains("two", StringComparison.OrdinalIgnoreCase))
                    {
                        if (player.Armor2 is null)
                        {
                            await ctx.RespondAsync($"Equipping {item.Name} in armor slot two...");
                            Players.Data[ctx.User.Id].Armor2 = item;
                            Players.Data[ctx.User.Id].AddStatsFromItem(item);
                            Players.Data[ctx.User.Id].Inventory.RemoveAt(index);
                        }
                        else
                        {
                            await AlreadyWearingEquipPattern(ctx, player, item, ItemSlot.ArmorTwo, index);
                        }
                    }
                }
                else
                    await ctx.RespondAsync("Timed out - no changes were made");
            }

            Players.Data[ctx.User.Id].Busy = false;
        }

        [Command("Unequip")]
        public async Task Unequip(CommandContext ctx)
        {
            if (await PlayerIsNotInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;
            var msg = new DiscordEmbedBuilder
            {
                Title = "No unequip slot specified",
                Color = DefRed,
                Description = "Valid unequip slots: `armor1`, `armor2`, `boots`, `mainweapon`, `offhandweapon`"
            };

            await ctx.RespondAsync(msg.Build());
        }

        [Command("Unequip"), Description("Unequip an item in a slot via slot name")]
        public async Task Unequip(CommandContext ctx, [RemainingText][Description("Slot name to unequip")] string slot)
        {
            if (await PlayerIsNotInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;

            slot = slot.RemoveWhitespace().ToLowerInvariant();
            var p = Players.Data[ctx.User.Id];
            Item i = new(); // failsafe new() should never be used :)

            var noequip = new DiscordEmbedBuilder
            {
                Title = $"There's nothing to unequip in slot `{slot}`",
                Description = "You can use the command `$equipped` to check what you have currently equipped",
                Color = DefRed
            }.Build();

            var success = new DiscordEmbedBuilder
            {
                Color = DefGreen
            };

            if (slot == "armor1")
            {
                if (p.Armor1 is null)
                {
                    await ctx.RespondAsync(noequip);
                }
                else
                {
                    i = p.Armor1;
                    p.Armor1 = null;
                    await ctx.RespondAsync(success.WithTitle($"Item '{i.Name}' successfully unequipped from slot {slot}").Build());
                }
            }
            else if (slot == "armor2")
            {
                if (p.Armor2 is null)
                {
                    await ctx.RespondAsync(noequip);
                }
                else
                {
                    i = p.Armor2;
                    p.Armor2 = null;
                    await ctx.RespondAsync(success.WithTitle($"Item '{i.Name}' successfully unequipped from slot {slot}").Build());
                }
            }
            else if (slot == "mainweapon")
            {
                if (p.MainWeapon is null)
                {
                    await ctx.RespondAsync(noequip);
                }
                else
                {
                    i = p.MainWeapon;
                    p.MainWeapon = null;
                    await ctx.RespondAsync(success.WithTitle($"Item '{i.Name}' successfully unequipped from slot {slot}").Build());
                }
            }
            else if (slot == "offhandweapon")
            {
                if (p.OffhandWeapon is null)
                {
                    await ctx.RespondAsync(noequip);
                }
                else
                {
                    i = p.OffhandWeapon;
                    p.OffhandWeapon = null;
                    await ctx.RespondAsync(success.WithTitle($"Item '{i.Name}' successfully unequipped from slot {slot}").Build());
                }
            }
            else if (slot == "boots")
            {
                if (p.Boots is null)
                {
                    await ctx.RespondAsync(noequip);
                }
                else
                {
                    i = p.Boots;
                    p.Boots = null;
                    await ctx.RespondAsync(success.WithTitle($"Item '{i.Name}' successfully unequipped from slot {slot}").Build());
                }
            }
            else
            {
                var err = new DiscordEmbedBuilder
                {
                    Title = "Invalid unequip slot specified",
                    Description = $"`{slot}` is not a valid unequip slot{NL}Valid unequip slots: `armor1`, `armor2`, `boots`, `mainweapon`, `offhandweapon`",
                    Color = DefRed
                };

                await ctx.RespondAsync(err.Build());
                return;
            }

            p.Inventory.Add(i);
            p.RemoveStatsFromItem(i);
        }
    }
}

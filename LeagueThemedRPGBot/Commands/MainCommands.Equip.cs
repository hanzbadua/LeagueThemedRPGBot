using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using LeagueThemedRPGBot.Game;

namespace LeagueThemedRPGBot.Commands
{
    // Equip + unequip commands 
    public partial class MainCommands
    {
        [Command("equip")]
        public async Task Equip(CommandContext ctx)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;
            await ctx.RespondAsync("You need to specify an item in your inventory to equip");
        }

        [Command("equip"), Description("Equip an item via inventory index")]
        public async Task Equip(CommandContext ctx, [Description("Inventory index of the item to equip")] int count)
        {
            if (!await PlayerIsInited(ctx)) return;
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
                    Players.Data[ctx.User.Id].Busy = true;
                    await AlreadyWearingEquipPattern(ctx, player, item, ItemSlot.Boots, index);
                }
            }
            else if (item.Type == ItemType.Weapon)
            {
                await ctx.RespondAsync($"Equipping weapon '{item.Name}'...");
                await ctx.RespondAsync($"Respond with *main* or *offhand* to choose what slot to equip {item.Name} in");

                Players.Data[ctx.User.Id].Busy = true;

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

                Players.Data[ctx.User.Id].Busy = true;

                var rr = await ctx.Message.GetNextMessageAsync(i => i.Content.ToLowerInvariant() == "one" || i.Content.ToLowerInvariant() == "two" || i.Content.ToLowerInvariant() == "three");
                if (!rr.TimedOut)
                {
                    if (rr.Result.Content.Contains("one", StringComparison.OrdinalIgnoreCase))
                    {
                        if (player.ArmorOne is null)
                        {
                            await ctx.RespondAsync($"Equipping {item.Name} in armor slot one...");
                            Players.Data[ctx.User.Id].ArmorOne = item;
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
                        if (player.ArmorTwo is null)
                        {
                            await ctx.RespondAsync($"Equipping {item.Name} in armor slot two...");
                            Players.Data[ctx.User.Id].ArmorTwo = item;
                            Players.Data[ctx.User.Id].AddStatsFromItem(item);
                            Players.Data[ctx.User.Id].Inventory.RemoveAt(index);
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
                            Players.Data[ctx.User.Id].ArmorThree = item;
                            Players.Data[ctx.User.Id].AddStatsFromItem(item);
                            Players.Data[ctx.User.Id].Inventory.RemoveAt(index);
                        }
                        else
                        {
                            await AlreadyWearingEquipPattern(ctx, player, item, ItemSlot.ArmorThree, index);
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
        public async Task Unequip(CommandContext ctx, [RemainingText][Description("Slot name to unequip")] string slot)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;

            slot = slot.RemoveWhitespace().ToLowerInvariant();
            var p = Players.Data[ctx.User.Id];

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
    }
}

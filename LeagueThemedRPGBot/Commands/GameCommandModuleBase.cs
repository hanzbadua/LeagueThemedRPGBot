using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using LeagueThemedRPGBot.Game;

namespace LeagueThemedRPGBot.Commands
{
    public class GameCommandModuleBase : BaseCommandModule
    {
        // not a command
        // append 'if (!await PlayerIsInited(ctx)) return;' to the prologue of all game-related methods
        protected async Task<bool> PlayerIsInited(CommandContext ctx)
        {
            if (!Player.PlayerIsAlreadyInitialized(ctx.User.Id))
            {
                await ctx.RespondAsync("You don't seem to exist in the player database - have you initialized? `$init`");
                return false;
            }

            return true;
        }

        // Remember to PlayerIsInited() before calling this!
        protected async Task<bool> InventoryIsEmpty(CommandContext ctx)
        {
            // Inventory is empty check
            if (!Player.Data[ctx.User.Id].Inventory.Any())
            {
                await ctx.RespondAsync("Your inventory is empty!");
                return true;
            }

            return false;
        }

        // recommended to also do PlayerIsInited() and InventoryIsEmpty() checks before calling this
        // uses index, not count!
        protected async Task<bool> ItemIndexIsValid(CommandContext ctx, int index)
        {
            if (Player.Data[ctx.User.Id].Inventory.ElementAtOrDefault(index) is null)
            {
                await ctx.RespondAsync($"There is no valid item in inventory index {index+1}"); // count is index+1
                return false;
            }

            return true;
        }

        // only use this after equip checks + with the context that the respective equip slot already has something equipped
        protected async Task AlreadyWearingEquipPattern(CommandContext ctx, Player pl, Item item, ItemReplacingSlotType stype, int index)
        {
            if (item.Type == ItemType.Boots && stype == ItemReplacingSlotType.Boots)
            {
                var current = pl.Boots;

                await ctx.RespondAsync($"You're already wearing a pair of boots ({current.Name}) - respond with *confirm* to replace");
                var rr = await ctx.Message.GetNextMessageAsync(i => i.Content.ToLowerInvariant() == "confirm");
                if (!rr.TimedOut)
                {
                    Player.Data[ctx.User.Id].Boots = item;
                    Player.Data[ctx.User.Id] = Player.AddStatsFromItem(Player.Data[ctx.User.Id], item);
                    Player.Data[ctx.User.Id] = Player.RemoveStatsFromItem(Player.Data[ctx.User.Id], current);
                    Player.Data[ctx.User.Id].Inventory.RemoveAt(index);
                    Player.Data[ctx.User.Id].Inventory.Add(current);
                    await ctx.RespondAsync("Boots replaced successfully");
                }
                else
                    await ctx.RespondAsync("Timed out - no changes were made");
                return;
            }
            else if (item.Type == ItemType.Weapon)
            {
                if (stype == ItemReplacingSlotType.MainWeapon)
                {
                    var current = pl.MainWeapon;

                    await ctx.RespondAsync($"You already have a main weapon equipped ({current.Name}) - respond with *confirm* to replace");
                    var rr = await ctx.Message.GetNextMessageAsync(i => i.Content.ToLowerInvariant() == "confirm");
                    if (!rr.TimedOut)
                    {
                        Player.Data[ctx.User.Id].MainWeapon = item;
                        Player.Data[ctx.User.Id] = Player.AddStatsFromItem(Player.Data[ctx.User.Id], item);
                        Player.Data[ctx.User.Id] = Player.RemoveStatsFromItem(Player.Data[ctx.User.Id], current);
                        Player.Data[ctx.User.Id].Inventory.RemoveAt(index);
                        Player.Data[ctx.User.Id].Inventory.Add(current);
                        await ctx.RespondAsync("Main weapon replaced successfully");
                    }
                    else
                        await ctx.RespondAsync("Timed out - no changes were made");
                    return;
                }
                else if (stype == ItemReplacingSlotType.OffhandWeapon)
                {
                    var current = pl.OffhandWeapon;

                    await ctx.RespondAsync($"You already have an offhand weapon equipped ({current.Name}) - respond with *confirm* to replace");
                    var rr = await ctx.Message.GetNextMessageAsync(i => i.Content.ToLowerInvariant() == "confirm");
                    if (!rr.TimedOut)
                    {
                        Player.Data[ctx.User.Id].OffhandWeapon = item;
                        Player.Data[ctx.User.Id] = Player.AddStatsFromItem(Player.Data[ctx.User.Id], item);
                        Player.Data[ctx.User.Id] = Player.RemoveStatsFromItem(Player.Data[ctx.User.Id], current);
                        Player.Data[ctx.User.Id].Inventory.RemoveAt(index);
                        Player.Data[ctx.User.Id].Inventory.Add(current);
                        await ctx.RespondAsync("Offhand weapon replaced successfully");
                    }
                    else
                        await ctx.RespondAsync("Timed out - no changes were made");
                    return;
                }
            }
            else if (item.Type == ItemType.Armor)
            {
                if (stype == ItemReplacingSlotType.ArmorOne)
                {
                    var current = pl.ArmorOne;

                    await ctx.RespondAsync($"You already have armor equipped in slot one ({current.Name}) - respond with *confirm* to replace");
                    var rr = await ctx.Message.GetNextMessageAsync(i => i.Content.ToLowerInvariant() == "confirm");
                    if (!rr.TimedOut)
                    {
                        Player.Data[ctx.User.Id].ArmorOne = item;
                        Player.Data[ctx.User.Id] = Player.AddStatsFromItem(Player.Data[ctx.User.Id], item);
                        Player.Data[ctx.User.Id] = Player.RemoveStatsFromItem(Player.Data[ctx.User.Id], current);
                        Player.Data[ctx.User.Id].Inventory.RemoveAt(index);
                        Player.Data[ctx.User.Id].Inventory.Add(current);
                        await ctx.RespondAsync("Armor in slot one replaced successfully");
                    }
                    else
                        await ctx.RespondAsync("Timed out - no changes were made");
                    return;
                }
                else if (stype == ItemReplacingSlotType.ArmorTwo)
                {
                    var current = pl.ArmorTwo;

                    await ctx.RespondAsync($"You already have armor equipped in slot two ({current.Name}) - respond with *confirm* to replace");
                    var rr = await ctx.Message.GetNextMessageAsync(i => i.Content.ToLowerInvariant() == "confirm");
                    if (!rr.TimedOut)
                    {
                        Player.Data[ctx.User.Id].ArmorTwo = item;
                        Player.Data[ctx.User.Id] = Player.AddStatsFromItem(Player.Data[ctx.User.Id], item);
                        Player.Data[ctx.User.Id] = Player.RemoveStatsFromItem(Player.Data[ctx.User.Id], current);
                        Player.Data[ctx.User.Id].Inventory.RemoveAt(index);
                        Player.Data[ctx.User.Id].Inventory.Add(current);
                        await ctx.RespondAsync("Armor in slot two replaced successfully");
                    }
                    else
                        await ctx.RespondAsync("Timed out - no changes were made");
                    return;
                }
                else if (stype == ItemReplacingSlotType.ArmorThree)
                {
                    var current = pl.ArmorThree;

                    await ctx.RespondAsync($"You already have armor equipped in slot three ({current.Name}) - respond with *confirm* to replace");
                    var rr = await ctx.Message.GetNextMessageAsync(i => i.Content.ToLowerInvariant() == "confirm");
                    if (!rr.TimedOut)
                    {
                        Player.Data[ctx.User.Id].ArmorThree = item;
                        Player.Data[ctx.User.Id] = Player.AddStatsFromItem(Player.Data[ctx.User.Id], item);
                        Player.Data[ctx.User.Id] = Player.RemoveStatsFromItem(Player.Data[ctx.User.Id], current);
                        Player.Data[ctx.User.Id].Inventory.RemoveAt(index);
                        Player.Data[ctx.User.Id].Inventory.Add(current);
                        await ctx.RespondAsync("Armor in slot three replaced successfully");
                    }
                    else
                        await ctx.RespondAsync("Timed out - no changes were made");
                    return;
                }
            }
        }

        protected readonly DiscordColor DefBlue = new DiscordColor(0, 191, 255);
    }
}

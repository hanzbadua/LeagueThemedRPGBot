using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using LeagueThemedRPGBot.Game;

namespace LeagueThemedRPGBot.Commands
{
    public class GameCommandModuleBase : BaseCommandModule
    {
        // append '= new();' if dependency injection doesn't work
        public PlayerData Players { protected get; set; }
        public ItemData Items { protected get; set; } 
        public Random Rng { protected get; set; }

        public override async Task AfterExecutionAsync(CommandContext ctx)
        {
            await Players.SaveAsync();
        }

        // not a command
        // append 'if (!await PlayerIsInited(ctx)) return;' to the prologue of all game-related methods
        protected async Task<bool> PlayerIsInited(CommandContext ctx)
        {
            if (!Players.IsInitedByID(ctx.User.Id))
            {
                await ctx.RespondAsync("You don't seem to exist in the player database - have you initialized? `$init`");
                return false;
            }

            return true;
        }

        // initcheck this first
        protected async Task<bool> PlayerIsBusy(CommandContext ctx)
        {
            if (Players.Data[ctx.User.Id].Busy == true)
            {
                await ctx.RespondAsync("You appear to be doing something else right now - please resolve that first");
                return true;
            }

            return false;
        }

        // Remember to PlayerIsInited() before calling this!
        protected async Task<bool> InventoryIsEmpty(CommandContext ctx)
        {
            // Inventory is empty check
            if (!Players.Data[ctx.User.Id].Inventory.Any())
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
            if (Players.Data[ctx.User.Id].Inventory.ElementAtOrDefault(index) is null)
            {
                await ctx.RespondAsync($"There is no valid item in inventory index {index+1}"); // count is index+1
                return false;
            }

            return true;
        }

        // only use this after equip checks + with the context that the respective equip slot already has something equipped
        protected async Task AlreadyWearingEquipPattern(CommandContext ctx, Player pl, Item item, ItemSlot stype, int index)
        {
            if (item.Type == ItemType.Boots && stype == ItemSlot.Boots)
            {
                var current = pl.Boots;

                await ctx.RespondAsync($"You're already wearing a pair of boots ({current.Name}) - respond with *confirm* to replace");
                var rr = await ctx.Message.GetNextMessageAsync(i => i.Content.ToLowerInvariant() == "confirm");
                if (!rr.TimedOut)
                {
                    Players.Data[ctx.User.Id].Boots = item;
                    Players.Data[ctx.User.Id].AddStatsFromItem(item);
                    Players.Data[ctx.User.Id].RemoveStatsFromItem(current);
                    Players.Data[ctx.User.Id].Inventory.RemoveAt(index);
                    Players.Data[ctx.User.Id].Inventory.Add(current);
                    await ctx.RespondAsync("Boots replaced successfully");
                }
                else
                    await ctx.RespondAsync("Timed out - no changes were made");
                return;
            }
            else if (item.Type == ItemType.Weapon)
            {
                if (stype == ItemSlot.MainWeapon)
                {
                    var current = pl.MainWeapon;

                    await ctx.RespondAsync($"You already have a main weapon equipped ({current.Name}) - respond with *confirm* to replace");
                    var rr = await ctx.Message.GetNextMessageAsync(i => i.Content.ToLowerInvariant() == "confirm");
                    if (!rr.TimedOut)
                    {
                        Players.Data[ctx.User.Id].MainWeapon = item;
                        Players.Data[ctx.User.Id].AddStatsFromItem(item);
                        Players.Data[ctx.User.Id].RemoveStatsFromItem(current);
                        Players.Data[ctx.User.Id].Inventory.RemoveAt(index);
                        Players.Data[ctx.User.Id].Inventory.Add(current);
                        await ctx.RespondAsync("Main weapon replaced successfully");
                    }
                    else
                        await ctx.RespondAsync("Timed out - no changes were made");
                    return;
                }
                else if (stype == ItemSlot.OffhandWeapon)
                {
                    var current = pl.OffhandWeapon;

                    await ctx.RespondAsync($"You already have an offhand weapon equipped ({current.Name}) - respond with *confirm* to replace");
                    var rr = await ctx.Message.GetNextMessageAsync(i => i.Content.ToLowerInvariant() == "confirm");
                    if (!rr.TimedOut)
                    {
                        Players.Data[ctx.User.Id].OffhandWeapon = item;
                        Players.Data[ctx.User.Id].AddStatsFromItem(item);
                        Players.Data[ctx.User.Id].RemoveStatsFromItem(current);
                        Players.Data[ctx.User.Id].Inventory.RemoveAt(index);
                        Players.Data[ctx.User.Id].Inventory.Add(current);
                        await ctx.RespondAsync("Offhand weapon replaced successfully");
                    }
                    else
                        await ctx.RespondAsync("Timed out - no changes were made");
                    return;
                }
            }
            else if (item.Type == ItemType.Armor)
            {
                if (stype == ItemSlot.ArmorOne)
                {
                    var current = pl.ArmorOne;

                    await ctx.RespondAsync($"You already have armor equipped in slot one ({current.Name}) - respond with *confirm* to replace");
                    var rr = await ctx.Message.GetNextMessageAsync(i => i.Content.ToLowerInvariant() == "confirm");
                    if (!rr.TimedOut)
                    {
                        Players.Data[ctx.User.Id].ArmorOne = item;
                        Players.Data[ctx.User.Id].AddStatsFromItem(item);
                        Players.Data[ctx.User.Id].RemoveStatsFromItem(current);
                        Players.Data[ctx.User.Id].Inventory.RemoveAt(index);
                        Players.Data[ctx.User.Id].Inventory.Add(current);
                        await ctx.RespondAsync("Armor in slot one replaced successfully");
                    }
                    else
                        await ctx.RespondAsync("Timed out - no changes were made");
                    return;
                }
                else if (stype == ItemSlot.ArmorTwo)
                {
                    var current = pl.ArmorTwo;

                    await ctx.RespondAsync($"You already have armor equipped in slot two ({current.Name}) - respond with *confirm* to replace");
                    var rr = await ctx.Message.GetNextMessageAsync(i => i.Content.ToLowerInvariant() == "confirm");
                    if (!rr.TimedOut)
                    {
                        Players.Data[ctx.User.Id].ArmorTwo = item;
                        Players.Data[ctx.User.Id].AddStatsFromItem(item);
                        Players.Data[ctx.User.Id].RemoveStatsFromItem(current);
                        Players.Data[ctx.User.Id].Inventory.RemoveAt(index);
                        Players.Data[ctx.User.Id].Inventory.Add(current);
                        await ctx.RespondAsync("Armor in slot two replaced successfully");
                    }
                    else
                        await ctx.RespondAsync("Timed out - no changes were made");
                    return;
                }
                else if (stype == ItemSlot.ArmorThree)
                {
                    var current = pl.ArmorThree;

                    await ctx.RespondAsync($"You already have armor equipped in slot three ({current.Name}) - respond with *confirm* to replace");
                    var rr = await ctx.Message.GetNextMessageAsync(i => i.Content.ToLowerInvariant() == "confirm");
                    if (!rr.TimedOut)
                    {
                        Players.Data[ctx.User.Id].ArmorThree = item;
                        Players.Data[ctx.User.Id].AddStatsFromItem(item);
                        Players.Data[ctx.User.Id].RemoveStatsFromItem(current);
                        Players.Data[ctx.User.Id].Inventory.RemoveAt(index);
                        Players.Data[ctx.User.Id].Inventory.Add(current);
                        await ctx.RespondAsync("Armor in slot three replaced successfully");
                    }
                    else
                        await ctx.RespondAsync("Timed out - no changes were made");
                    return;
                }
            }
        }

        // Use after checking if the respective item slot is null (aka empty)
        protected async Task UnequipLogic(CommandContext ctx, ItemSlot slot)
        {
            Item i = ItemData.FailsafeLongSword; // failsafe should never be used.

            switch (slot)
            {
                case ItemSlot.ArmorOne:
                    i = Players.Data[ctx.User.Id].ArmorOne;
                    Players.Data[ctx.User.Id].ArmorOne = null;
                    await ctx.RespondAsync($"Item '{i.Name}' successfully unequipped from slot `armorone`");
                    break;
                case ItemSlot.ArmorTwo:
                    i = Players.Data[ctx.User.Id].ArmorTwo;
                    Players.Data[ctx.User.Id].ArmorTwo = null;
                    await ctx.RespondAsync($"Item '{i.Name}' successfully unequipped from slot `armortwo`");
                    break;
                case ItemSlot.ArmorThree:
                    i = Players.Data[ctx.User.Id].ArmorThree;
                    Players.Data[ctx.User.Id].ArmorThree = null;
                    await ctx.RespondAsync($"Item '{i.Name}' successfully unequipped from slot `armorthree`");
                    break;
                case ItemSlot.MainWeapon:
                    i = Players.Data[ctx.User.Id].MainWeapon;
                    Players.Data[ctx.User.Id].MainWeapon = null;
                    await ctx.RespondAsync($"Item '{i.Name}' successfully unequipped from slot `mainweapon`");
                    break;
                case ItemSlot.OffhandWeapon:
                    i = Players.Data[ctx.User.Id].OffhandWeapon;
                    Players.Data[ctx.User.Id].OffhandWeapon = null;
                    await ctx.RespondAsync($"Item '{i.Name}' successfully unequipped from slot `offhandweapon`");
                    break;
                case ItemSlot.Boots:
                    i = Players.Data[ctx.User.Id].Boots;
                    Players.Data[ctx.User.Id].Boots = null;
                    await ctx.RespondAsync($"Item '{i.Name}' successfully unequipped from slot `boots`");
                    break;
            }

            Players.Data[ctx.User.Id].Inventory.Add(i);
            Players.Data[ctx.User.Id].RemoveStatsFromItem(i);
        }

        protected async Task CombatRoutine(CommandContext ctx, Enemy e)
        {
            var pl = Players.Data[ctx.User.Id];
            int enemyEffectiveAr = e.Armor - (e.Armor * pl.ArmorPenPercent / 100) - pl.ArmorPenFlat;
            int enemyEffectiveMr = e.MagicResist - (e.Armor * pl.MagicPenPercent / 100) - pl.MagicPenFlat;

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Encounter: {e.Name}",
                Description = $"{e.Name} approaches you! What do you do?",
                Color = DefBlue
            };

            var resp = await ctx.RespondAsync(embed.Build());

            var swordEmoji = DiscordEmoji.FromName(ctx.Client, ":crossed_swords:");
            await resp.CreateReactionAsync(swordEmoji);
            while (true)
            {
                embed.ClearFields();
                // Player's turn
                embed.AddField("Your Health | Damage | Resists", $"{pl.Health}/{pl.MaxHealth} | {pl.AttackDamage} AD, {pl.AbilityPower} AP | {pl.Armor} AR. {pl.MagicResist} MR")
                    .AddField("Enemy Health | Damage | Resists", $"{e.Health}/{e.MaxHealth} | {e.AttackDamage} AD, {e.AbilityPower} AP | {e.Armor} AR, {e.MagicResist} MR");
                await resp.ModifyAsync(embed.Build());

                var result = await resp.WaitForReactionAsync(ctx.Member);
                if (!result.TimedOut)
                {
                    if (result.Result.Emoji == swordEmoji)
                    {
                        var dmgl = Rng.Next(pl.AttackDamage - (pl.AttackDamage * 25 / 100), pl.AttackDamage + (pl.AttackDamage * 25 / 100)) - enemyEffectiveAr;
                        e.Health -= dmgl;
                        if (e.Health <= 0)
                        {
                            embed.Description = $"You won fighting against {e.Name}! Implement rewards here...";
                            embed.Color = DefGreen;
                            embed.ClearFields();
                            embed.AddField("Your Health | Damage | Resists", $"{pl.Health}/{pl.MaxHealth} | {pl.AttackDamage} AD, {pl.AbilityPower} AP | {pl.Armor} AR. {pl.MagicResist} MR")
                                .AddField("Enemy Health | Damage | Resists", $"{0}/{e.MaxHealth} | {e.AttackDamage} AD, {e.AbilityPower} AP | {e.Armor} AR, {e.MagicResist} MR");
                            await resp.ModifyAsync(embed.Build());
                            return;
                            //break;
                        }
                        embed.Description = $"You deal {dmgl} to {e.Name}";
                        await resp.DeleteReactionAsync(swordEmoji, ctx.User);
                    }
                }
                else {
                    await resp.ModifyAsync(new DiscordEmbedBuilder { 
                        Title = "Encounter timed out. You took too long to make a decision - no rewards given",
                        Color = DefRed
                    }.Build());
                    return;
                }

                // Enemy's turn
                embed.Description += Environment.NewLine;
                embed.ClearFields();
                embed.AddField("Your Health | Damage | Resists", $"{pl.Health}/{pl.MaxHealth} | {pl.AttackDamage} AD, {pl.AbilityPower} AP | {pl.Armor} AR. {pl.MagicResist} MR")
                    .AddField("Enemy Health | Damage | Resists", $"{e.Health}/{e.MaxHealth} | {e.AttackDamage} AD, {e.AbilityPower} AP | {e.Armor} AR, {e.MagicResist} MR");
                await resp.ModifyAsync(embed.Build());

                var dmg = Rng.Next(e.AttackDamage - (e.AttackDamage * 25 / 100), e.AttackDamage + (e.AttackDamage * 25 / 100)) - pl.Armor;
                pl.Health -= dmg;
                if (pl.Health <= 0)
                {
                    embed.Description = $"You lost against {e.Name}... you got no rewards.";
                    embed.Color = DefRed;
                    embed.ClearFields();
                    embed.AddField("Your Health | Damage | Resists", $"{0}/{pl.MaxHealth} | {pl.AttackDamage} AD, {pl.AbilityPower} AP | {pl.Armor} AR. {pl.MagicResist} MR")
                        .AddField("Enemy Health | Damage | Resists", $"{e.Health}/{e.MaxHealth} | {e.AttackDamage} AD, {e.AbilityPower} AP | {e.Armor} AR, {e.MagicResist} MR");
                    Players.Data[ctx.User.Id].Health = pl.MaxHealth / 4;
                    await resp.ModifyAsync(embed.Build());
                    return;
                }
                embed.Description += $"{e.Name} did {dmg} to you";
            }
        }

        protected readonly DiscordColor DefGreen = new (55, 255, 119);
        protected readonly DiscordColor DefRed = new (255, 45, 0);
        protected readonly DiscordColor DefBlue = new (0, 191, 255);
    }
}

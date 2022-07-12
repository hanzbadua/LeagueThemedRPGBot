using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using LeagueThemedRPGBot.Game;

namespace LeagueThemedRPGBot.Commands
{
    // main game commands
    public partial class MainCommands : GameCommandModuleBase
    {
        [Command("init"), Description("Initialize your character")]
        public async Task Init(CommandContext ctx)
        {
            var msg = new DiscordEmbedBuilder().WithTitle("Character initialization");

            if (Players.IsInitedByID(ctx.User.Id))
            {
                await ctx.RespondAsync(msg
                    .WithColor(DefRed)
                    .WithDescription("You have already initialized your character")
                    .Build());
                return;
            }

            var assassinEmoji = DiscordEmoji.FromName(ctx.Client, ":dagger:");
            var bruiserEmoji = DiscordEmoji.FromName(ctx.Client, ":crossed_swords:");
            var marksmanEmoji = DiscordEmoji.FromName(ctx.Client, ":bow_and_arrow:");
            var mageEmoji = DiscordEmoji.FromName(ctx.Client, ":crystal_ball:");
            var battleMageEmoji = DiscordEmoji.FromName(ctx.Client, ":hourglass:");

            var req = await ctx.RespondAsync(msg
                .WithColor(DefBlue)
                .WithDescription("Initializing character... choose your starting class")
                .AddField("Classes", $"Assassin {assassinEmoji}, Bruiser/Skirmisher {bruiserEmoji}, Marksman {marksmanEmoji}, Burst Mage {mageEmoji}, Battlemage {battleMageEmoji}")
                .Build());
            await req.CreateReactionAsync(assassinEmoji);
            await req.CreateReactionAsync(bruiserEmoji);
            await req.CreateReactionAsync(marksmanEmoji);
            await req.CreateReactionAsync(mageEmoji);
            await req.CreateReactionAsync(battleMageEmoji);

            var res = await req.WaitForReactionAsync(ctx.Member);

            if (!res.TimedOut)
            {
                if (res.Result.Emoji == assassinEmoji)
                {
                    Players.Data[ctx.User.Id] = new Player();
                }
            }
            else
            {
                await req.DeleteAllReactionsAsync();
                await req.ModifyAsync(msg
                    .WithColor(DefRed)
                    .WithDescription("Timed out - no changes were made")
                    .ClearFields()
                    .Build());
                return;
            }
        }

        [Command("balance"), Aliases("bal"), Description("Check your current gold balance")]
        public async Task Balance(CommandContext ctx)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;

            await ctx.RespondAsync($"Current gold amount: {Players.Data[ctx.User.Id].Gold}");
        }

        [Command("stats"), Description("Check your current stats")]
        public async Task Stats(CommandContext ctx)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;

            var p = Players.Data[ctx.User.Id];

            var msg = new DiscordEmbedBuilder
            {
                Title = "Stats",
                Color = DefBlue
            };

            msg.AddField("Level, XP", $"{p.Level}, {p.XP}/{p.CalculateXPForNextLevel()}");
            msg.AddField("Health", $"{p.Health}/{p.MaxHealth}");
            msg.AddField("Mana", $"{p.Mana}/{p.MaxMana}");
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

            var p = Players.Data[ctx.User.Id];
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

            foreach (var i in Players.Data[ctx.User.Id].Inventory)
            {
                contents += $"{index}. {i.Name} ({Enum.GetName(i.Rarity)}, {Enum.GetName(i.Type)}){Environment.NewLine}";
                index++;
            }

            msg.AddField("Contents", contents);
            await ctx.RespondAsync(msg.Build());
        }

        [Command("inventory"), Description("View an item in your inventory via index")]
        public async Task Inventory(CommandContext ctx, [Description("Inventory index of the type to equip")] int count)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;
            if (await InventoryIsEmpty(ctx)) return;
            int index = count - 1; // internal indexes start at 0, for humans it starts at 1, so sub by 1
            if (!await ItemIndexIsValid(ctx, index)) return;

            var item = Players.Data[ctx.User.Id].Inventory[index];
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

        [Command("encounter"), Description("Maybe you'll find something worthwhile to fight")]
        public async Task Encounter(CommandContext ctx)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;

            Players.Data[ctx.User.Id].Busy = true;
            await CombatRoutine(ctx, Enemy.GetScalingEnemy(Players.Data[ctx.User.Id].Level, EncounterTypes.Common, Rng));
            Players.Data[ctx.User.Id].Busy = false;
        }

        [Command("rest"), Description("Resting restores your health and mana back to full")]
        public async Task Rest(CommandContext ctx)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;

            Players.Data[ctx.User.Id].Health = Players.Data[ctx.User.Id].MaxHealth;
            Players.Data[ctx.User.Id].Mana = Players.Data[ctx.User.Id].MaxMana;

            await ctx.RespondAsync("Resting makes you feel good again. Health and mana restored.");
        }

        // NOTE: remove later because it is a test command
        [Command("addweapon"), Description("Add an item by name")]
        public async Task AddWeapon(CommandContext ctx, [RemainingText] string name)
        {
            if (!await PlayerIsInited(ctx)) 
                return;

            Players.Data[ctx.User.Id].Inventory.Add(Items.GetWeaponByName(name));
        }
    }
}
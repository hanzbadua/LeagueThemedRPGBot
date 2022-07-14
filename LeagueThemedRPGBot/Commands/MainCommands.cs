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
            var msg = new DiscordEmbedBuilder { Title = "Character initialization"};

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
                .AddField("Classes", $"Assassin {assassinEmoji}, Bruiser {bruiserEmoji}, Marksman {marksmanEmoji}, Burst Mage {mageEmoji}, Battlemage {battleMageEmoji}")
                .Build());
            await req.CreateReactionAsync(assassinEmoji);
            await req.CreateReactionAsync(bruiserEmoji);
            await req.CreateReactionAsync(marksmanEmoji);
            await req.CreateReactionAsync(mageEmoji);
            await req.CreateReactionAsync(battleMageEmoji);

            var res = await req.WaitForReactionAsync(ctx.Member);
            var startingItem = new Item();
            if (!res.TimedOut)
            {
                if (res.Result.Emoji == assassinEmoji)
                {
                    startingItem = Refs.GetWeaponByName("(Barely) Serrated Dirk");
                    Players.Data[ctx.User.Id] = new()
                    {
                        MainWeapon = startingItem,
                        Skill1 = Refs.GetSkillByName("Double Slash")
                    };
                    Players.Data[ctx.User.Id].AddStatsFromItem(startingItem);
                    await req.DeleteAllReactionsAsync();
                    await req.ModifyAsync(
                        msg.WithDescription("You have decided to begin your journey as an... \"Assassin\"... if you can even call yourself that")
                        .ClearFields()
                        .WithColor(DefGreen)
                        .Build());
                }
                else if (res.Result.Emoji == bruiserEmoji)
                {
                    startingItem = Refs.GetWeaponByName("Doran's Blade");
                    Players.Data[ctx.User.Id] = new()
                    {
                        MainWeapon = startingItem
                    };
                    Players.Data[ctx.User.Id].AddStatsFromItem(startingItem);
                    await req.DeleteAllReactionsAsync();
                    await req.ModifyAsync(
                        msg.WithDescription("You have decided to begin your journey as a Bruiser, able to take extended fights and close skirmishes.")
                        .ClearFields()
                        .WithColor(DefGreen)
                        .Build());
                }
                else if (res.Result.Emoji == marksmanEmoji)
                {
                    startingItem = Refs.GetWeaponByName("Toy Noonquiver");
                    Players.Data[ctx.User.Id] = new()
                    {
                        MainWeapon = startingItem
                    };
                    Players.Data[ctx.User.Id].AddStatsFromItem(startingItem);
                    await req.DeleteAllReactionsAsync();
                    await req.ModifyAsync(
                        msg.WithDescription("You have decided to begin your journey as a Marksman; precise and delicate methods of... execution...")
                        .ClearFields()
                        .WithColor(DefGreen)
                        .Build());
                }
                else if (res.Result.Emoji == mageEmoji)
                {
                    startingItem = Refs.GetWeaponByName("Doran's Ring");
                    Players.Data[ctx.User.Id] = new()
                    {
                        MainWeapon = startingItem
                    };
                    Players.Data[ctx.User.Id].AddStatsFromItem(startingItem);
                    await req.DeleteAllReactionsAsync();
                    await req.ModifyAsync(
                        msg.WithDescription("You have decided to begin your journey as a Mage; little regard for your surroundings, you prefer to just blow things up with magic.")
                        .ClearFields()
                        .WithColor(DefGreen)
                        .Build());
                }
                else if (res.Result.Emoji == battleMageEmoji)
                {
                    startingItem = Refs.GetWeaponByName("Dark Seal");
                    Players.Data[ctx.User.Id] = new()
                    {
                        MainWeapon = startingItem
                    };
                    Players.Data[ctx.User.Id].AddStatsFromItem(startingItem);
                    await req.DeleteAllReactionsAsync();
                    await req.ModifyAsync(
                        msg.WithDescription("You have decided to begin your journey as a Battlemage; durable AND flashy!")
                        .ClearFields()
                        .WithColor(DefGreen)
                        .Build());
                }
            }
            else
            {
                await req.DeleteAllReactionsAsync();
                await req.ModifyAsync(
                    msg.WithColor(DefRed)
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
            msg.WithFooter($"to check your inventory, use '$inventory'{NL}to check your gold, use '$balance'{NL}to check your equipped items, use '$equipped'");

            await ctx.RespondAsync(msg.Build());
        }

        [Command("equipped"), Description("Check your currently equipped items and currently learned skills")]
        public async Task Equipped(CommandContext ctx)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;

            var p = Players.Data[ctx.User.Id];
            const string na = "N/A";

            var msg = new DiscordEmbedBuilder
            {
                Title = "Equipped items + currently learned skills",
                Color = DefBlue
            };

            msg.AddField("Main Weapon", p.MainWeapon is not null ? p.MainWeapon.Name : na)
                .AddField("Offhand Weapon", p.OffhandWeapon is not null ? p.OffhandWeapon.Name : na)
                .AddField("Armor (1)", p.Armor1 is not null ? p.Armor1.Name : na)
                .AddField("Armor (2)", p.Armor2 is not null ? p.Armor2.Name : na)
                .AddField("Armor (3)", p.Armor3 is not null ? p.Armor3.Name : na)
                .AddField("Boots", p.Boots is not null ? p.Boots.Name : na)
                .AddField("Skill (1)", p.Skill1 is not null ? p.Skill1.Name : na)
                .AddField("Skill (2)", p.Skill2 is not null ? p.Skill2.Name : na)
                .AddField("Skill (3)", p.Skill3 is not null ? p.Skill3.Name : na);
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

            Players.Data[ctx.User.Id].Inventory.Add(Refs.GetWeaponByName(name));
        }
    }
}
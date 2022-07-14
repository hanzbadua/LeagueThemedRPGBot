using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using LeagueThemedRPGBot.Game;

namespace LeagueThemedRPGBot.Commands
{
    // Main combat routines and combat-related functions
    public abstract partial class GameCommandModuleBase : BaseCommandModule
    {
        protected async Task CombatRoutine(CommandContext ctx, Enemy e)
        {
            var pl = Players.Data[ctx.User.Id];

            var embed = new DiscordEmbedBuilder
            {
                Title = $"Encounter: {e.Name}",
                Description = $"{e.Name} approaches you! What do you do?",
                Color = DefBlue
            };

            var resp = await ctx.RespondAsync(embed.Build());
            var swordEmoji = DiscordEmoji.FromName(ctx.Client, ":crossed_swords:");
            var oneEmoji = DiscordEmoji.FromName(ctx.Client, ":one:");
            var twoEmoji = DiscordEmoji.FromName(ctx.Client, ":two:");
            var threeEmoji = DiscordEmoji.FromName(ctx.Client, ":three:");

            await resp.CreateReactionAsync(swordEmoji);

            if (pl.Skill1 is not null) await resp.CreateReactionAsync(oneEmoji);
            if (pl.Skill2 is not null) await resp.CreateReactionAsync(twoEmoji);
            if (pl.Skill3 is not null) await resp.CreateReactionAsync(threeEmoji);

            while (true)
            {
                // Player's turn
                UpdateDisplayValues(embed, pl, e);

            redo:
                await resp.ModifyAsync(embed.Build()); 
                var result = await resp.WaitForReactionAsync(ctx.Member);
                if (!result.TimedOut)
                {
                    if (result.Result.Emoji == swordEmoji)
                    {
                        var dmglp = Rng.Next(pl.AttackDamage - (pl.AttackDamage * 25 / 100), pl.AttackDamage + (pl.AttackDamage * 25 / 100));
                        bool crit = CriticalStrike(ref dmglp, pl);
                        PostEnemyPhysicalMitigations(ref dmglp, pl, e);

                        e.Health -= dmglp;

                        if (await CombatCheckVictory(ctx, pl, e, resp, embed)) return;

                        if (crit)
                            embed.Description = $"You critically striked {e.Name} for {dmglp} damage!";
                        else
                            embed.Description = $"You deal {dmglp} damage to {e.Name}";

                        await resp.DeleteReactionAsync(swordEmoji, ctx.User);
                    }
                    else if (result.Result.Emoji == oneEmoji && pl.Skill1 is not null)
                    {
                        if (pl.Mana < pl.Skill1.ManaCost)
                        {
                            embed.Description = $"You don't have enough mana to cast {pl.Skill1.Name}, please choose another action";
                            await resp.DeleteReactionAsync(oneEmoji, ctx.User);
                            goto redo;
                        }

                        pl.Mana -= pl.Skill1.ManaCost;

                        // WRAP->NEW METHOD. PLS.
                        // Deal two quick slashes to your target, each strike doing base damage equal to 45% total AD, with each strike doing 5% more per 1 flat armor pen
                        if (pl.Skill1.Effect == SkillEffect.AssassinDoubleSlash)
                        {
                            var strike = ((pl.AttackDamage * (45 + (5 * pl.ArmorPenFlat))) / 100) * 2;
                            PostEnemyPhysicalMitigations(ref strike, pl, e);
                            e.Health -= strike;
                            if (await CombatCheckVictory(ctx, pl, e, resp, embed)) return;
                            embed.Description = $"You slashed twice precisely, dealing a total of {strike} damage";
                            await resp.DeleteReactionAsync(oneEmoji, ctx.User);
                        }
                    }
                }
                else
                {
                    await resp.ModifyAsync(new DiscordEmbedBuilder
                    {
                        Title = "Encounter timed out. You took too long to make a decision - no rewards given",
                        Color = DefRed
                    }.Build());
                    return;
                }

                embed.Description += Environment.NewLine;

                // Enemy's turn
                UpdateDisplayValues(embed, pl, e);
                await resp.ModifyAsync(embed.Build());

                var dmg = Rng.Next(e.AttackDamage - (e.AttackDamage * 25 / 100), e.AttackDamage + (e.AttackDamage * 25 / 100)) - pl.Armor;
                pl.Health -= dmg;

                if (await CombatCheckLoss(ctx, pl, e, resp, embed)) return;

                embed.Description += $"{e.Name} did {dmg} to you";
            }
        }

        // Functions which help with combat related math
        // modify = damage number to modify
        // p = player
        // e = enemy in combat context

        private void PostEnemyPhysicalMitigations(ref int modify, Player p, Enemy e)
        {
            int enemyEffectiveAr = e.Armor - (e.Armor * p.ArmorPenPercent / 100) - p.ArmorPenFlat;
            if (enemyEffectiveAr < 0) enemyEffectiveAr = 0;
            if (modify - enemyEffectiveAr <= 0)
                modify = 1;
            else
                modify -= enemyEffectiveAr;

        }

        private void PostEnemyMagicalMitigations(ref int modify, Player p, Enemy e)
        {
            int enemyEffectiveMr = e.MagicResist - (e.MagicResist * p.MagicPenPercent / 100) - p.MagicPenFlat;
            if (enemyEffectiveMr < 0) enemyEffectiveMr = 0;
            if (modify - enemyEffectiveMr <= 0)
                modify = 1;
            else
                modify -= enemyEffectiveMr;
        }

        private bool CriticalStrike(ref int modify, Player p)
        {
            if (p.CritChance >= Rng.Next(0, 101))
            {
                modify += (modify * (75 + p.BonusCritDamage) / 100);
                return true;
            }

            return false;
        }

        // other combat related functions idk read the method names
        private void UpdateDisplayValues(DiscordEmbedBuilder msg, Player p, Enemy e)
        {
            int strippedEnemyAr = e.Armor - (e.Armor * p.ArmorPenPercent / 100) - p.ArmorPenFlat;
            int strippedEnemyMr = e.MagicResist - (e.MagicResist * p.MagicPenPercent / 100) - p.MagicPenFlat;

            if (strippedEnemyAr > e.Armor) strippedEnemyAr = 0;
            if (strippedEnemyMr > e.MagicResist) strippedEnemyMr = 0;

            msg.ClearFields()
                .AddField("Your HP | Damage | Resists", $"{p.Health}/{p.MaxHealth} | {p.AttackDamage} AD, {p.AbilityPower} AP | {p.Armor} AR, {p.MagicResist} MR")
                .AddField($"{e.Name}'s HP | Damage | Resists", $"{e.Health}/{e.MaxHealth} | {e.AttackDamage} AD, {e.AbilityPower} AP | {e.Armor} AR ({strippedEnemyAr}), {e.MagicResist} MR ({strippedEnemyMr})");
        }

        private async Task<bool> CombatCheckLoss(CommandContext ctx, Player p, Enemy e, DiscordMessage toModify, DiscordEmbedBuilder modify)
        {
            if (p.Health <= 0)
            {
                modify.WithDescription($"You lost against {e.Name}... you got no rewards")
                    .WithColor(DefRed);
                UpdateDisplayValues(modify, p, e);
                Players.Data[ctx.User.Id].Health = p.MaxHealth / 4;
                await toModify.ModifyAsync(modify.Build());
                return true;
            }

            return false;
        }

        private async Task<bool> CombatCheckVictory(CommandContext ctx, Player p, Enemy e, DiscordMessage toModify, DiscordEmbedBuilder modify)
        {
            if (e.Health <= 0)
            {
                modify.WithDescription($"You won fighting against {e.Name}! Implement rewards here...")
                    .WithColor(DefGreen)
                    .ClearFields()
                    .AddField("Your Health | Damage | Resists", $"{p.Health}/{p.MaxHealth} | {p.AttackDamage} AD, {p.AbilityPower} AP | {p.Armor} AR. {p.MagicResist} MR")
                    .AddField("Enemy Health | Damage | Resists", $"{0}/{e.MaxHealth} | {e.AttackDamage} AD, {e.AbilityPower} AP | {e.Armor} AR, {e.MagicResist} MR");
                await toModify.ModifyAsync(modify.Build());
                return true;
            }

            return false;
        }
    }
}

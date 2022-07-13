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
            int enemyEffectiveAr = e.Armor - (e.Armor * pl.ArmorPenPercent / 100) - pl.ArmorPenFlat;
            int enemyEffectiveMr = e.MagicResist - (e.Armor * pl.MagicPenPercent / 100) - pl.MagicPenFlat;

            if (enemyEffectiveAr <= 0) enemyEffectiveAr = 0;
            if (enemyEffectiveMr <= 0) enemyEffectiveMr = 0;

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
                embed.ClearFields();
                // Player's turn
                embed.AddField("Your Health | Damage | Resists", $"{pl.Health}/{pl.MaxHealth} | {pl.AttackDamage} AD, {pl.AbilityPower} AP | {pl.Armor} AR. {pl.MagicResist} MR")
                    .AddField("Enemy Health | Damage | Resists", $"{e.Health}/{e.MaxHealth} | {e.AttackDamage} AD, {e.AbilityPower} AP | {e.Armor} AR, {e.MagicResist} MR");

            redo:
                await resp.ModifyAsync(embed.Build()); 
                var result = await resp.WaitForReactionAsync(ctx.Member);
                if (!result.TimedOut)
                {
                    if (result.Result.Emoji == swordEmoji)
                    {
                        bool crit = false;
                        var dmglp = Rng.Next(pl.AttackDamage - (pl.AttackDamage * 25 / 100), pl.AttackDamage + (pl.AttackDamage * 25 / 100)) - enemyEffectiveAr;
                        if (pl.CritChance >= Rng.Next(0, 101))
                        {
                            dmglp += (dmglp * (75 + pl.BonusCritDamage) / 100);
                            crit = true;
                        }
                        e.Health -= dmglp;

                        //towrap->method
                        if (e.Health <= 0)
                        {
                            embed.WithDescription($"You won fighting against {e.Name}! Implement rewards here...")
                                .WithColor(DefGreen)
                                .ClearFields()
                                .AddField("Your Health | Damage | Resists", $"{pl.Health}/{pl.MaxHealth} | {pl.AttackDamage} AD, {pl.AbilityPower} AP | {pl.Armor} AR. {pl.MagicResist} MR")
                                .AddField("Enemy Health | Damage | Resists", $"{0}/{e.MaxHealth} | {e.AttackDamage} AD, {e.AbilityPower} AP | {e.Armor} AR, {e.MagicResist} MR");
                            await resp.ModifyAsync(embed.Build());
                            return;
                            //break;
                        }

                        if (crit)
                            embed.Description = $"You critically striked {dmglp} to {e.Name}";
                        else
                            embed.Description = $"You deal {dmglp} to {e.Name}";

                        await resp.DeleteReactionAsync(swordEmoji, ctx.User);
                    }
                    else if (result.Result.Emoji == oneEmoji && pl.Skill1 is not null)
                    {
                        var s = pl.Skill1;
                        if (pl.Mana < pl.Skill1.ManaCost)
                        {
                            embed.Description = $"You don't have enough mana to cast {pl.Skill1.Name}, please choose another action";
                            await resp.DeleteReactionAsync(oneEmoji, ctx.User);
                            goto redo;
                        }

                        pl.Mana -= pl.Skill1.ManaCost;

                        if (pl.Skill1.Effect == SkillEffect.AssassinDoubleSlash)
                        {
                            var strike = ((pl.AttackDamage * (45 + (5 * pl.ArmorPenFlat))) / 100);
                            e.Health -= strike * 2;
                            //towrap->method
                            if (e.Health <= 0)
                            {
                                embed.WithDescription($"You won fighting against {e.Name}! Implement rewards here...")
                                    .WithColor(DefGreen)
                                    .ClearFields()
                                    .AddField("Your Health | Damage | Resists", $"{pl.Health}/{pl.MaxHealth} | {pl.AttackDamage} AD, {pl.AbilityPower} AP | {pl.Armor} AR. {pl.MagicResist} MR")
                                    .AddField("Enemy Health | Damage | Resists", $"{0}/{e.MaxHealth} | {e.AttackDamage} AD, {e.AbilityPower} AP | {e.Armor} AR, {e.MagicResist} MR");
                                await resp.ModifyAsync(embed.Build());
                                return;
                                //break;
                            }
                            embed.Description = $"You slashed twice, each slash doing {strike} damage for a total of {strike * 2} damage";
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
                embed.ClearFields();
                embed.AddField("Your Health | Damage | Resists", $"{pl.Health}/{pl.MaxHealth} | {pl.AttackDamage} AD, {pl.AbilityPower} AP | {pl.Armor} AR. {pl.MagicResist} MR")
                    .AddField("Enemy Health | Damage | Resists", $"{e.Health}/{e.MaxHealth} | {e.AttackDamage} AD, {e.AbilityPower} AP | {e.Armor} AR, {e.MagicResist} MR");
                await resp.ModifyAsync(embed.Build());

                var dmg = Rng.Next(e.AttackDamage - (e.AttackDamage * 25 / 100), e.AttackDamage + (e.AttackDamage * 25 / 100)) - pl.Armor;
                pl.Health -= dmg;

                if (await CombatCheckLoss(ctx, pl, e, resp, embed)) return;

                embed.Description += $"{e.Name} did {dmg} to you";
            }
        }

        private async Task<bool> CombatCheckLoss(CommandContext ctx, Player pl, Enemy e, DiscordMessage toModify, DiscordEmbedBuilder modify)
        {
            if (pl.Health <= 0)
            {
                modify.WithDescription($"You lost against {e.Name}... you got no rewards")
                    .WithColor(DefRed)
                    .ClearFields()
                    .AddField("Your Health | Damage | Resists", $"0/{pl.MaxHealth} | {pl.AttackDamage} AD, {pl.AbilityPower} AP | {pl.Armor} AR. {pl.MagicResist} MR")
                    .AddField("Enemy Health | Damage | Resists", $"{e.Health}/{e.MaxHealth} | {e.AttackDamage} AD, {e.AbilityPower} AP | {e.Armor} AR, {e.MagicResist} MR");
                Players.Data[ctx.User.Id].Health = pl.MaxHealth / 4;
                await toModify.ModifyAsync(modify.Build());
                return true;
            }

            return false;
        }

    }
}

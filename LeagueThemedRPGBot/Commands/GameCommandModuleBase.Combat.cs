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
                UpdateDisplayValues(ctx, embed, pl, e);

                await resp.ModifyAsync(embed.Build()); 
                var result = await resp.WaitForReactionAsync(ctx.Member);
                if (!result.TimedOut)
                {
                    if (result.Result.Emoji == swordEmoji)
                    {
                        GetBasicAttack(pl, e, out bool crit, out int aa);

                        e.Health -= aa;

                        if (crit)
                            embed.Description = $"You critically striked {e.Name} for {aa} damage!";
                        else
                            embed.Description = $"You deal {aa} damage to {e.Name}";

                        await resp.DeleteReactionAsync(swordEmoji, ctx.User);
                    }
                    else if (result.Result.Emoji == oneEmoji && pl.Skill1 is not null)
                    {
                        if (pl.Mana < pl.Skill1.ManaCost)
                        {
                            embed.WithDescription($"You don't have enough mana to cast {pl.Skill1.Name} ({pl.Mana}/{pl.Skill1.ManaCost}), please choose another action");
                            await resp.DeleteReactionAsync(oneEmoji, ctx.User);
                            continue;
                        }

                        pl.Mana -= pl.Skill1.ManaCost;

                        SkillEffectActions(pl.Skill1.Effect, pl, e, embed);
                        await resp.DeleteReactionAsync(oneEmoji, ctx.User);
                    }
                }
                else
                {
                    await resp.ModifyAsync(new DiscordEmbedBuilder
                    {
                        Title = "Encounter timed out. You took too long to make a decision - no rewards given",
                        Color = DefRed
                    }.Build());
                    await resp.DeleteAllReactionsAsync();
                    return;
                }

                if (await CombatCheckVictory(ctx, pl, e, resp, embed)) return;
                embed.Description += NL;

                // Enemy's turn
                UpdateDisplayValues(ctx, embed, pl, e);
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
        private void GetBasicAttack(Player p, Enemy e, out bool wasCrit, out int result)
        {
            var aa = Rng.Next(p.AttackDamage - (p.AttackDamage * 25 / 100), p.AttackDamage + (p.AttackDamage * 25 / 100));
            wasCrit = CriticalStrike(ref aa, p);
            PostEnemyPhysicalMitigations(ref aa, p, e);
            result = aa;
        }


        private void SkillEffectActions(SkillEffect s, Player p, Enemy e, DiscordEmbedBuilder toModify)
        {
            int dmg;

            // Deal two quick slashes to your target, each strike doing base damage equal to 45% total AD, with each strike doing 5% more per 1 flat armor pen
            if (s == SkillEffect.AssassinDoubleSlash)
            {
                // This is code for one strike - we multiply it by 2 later for actual damage in order to take account for mitigations per strike
                dmg = ((p.AttackDamage * (45 + (5 * p.ArmorPenFlat))) / 100);
                PostEnemyPhysicalMitigations(ref dmg, p, e);
                e.Health -= dmg * 2;
                toModify.WithDescription($"You slashed twice precisely, dealing a total of {dmg * 2} damage");
                return;
            }
        }

        private void UpdateDisplayValues(CommandContext ctx, DiscordEmbedBuilder msg, Player p, Enemy e)
        {
            int strippedEnemyAr = e.Armor - (e.Armor * p.ArmorPenPercent / 100) - p.ArmorPenFlat;
            int strippedEnemyMr = e.MagicResist - (e.MagicResist * p.MagicPenPercent / 100) - p.MagicPenFlat;

            if (strippedEnemyAr < 0) strippedEnemyAr = 0;
            if (strippedEnemyMr < 0) strippedEnemyMr = 0;

            if (p.Health < 0) p.Health = 0;
            if (e.Health < 0) e.Health = 0;

            // See footer note below
            // var swordEmoji = DiscordEmoji.FromName(ctx.Client, ":crossed_swords:");
            // var oneEmoji = DiscordEmoji.FromName(ctx.Client, ":one:");
            // var twoEmoji = DiscordEmoji.FromName(ctx.Client, ":two:");
            // var threeEmoji = DiscordEmoji.FromName(ctx.Client, ":three:");
            //
            var hpEmoji = DiscordEmoji.FromName(ctx.Client, ":heart:");
            var manaEmoji = DiscordEmoji.FromName(ctx.Client, ":droplet:");
            var adEmoji = DiscordEmoji.FromName(ctx.Client, ":knife:");
            var apEmoji = DiscordEmoji.FromName(ctx.Client, ":star:");
            var arEmoji = DiscordEmoji.FromName(ctx.Client, ":shield:");
            var mrEmoji = DiscordEmoji.FromName(ctx.Client, ":zap:");

            msg.ClearFields()
                // Embed footers in Discord don't support Twemoji, so it looks kinda ugly...
                //.WithFooter($"Health {hpEmoji}, Mana {manaEmoji}, Attack Damage {adEmoji}, Ability Power {apEmoji}, Armor {arEmoji}, Magic Resist {mrEmoji}{NL}Basic Attack {swordEmoji}, Skills {oneEmoji}{twoEmoji}{threeEmoji}")
                .AddField("Your stats", $"{hpEmoji} {p.Health}/{p.MaxHealth}{NL}{manaEmoji} {p.Mana}/{p.MaxMana}{NL}{adEmoji} {p.AttackDamage}, {apEmoji} {p.AbilityPower}{NL}{arEmoji} {p.Armor}, {mrEmoji} {p.MagicResist}")
                .AddField($"{e.Name}'s stats", $"{hpEmoji} {e.Health}/{e.MaxHealth}{NL}{adEmoji} {e.AttackDamage}, {apEmoji} {e.AbilityPower}{NL}{arEmoji} {e.Armor} ({strippedEnemyAr}), {mrEmoji} {e.MagicResist} ({strippedEnemyMr})");
        }

        private async Task<bool> CombatCheckLoss(CommandContext ctx, Player p, Enemy e, DiscordMessage toModify, DiscordEmbedBuilder modify)
        {
            if (p.Health <= 0)
            {
                modify.Description += $"{NL}You lost against {e.Name}... you got no rewards";
                modify.WithColor(DefRed);
                UpdateDisplayValues(ctx, modify, p, e);
                Players.Data[ctx.User.Id].Health = p.MaxHealth / 4;
                await toModify.ModifyAsync(modify.Build());
                await toModify.DeleteAllReactionsAsync();
                return true;
            }

            return false;
        }

        private async Task<bool> CombatCheckVictory(CommandContext ctx, Player p, Enemy e, DiscordMessage toModify, DiscordEmbedBuilder modify)
        {
            if (e.Health <= 0)
            {
                modify.Description += $"{NL}You won fighting against {e.Name}! Implement rewards here...";
                modify.WithColor(DefGreen);
                UpdateDisplayValues(ctx, modify, p, e);
                await toModify.ModifyAsync(modify.Build());
                await toModify.DeleteAllReactionsAsync();
                return true;
            }

            return false;
        }

        protected readonly string NL = Environment.NewLine;
    }
}

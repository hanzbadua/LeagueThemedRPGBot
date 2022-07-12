using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace LeagueThemedRPGBot.Commands
{
    // skill related cmds
    public partial class MainCommands
    {
        [Command("skills"), Description("View your known skill collection")]
        public async Task Skills(CommandContext ctx)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;
            if (await SkillsIsEmpty(ctx)) return;

            string contents = "";
            int index = 1;

            foreach (var i in Players.Data[ctx.User.Id].KnownSkills)
            {
                contents += $"{index}. {i.Name}{Environment.NewLine}";
                index++;
            }

            var msg = new DiscordEmbedBuilder
            {
                Title = "Known Skills",
                Color = DefBlue,
                Description = contents
            };

            await ctx.RespondAsync(msg.Build());
        }

        [Command("skills"), Description("View a skill in your known skills collection")]
        public async Task Skills(CommandContext ctx, [Description("Index of the skill to view")] int count)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;
            if (await SkillsIsEmpty(ctx)) return;
            int index = count - 1; // internal indexes start at 0, for humans it starts at 1, so sub by 1
            if (!await KnownSkillIndexIsValid(ctx, index)) return;

            var skill = Players.Data[ctx.User.Id].KnownSkills[index];
            var msg = new DiscordEmbedBuilder { Title = $"Viewing skill: {skill.Name}", Color = DefBlue, Description = skill.Description};

            await ctx.RespondAsync(msg.Build());
        }

        [Command("learn")]
        public async Task Learn(CommandContext ctx)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;
            await ctx.RespondAsync("You need to specify a skill to learn");
        }

        [Command("learn"), Description("Learn a skill in your known skills collection")]
        public async Task Learn(CommandContext ctx, [Description("Index of the skill to learn")] int count)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;
            if (await SkillsIsEmpty(ctx)) return;
            int index = count - 1; // internal indexes start at 0, for humans it starts at 1, so sub by 1
            if (!await KnownSkillIndexIsValid(ctx, index)) return;

            var player = Players.Data[ctx.User.Id];
            var skill = player.KnownSkills[index];

            Players.Data[ctx.User.Id].Busy = true;

            var msg = new DiscordEmbedBuilder { Title = $"Learning skill {skill.Name}", Color = DefBlue };
            var req = await ctx.RespondAsync(msg);
            var oneEmoji = DiscordEmoji.FromName(ctx.Client, ":one:");
            var twoEmoji = DiscordEmoji.FromName(ctx.Client, ":two:");
            var threeEmoji = DiscordEmoji.FromName(ctx.Client, ":three:");

            await req.CreateReactionAsync(oneEmoji);
            await req.CreateReactionAsync(twoEmoji);
            await req.CreateReactionAsync(threeEmoji);

            var res = await req.WaitForReactionAsync(ctx.Member);

            if (!res.TimedOut)
            {
                if (res.Result.Emoji == oneEmoji)
                {
                    if (player.Skill1 is null)
                    {
                        await req.ModifyAsync(msg.WithTitle($"Skill {skill.Name} learned in slot one").WithColor(DefGreen).Build());
                        await req.DeleteAllReactionsAsync();

                        Players.Data[ctx.User.Id].Skill1 = skill;
                        Players.Data[ctx.User.Id].KnownSkills.RemoveAt(index);
                    }
                    else
                    {
                        await req.ModifyAsync(msg
                            .WithTitle($"Skill {skill.Name} learned in slot one")
                            .WithDescription($"NOTE: you already had a skill in this slot, replacing current skill {player.Skill1.Name} with {skill.Name}")
                            .WithColor(DefGreen)
                            .Build());
                        await req.DeleteAllReactionsAsync();

                        Players.Data[ctx.User.Id].KnownSkills.Add(player.Skill1);
                        Players.Data[ctx.User.Id].Skill1 = skill;
                        Players.Data[ctx.User.Id].KnownSkills.RemoveAt(index);
                    }
                }
                else if (res.Result.Emoji == twoEmoji)
                {
                    if (player.Skill2 is null)
                    {
                        await req.ModifyAsync(msg.WithTitle($"Skill {skill.Name} learned in slot two").WithColor(DefGreen).Build());
                        await req.DeleteAllReactionsAsync();

                        Players.Data[ctx.User.Id].Skill2 = skill;
                        Players.Data[ctx.User.Id].KnownSkills.RemoveAt(index);
                    }
                    else
                    {
                        await req.ModifyAsync(msg
                            .WithTitle($"Skill {skill.Name} learned in slot two")
                            .WithDescription($"NOTE: you already had a skill in this slot, replacing current skill {player.Skill2.Name} with {skill.Name}")
                            .WithColor(DefGreen)
                            .Build());
                        await req.DeleteAllReactionsAsync();

                        Players.Data[ctx.User.Id].KnownSkills.Add(player.Skill2);
                        Players.Data[ctx.User.Id].Skill2 = skill;
                        Players.Data[ctx.User.Id].KnownSkills.RemoveAt(index);
                    }
                }
                else if (res.Result.Emoji == threeEmoji)
                {
                    if (player.Skill3 is null)
                    {
                        await req.ModifyAsync(msg.WithTitle($"Skill {skill.Name} learned in slot three").WithColor(DefGreen).Build());
                        await req.DeleteAllReactionsAsync();

                        Players.Data[ctx.User.Id].Skill3 = skill;
                        Players.Data[ctx.User.Id].KnownSkills.RemoveAt(index);
                    }
                    else
                    {
                        await req.ModifyAsync(msg
                            .WithTitle($"Skill {skill.Name} learned in slot three")
                            .WithDescription($"NOTE: you already had a skill in this slot, replacing current skill {player.Skill3.Name} with {skill.Name}")
                            .WithColor(DefGreen)
                            .Build());
                        await req.DeleteAllReactionsAsync();

                        Players.Data[ctx.User.Id].KnownSkills.Add(player.Skill3);
                        Players.Data[ctx.User.Id].Skill3 = skill;
                        Players.Data[ctx.User.Id].KnownSkills.RemoveAt(index);
                    }
                }
            }
            else 
            {
                await ctx.RespondAsync(msg.WithDescription("Timed out - no changes were made").WithColor(DefRed).Build());
                await req.DeleteAllReactionsAsync();
            }

            Players.Data[ctx.User.Id].Busy = false;
        }

        [Command("unlearn"), Description("Unlearn a currently learned skill and store the skill back in your known skills collection")]
        public async Task Unlearn(CommandContext ctx)
        {
            if (!await PlayerIsInited(ctx)) return;
            if (await PlayerIsBusy(ctx)) return;

            Players.Data[ctx.User.Id].Busy = true;

            var player = Players.Data[ctx.User.Id];
            var msg = new DiscordEmbedBuilder { Title = "Unlearning skill...", Description = "Choose a skill slot to unlearn its skill from", Color = DefBlue };
            var req = await ctx.RespondAsync(msg);
            var oneEmoji = DiscordEmoji.FromName(ctx.Client, ":one:");
            var twoEmoji = DiscordEmoji.FromName(ctx.Client, ":two:");
            var threeEmoji = DiscordEmoji.FromName(ctx.Client, ":three:");

            await req.CreateReactionAsync(oneEmoji);
            await req.CreateReactionAsync(twoEmoji);
            await req.CreateReactionAsync(threeEmoji);

            var res = await req.WaitForReactionAsync(ctx.Member);

            if (!res.TimedOut)
            {
                if (res.Result.Emoji == oneEmoji)
                {
                    if (player.Skill1 is not null)
                    {
                        await req.ModifyAsync(msg.WithDescription($"Skill {player.Skill1.Name} unlearned in slot one").WithColor(DefGreen).Build());
                        await req.DeleteAllReactionsAsync();
                        Players.Data[ctx.User.Id].KnownSkills.Add(Players.Data[ctx.User.Id].Skill1);
                        Players.Data[ctx.User.Id].Skill1 = null;
                    }
                    else 
                    {
                        await req.ModifyAsync(msg.WithDescription($"There is no skill to unlearn in slot one - no changes made").WithColor(DefRed).Build());
                        await req.DeleteAllReactionsAsync();
                    }
                }
                else if (res.Result.Emoji == twoEmoji)
                {
                    if (player.Skill2 is not null)
                    {
                        await req.ModifyAsync(msg.WithDescription($"Skill {player.Skill2.Name} unlearned in slot two").WithColor(DefGreen).Build());
                        await req.DeleteAllReactionsAsync();
                        Players.Data[ctx.User.Id].KnownSkills.Add(Players.Data[ctx.User.Id].Skill2);
                        Players.Data[ctx.User.Id].Skill2 = null;
                    }
                    else
                    {
                        await req.ModifyAsync(msg.WithDescription($"There is no skill to unlearn in slot two - no changes made").WithColor(DefRed).Build());
                        await req.DeleteAllReactionsAsync();
                    }
                }
                else if (res.Result.Emoji == threeEmoji) 
                {
                    if (player.Skill3 is not null)
                    {
                        await req.ModifyAsync(msg.WithDescription($"Skill {player.Skill3.Name} unlearned in slot three").WithColor(DefGreen).Build());
                        await req.DeleteAllReactionsAsync();
                        Players.Data[ctx.User.Id].KnownSkills.Add(Players.Data[ctx.User.Id].Skill3);
                        Players.Data[ctx.User.Id].Skill3 = null;
                    }
                    else
                    {
                        await req.ModifyAsync(msg.WithDescription($"There is no skill to unlearn in slot three - no changes made").WithColor(DefRed).Build());
                        await req.DeleteAllReactionsAsync();
                    }
                }
            }
            else
            {
                await req.ModifyAsync(msg.WithDescription("Timed out - no changes were made").WithColor(DefRed).Build());
                await req.DeleteAllReactionsAsync();
            }

            Players.Data[ctx.User.Id].Busy = false;
        }
    }
}

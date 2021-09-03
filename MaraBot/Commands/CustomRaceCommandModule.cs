using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MaraBot.Core;
using MaraBot.Messages;

namespace MaraBot.Commands
{
    public class CustomRaceCommandModule : BaseCommandModule
    {
        public IReadOnlyDictionary<string, Option> Options { private get; set; }
        public IConfig Config { private get; set; }

        [Command("custom")]
        [Cooldown(3, 600, CooldownBucketType.User)]
        [RequireGuild]
        public async Task Execute(CommandContext ctx)
        {
            // Safety measure to avoid potential misuses of this command. May be revisited in the future.
            if (!ctx.Member.Roles.Any(role => (Config.OrganizerRoles?.Contains(role.Name)).GetValueOrDefault()))
            {
                var guildRoles = ctx.Guild.Roles
                    .Where(role => (Config.OrganizerRoles?.Contains(role.Value.Name)).GetValueOrDefault());

                await ctx.RespondAsync(
                    "Insufficient privileges to create a custom race.\n" +
                    "This command is only available to the following roles:\n" +
                    String.Join(
                        ", ",
                        await CommandUtils.MentionRoleWithoutPing(ctx, guildRoles.Select(r => r.Value).ToArray())
                    )
                );
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            if (ctx.Message.Attachments == null || ctx.Message.Attachments.Count != 1)
            {
                await ctx.RespondAsync("No attachment for custom race. You must supply a valid json file.");
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            var attachment = ctx.Message.Attachments[0];
            var url = attachment.Url;

            var request = WebRequest.Create(url);
            var responseTask = request.GetResponseAsync();

            if (Path.GetExtension(attachment.FileName).ToLower() != ".json")
            {
                await ctx.RespondAsync("Expected file extension to be .json.");
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            var response = await responseTask;
            var dataStream = response.GetResponseStream();

            using (StreamReader r = new StreamReader(dataStream))
            {
                var jsonContent = await r.ReadToEndAsync();
                var preset = PresetIO.LoadPreset(jsonContent, Options);
                if (preset == null)
                {
                    await ctx.RespondAsync($"Could not parse custom json preset. Please supply a valid json file.");
                    await CommandUtils.SendFailReaction(ctx);
                    return;
                }

                var seed = RandomUtils.GetRandomSeed();
                await Display.Race(ctx, preset, seed);
            }

            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}

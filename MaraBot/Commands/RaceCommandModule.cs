using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace MaraBot.Commands
{
    using Core;
    using Messages;

    public class RaceCommandModule : BaseCommandModule
    {
        public IReadOnlyDictionary<string, Preset> Presets { private get; set; }
        public IReadOnlyDictionary<string, Option> Options { private get; set; }

        [Command("race")]
        [Cooldown(15, 900, CooldownBucketType.User)]
        [RequireGuild]
        public async Task Execute(CommandContext ctx, string presetName)
        {
            var success = (presetName == "custom") ?
                LaunchCustomRace(ctx) :
                LaunchPresetRace(ctx, presetName);

            var emoji = DiscordEmoji.FromName(ctx.Client, await success ? Display.kValidCommandEmoji : Display.kInvalidCommandEmoji);
            await ctx.Message.CreateReactionAsync(emoji);
        }

        public async Task<bool> LaunchCustomRace(CommandContext ctx)
        {
            if (ctx.Message.Attachments == null || ctx.Message.Attachments.Count == 0)
            {
                await ctx.RespondAsync("No attachment for custom race. You must supply a valid json file.");
                return false;
            }

            var attachment = ctx.Message.Attachments[0];
            var url = attachment.Url;

            var request = WebRequest.Create(url);
            var responseTask = request.GetResponseAsync();

            if (Path.GetExtension(attachment.FileName).ToLower() != ".json")
            {
                await ctx.RespondAsync("Invalid json file.");
                return false;
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
                    return false;
                }

                var seed = RandomUtils.GetRandomSeed();
                await Display.Race(ctx, preset, seed);
            }

            return true;
        }

        public async Task<bool> LaunchPresetRace(CommandContext ctx, string presetName)
        {
            if (!Presets.ContainsKey(presetName))
            {
                await ctx.RespondAsync($"{presetName} is not a a valid preset");
                return false;
            }

            var seed = RandomUtils.GetRandomSeed();
            var preset = Presets[presetName];

            await Display.Race(ctx, preset, seed);
            return true;
        }
    }
}

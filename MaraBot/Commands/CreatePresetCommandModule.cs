using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus;
using Newtonsoft.Json;

namespace MaraBot.Commands
{
    using Core;
    using Messages;

    public class CreatePresetCommandModule : BaseCommandModule
    {
        public IReadOnlyDictionary<string, Preset> Presets { private get; set; }

        [Command("newpreset")]
        [Cooldown(5, 600, CooldownBucketType.User)]
        [RequireDirectMessage]
        public async Task Execute(CommandContext ctx, [RemainingText] string optionString)
        {
            string[] optionValues = optionString == null ? new string[] { "key=value" } : optionString.Split(' ');

            var options = new Dictionary<string, string>();

            foreach(string option in optionValues)
            {
                if(!option.Contains('='))
                {
                    await ctx.RespondAsync($"'{option}' is not formatted correctly. Format must be 'key=value'.");
                    await CommandUtils.SendFailReaction(ctx);
                    return;
                }
                string[] values = option.Split('=');
                options.Add(values[0], values[1]);
            }

            Dictionary<string, object> preset = new Dictionary<string, object>();
            preset.Add("name", "Dummyname");
            preset.Add("description", "Dummydescription");
            preset.Add("version",  "0.00");
            preset.Add("author", "Dummy");
            preset.Add("options", options);

            // TODO: validation
            // if(optionString != null)
            // {
            //     string err = Preset.ValidateOptions(options);
            //     if (err != "")
            //     {
            //         await ctx.RespondAsync(err);
            //
            //         var invalidEmoji = DiscordEmoji.FromName(ctx.Client, Display.kInvalidCommandEmoji);
            //         await ctx.Message.CreateReactionAsync(invalidEmoji);
            //
            //         return;
            //     }
            // }

            string json = JsonConvert.SerializeObject(preset, Formatting.Indented);
            await ctx.RespondAsync(Formatter.BlockCode(json));
            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}

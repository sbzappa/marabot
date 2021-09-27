using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus;
using Newtonsoft.Json;

namespace MaraBot.Commands
{
    using Core;

    /// <summary>
    /// Implements the newpreset command.
    /// This command is used to create a dummy preset file with specified options.
    /// </summary>
    public class CreatePresetCommandModule : BaseCommandModule
    {
        /// <summary>
        /// Randomizer Options.
        /// </summary>
        public IReadOnlyDictionary<string, Option> Options { private get; set; }

        /// <summary>
        /// Executes the newpreset command.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="optionString">Raw options string.</param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("newpreset")]
        [Description("Create a dummy preset file, with options if given.")]
        [Cooldown(5, 600, CooldownBucketType.User)]
        [RequireDirectMessage]
        [RequireBotPermissions(Permissions.SendMessages)]
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
            preset.Add("version",  PresetValidation.kVersion);
            preset.Add("author", ctx.User.Username);
            preset.Add("options", options);

            string message = "";
            if (optionString != null)
            {
                message += $"**Options have been validated for randomizer version {PresetValidation.kVersion}. Result:**\n";

                List<string> errors = PresetValidation.ValidateOptions(options, Options);
                foreach (var e in errors)
                    message += $"> {e}\n";
            }

            string json = JsonConvert.SerializeObject(preset, Formatting.Indented);
            message += Formatter.BlockCode(json);

            await ctx.RespondAsync(message);
            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}

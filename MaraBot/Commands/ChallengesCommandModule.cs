using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace MaraBot.Commands
{
    using System;
    using Core;
    using DSharpPlus.Entities;
    using Messages;

    /// <summary>
    /// Implements the challenges command.
    /// This command is used to display all available challenge presets.
    /// </summary>
    public class ChallengesCommandModule : BaseCommandModule
    {
        /// <summary>
        /// List of challenge presets available.
        /// </summary>
        public IReadOnlyDictionary<string, Preset> Challenges { private get; set; }

        struct PresetComparer : IComparer<KeyValuePair<string, Preset>>
        {
            public int Compare(KeyValuePair<string, Preset> x, KeyValuePair<string, Preset> y) =>
                x.Key.CompareTo(y.Key);
        }

        const string k_ButtonFirstID = "button_first";
        const string k_ButtonPreviousID = "button_previous";
        const string k_ButtonNextID = "button_next";
        const string k_ButtonLastID = "button_last";

        static int s_CommandID = 0;

        /// <summary>
        /// Executes the presets command.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("challenges")]
        [Description("Get the list of challenge presets.")]
        [Cooldown(2, 900, CooldownBucketType.Channel)]
        [RequireGuild]
        [RequireBotPermissions(Permissions.SendMessages)]
        public async Task Execute(CommandContext ctx)
        {
            var challenges = Challenges.ToList();
            challenges.Sort(new PresetComparer());

            var index = 0;
            var id = $"{++s_CommandID}";

            var buttonFirstID = $"{id}/{k_ButtonFirstID}";
            var buttonPreviousID = $"{id}/{k_ButtonPreviousID}";
            var buttonNextID = $"{id}/{k_ButtonNextID}";
            var buttonLastID = $"{id}/{k_ButtonLastID}";

            await ctx.RespondAsync(Display.PresetsFlipBook(
                challenges[index].Value, index, challenges.Count,
                buttonFirstID, buttonPreviousID, buttonNextID, buttonLastID));
            await CommandUtils.SendSuccessReaction(ctx);

            ctx.Client.ComponentInteractionCreated += async (s, e) =>
            {
                var splits = e.Id.Split("/");
                if (splits[0] != id)
                    return;

                switch (splits[1])
                {
                    case k_ButtonFirstID: index = 0; break;
                    case k_ButtonPreviousID: index = Math.Max(0, index - 1); break;
                    case k_ButtonNextID: index = Math.Min(challenges.Count - 1, index + 1); break;
                    case k_ButtonLastID: index = challenges.Count - 1; break;
                }

                await e.Interaction.CreateResponseAsync(
                    InteractionResponseType.UpdateMessage,
                    new DiscordInteractionResponseBuilder()
                        .AddPresetsFlipBook(
                            challenges[index].Value, index, challenges.Count,
                            buttonFirstID, buttonPreviousID, buttonNextID, buttonLastID));
            };
        }
    }
}

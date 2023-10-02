using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace MaraBot.Commands
{
    using Core;

    /// <summary>
    /// Implements the 8ball command.
    /// This command is used to answer yes/no questions.
    /// </summary>
    public class EightBallCommandModule : BaseCommandModule
    {
        /// <summary>
        /// Bot configuration.
        /// </summary>
        public string[] Responses { private get; set; }

        /// <summary>
        /// Answer a question.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="rawArgs">Command line arguments</param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("8ball")]
        [Description("Get the answer to your question.")]
        [Cooldown(30, 600, CooldownBucketType.Channel)]
        [RequireGuild]
        [RequireBotPermissions(Permissions.SendMessages)]
        public async Task Execute(CommandContext ctx, [RemainingText][Description("Question string")]string question)
        {
            await ctx.RespondAsync(Responses[RandomUtils.GetRandomIndex(0, Responses.Length - 1)]);
        }
    }
}

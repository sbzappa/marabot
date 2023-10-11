using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace MaraBot.Commands
{
    using Core;
    using Messages;

    /// <summary>
    /// Implements the cheat command.
    /// This command is used to remind users that cheating is never the answer :)
    /// </summary>
    public class CheatCommandModule : BaseCommandModule
    {
        const string k_CheatMessage =
            "You cheated not only the game, but yourself.\n" +
            "You didn't grow. You didn't improve. You took a shortcut and gained nothing.\n" +
            "You experienced a hollow victory. Nothing was risked and nothing was gained.\n" +
            "It's sad that you don't know the difference.\n";

        /// <summary>
        /// Executes the weekly command.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("cheat")]
        [Description("Display a reminder that cheating is never the answer.")]
        [Cooldown(15, 600, CooldownBucketType.Channel)]
        [RequireGuild]
        [RequireBotPermissions(Permissions.SendMessages)]
        public async Task Execute(CommandContext ctx)
        {
            await ctx.RespondAsync(k_CheatMessage);
        }
    }
}

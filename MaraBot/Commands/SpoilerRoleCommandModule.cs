using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MaraBot.IO;

namespace MaraBot.Commands
{
    using Core;

    /// <summary>
    /// Implements the spoiler command.
    /// This command is used to manually grant/revoke the spoiler role to certain users.
    /// </summary>
    public class SpoilerRoleCommandModule : BaseCommandModule
    {
        /// <summary>
        /// Bot configuration.
        /// </summary>
        public IConfig Config { private get; set; }

        /// <summary>
        /// Executes the weekly command.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("spoiler")]
        [Description("Grant or revoke the spoiler roles.")]
        [Cooldown(30, 900, CooldownBucketType.Channel)]
        [RequireGuild]
        [RequireBotPermissions(
            Permissions.SendMessages |
            Permissions.ManageRoles)]
        public async Task Execute(CommandContext ctx, string optionString, string memberString)
        {
            // Safety measure to avoid potential misuses of this command.
            if (!await CommandUtils.MemberHasPermittedRole(ctx, Config.OrganizerRoles))
            {
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            if (optionString == "done")
            {
                await CommandUtils.GrantRolesAsync(ctx, new[] {memberString}, new[] {Config.WeeklyCompletedRole});
            }
            else if (optionString == "forfeit")
            {
                await CommandUtils.GrantRolesAsync(ctx, new[] {memberString}, new[] {Config.WeeklyForfeitedRole});
            }
            else if (optionString == "revoke")
            {
                await CommandUtils.RevokeRolesAsync(ctx,
                    new[] {memberString},
                    new []
                    {
                        Config.WeeklyCompletedRole,
                        Config.WeeklyForfeitedRole
                    });
            }
            else
            {
                await ctx.RespondAsync($"Unrecognized option '{optionString}'");
                await CommandUtils.SendFailReaction(ctx);

                return;
            }

            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}

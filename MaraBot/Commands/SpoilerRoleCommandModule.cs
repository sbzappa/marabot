using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace MaraBot.Commands
{
    using Core;

    /// <summary>
    /// Implements the spoiler command.
    /// This command is used to manually grant/revoke the spoiler role to certain users.
    /// </summary>
    public class SpoilerRoleCommandModule : BaseCommandModule
    {
        private const string kArgsDescription = "\n```" +
                                                "  --done member      Grants WeeklyCompletedRole.\n" +
                                                "  --completed member Grants WeeklyCompletedRole.\n" +
                                                "  --forfeit member   Grants WeeklyForfeitedRole.\n" +
                                                "  --revoke member    Revokes WeelyForfeitedRole & WeeklyCompletedRole.\n" +
                                                "```";

        /// <summary>
        /// Bot configuration.
        /// </summary>
        public Config Config { private get; set; }

        /// <summary>
        /// Grant/Revoke spoiler roles manually.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="rawArgs">Command line arguments</param>
        /// <returns>Returns an asynchronous task.</returns>
        [Command("spoiler")]
        [Description("Grant or revoke the spoiler roles.")]
        [Cooldown(15, 900, CooldownBucketType.Channel)]
        [RequireGuild]
        [RequireBotPermissions(
            Permissions.SendMessages |
            Permissions.ManageRoles)]
        public async Task Execute(CommandContext ctx, [RemainingText][Description(kArgsDescription)]string rawArgs)
        {
            // Safety measure to avoid potential misuses of this command.
            if (!await CommandUtils.MemberHasPermittedRole(ctx, Config.OrganizerRoles))
            {
                await CommandUtils.SendFailReaction(ctx);
                return;
            }

            var args = rawArgs.Split(' ');

            var membersToGrantDoneRole = new List<string>();
            var membersToGrantForfeitRole = new List<string>();
            var membersToRevokeRoles = new List<string>();

            var index = 0;
            while (index + 1 < args.Length)
            {
                switch (args[index])
                {
                   case "--done":
                   case "--completed":
                       membersToGrantDoneRole.Add(args[++index]);
                       break;
                   case "--forfeit":
                       membersToGrantForfeitRole.Add(args[++index]);
                       break;
                   case "--revoke":
                       membersToRevokeRoles.Add(args[++index]);
                       break;
                   default:
                       await ctx.RespondAsync($"Unrecognized option '{args[index]}'");
                       await CommandUtils.SendFailReaction(ctx);
                       return;
                }

                ++index;
            }

            if (index < args.Length)
            {
                await ctx.RespondAsync($"Unrecognized option '{args[index]}'");
                await CommandUtils.SendFailReaction(ctx);
            }

            if (membersToGrantDoneRole.Count > 0)
                await CommandUtils.GrantRolesAsync(ctx, membersToGrantDoneRole, new[] {Config.WeeklyCompletedRole});

            if (membersToGrantForfeitRole.Count > 0)
                await CommandUtils.GrantRolesAsync(ctx, membersToGrantDoneRole, new[] {Config.WeeklyForfeitedRole});

            if (membersToRevokeRoles.Count > 0)
            {
                await CommandUtils.RevokeRolesAsync(ctx,
                    membersToRevokeRoles,
                    new[]
                    {
                        Config.WeeklyCompletedRole,
                        Config.WeeklyForfeitedRole
                    });
            }

            await CommandUtils.SendSuccessReaction(ctx);
        }
    }
}

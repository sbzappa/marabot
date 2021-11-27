using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using Microsoft.Extensions.Logging;

namespace MaraBot.Core
{
    /// <summary>
    /// Events triggered by commands.
    /// </summary>
    public static class CommandEvents
    {
        /// <summary>
        /// Event function called whenever a command is executed.
        /// </summary>
        /// <param name="cne">Commands specifications.</param>
        /// <param name="e">Commands arguments.</param>
        /// <returns>Returns an asynchronous task.</returns>
        public static Task OnCommandExecuted(CommandsNextExtension cne, CommandExecutionEventArgs e)
        {
            string msg = $"Executed command [{e.Context.Prefix}{e.Command.Name}] in " + (e.Context.Guild == null
                    ? "direct message."
                    : $"channel [#{e.Context.Channel.Name}] of guild [{e.Context.Guild.Name}].");
            cne.Client.Logger.LogInformation(msg);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Event function called whenever there's an error while executing a command.
        /// </summary>
        /// <param name="cne">Commands specifications.</param>
        /// <param name="e">Commands arguments.</param>
        /// <returns>Returns an asynchronous task.</returns>
        public static async Task OnCommandErrored(CommandsNextExtension cne, CommandErrorEventArgs e)
        {
            var formattedCommand = Formatter.InlineCode($"{e.Context.Prefix}{e.Command.Name}");
            var failedChecks = ((ChecksFailedException)e.Exception).FailedChecks;
            foreach (var failedCheck in failedChecks)
            {
                switch (failedCheck)
                {
                    case CooldownAttribute cooldownAttribute:
                    {
                        var cooldown = cooldownAttribute.Reset.ToString(@"mm");
                        var remaining = cooldownAttribute.GetRemainingCooldown(e.Context).ToString(@"mm\:ss");

                        await e.Context.RespondAsync(
                            $"Command {formattedCommand} is on cooldown for {cooldown} minutes!\n" +
                            $"{remaining} is remaining!"
                            );
                        break;
                    }
                    case RequireBotPermissionsAttribute _:
                    {
                        await e.Context.RespondAsync($"Insufficient bot permissions to run command {formattedCommand}.");
                        break;
                    }
                    case RequireUserPermissionsAttribute _:
                    {
                        await e.Context.RespondAsync($"{e.Context.User.Mention} has insufficient permissions to run command {formattedCommand}.");
                        break;
                    }
                    case RequireRolesAttribute _:
                    {
                        await e.Context.RespondAsync($"{e.Context.User.Mention} does not have a role that can run command {formattedCommand}.");
                        break;
                    }
                    case RequireGuildAttribute _:
                    {
                        await e.Context.RespondAsync($"Command {formattedCommand} can only be run in a guild.");
                        break;
                    }
                    case RequireDirectMessageAttribute _:
                    {
                        await e.Context.RespondAsync($"Command {formattedCommand} can only be run in direct message to the bot.");
                        break;
                    }
                    default:
                    {
                        await e.Context.RespondAsync(
                            $"Unrecognized failed check {failedCheck.GetType().Name}.\n" +
                            CommandUtils.kFriendlyMessage);
                        break;
                    }
                }
            }

            string msg = $"Error while executing command [{e.Context.Prefix}{e.Command.Name}] in " + (e.Context.Guild == null
                    ? "direct message:"
                    : $"channel [#{e.Context.Channel.Name}] of guild [{e.Context.Guild.Name}]:");
            cne.Client.Logger.LogError(e.Exception, msg);
        }
    }
}

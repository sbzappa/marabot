using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
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
        public static Task OnCommandErrored(CommandsNextExtension cne, CommandErrorEventArgs e)
        {
            string msg = $"Error while executing command [{e.Context.Prefix}{e.Command.Name}] in " + (e.Context.Guild == null
                    ? "direct message:"
                    : $"channel [#{e.Context.Channel.Name}] of guild [{e.Context.Guild.Name}]:");
            cne.Client.Logger.LogError(e.Exception, msg);
            return Task.CompletedTask;
        }
    }
}

using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;

namespace MaraBot.Core
{
    public static class CommandEvents
    {
        public static Task OnCommandExecuted(CommandsNextExtension cne, CommandExecutionEventArgs e)
        {
            string msg = $"Executed command [{e.Context.Prefix}{e.Command.Name}] in " + (e.Context.Guild == null
                    ? "direct message."
                    : $"channel [#{e.Context.Channel.Name}] of guild [{e.Context.Guild.Name}].");
            cne.Client.Logger.LogInformation(msg);
            return Task.CompletedTask;
        }

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

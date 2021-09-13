using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace MaraBot.Core
{
    using Messages;

    public static class CommandUtils
    {
        /// <summary>
        /// Check if the bot is in a direct message.
        /// Runtime-equivalent of RequireDirectMessageAttribute.
        /// </summary>
        public static bool IsDirectMessage(CommandContext ctx)
        {
            return ctx.Channel is DiscordDmChannel;
        }

        /// <summary>
        /// Check if the bot has certain permissions.
        /// Runtime-equivalent of RequireBotPermissionsAttribute.
        /// </summary>
        public static async Task<bool> HasBotPermissions(CommandContext ctx, Permissions permissions, bool ignoreDms = true)
        {
            if(ctx.Guild == null)
                return ignoreDms;

            var bot = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id).ConfigureAwait(false);
            if (bot == null)
                return false;

            if (bot.Id == ctx.Guild.OwnerId)
                return true;

            var botPerms = ctx.Channel.PermissionsFor(bot);

            if ((botPerms & Permissions.Administrator) != 0)
                return true;

            return (botPerms & permissions) == permissions;
        }

        /// <summary>
        /// Send a success (or fail) reaction if the bot has the react permission.
        /// The bot won't send reactions in DMs, because there it's way easier
        /// to see if a command completes.
        /// </summary>
        public static async Task SendSuccessReaction(CommandContext ctx, bool success = true)
        {
            if(IsDirectMessage(ctx))
                return;

            if(!(await HasBotPermissions(ctx, Permissions.AddReactions)))
                return;

            var emoji = DiscordEmoji.FromName(ctx.Client, success ? Display.kValidCommandEmoji : Display.kInvalidCommandEmoji);
            await ctx.Message.CreateReactionAsync(emoji);
        }

        /// <summary>
        /// Calls SendSuccessReaction(ctx, false).
        /// </summary>
        public static Task SendFailReaction(CommandContext ctx)
        {
            return SendSuccessReaction(ctx, false);
        }

        /// <summary>
        /// Make a mention without fear of making a pinging mention.
        /// </summary>
        public static async Task<string> MentionRoleWithoutPing(CommandContext ctx, DiscordRole role)
        {
            return (await MentionRoleWithoutPing(ctx, new [] { role }))[ 0 ];
        }

        /// <summary>
        /// Make mentions without fear of making pinging mentions.
        /// </summary>
        public static async Task<string[]> MentionRoleWithoutPing(CommandContext ctx, DiscordRole[] roles)
        {
            var hasPerms = await HasBotPermissions(ctx, Permissions.MentionEveryone);
            return roles.Select(r => r.IsMentionable || hasPerms ? $"@({r.Name})" : r.Mention).ToArray();
        }

        /// <summary>
        /// Grants a role to a user.
        /// </summary>
        public static async Task GrantRoleAsync(CommandContext ctx, string roleString)
        {
            // Grant user their new role.
            var newRole = ctx.Guild.Roles
                .FirstOrDefault(kvp => roleString.Equals(kvp.Value.Name));

            if (newRole.Value == null)
            {
                await ctx.RespondAsync(
                    "No role set for access to spoiler channel.\n" +
                    "This shouldn't happen! Please contact your friendly neighbourhood developers!");

                throw new InvalidOperationException($"role {newRole} has not been found in guild {ctx.Guild.Name}.");
            }

            await ctx.Member.GrantRoleAsync(newRole.Value);
        }

        /// <summary>
        /// Sends a message to a specific channel.
        /// </summary>
        public static async Task SendToChannelAsync(CommandContext ctx, string channelName, DiscordEmbedBuilder embed)
        {
            var channel = ctx.Guild.Channels
                .FirstOrDefault(kvp => channelName.Equals(kvp.Value.Name)).Value;

            if (channel == null)
            {
                var errorMessage = $"Channel {channelName} has not been found in guild {ctx.Guild.Name}.";

                await ctx.RespondAsync(
                    errorMessage +
                    "This shouldn't happen! Please contact your friendly neighbourhood developers!");

                throw new InvalidOperationException(errorMessage);
            }

            await channel.SendMessageAsync(embed);
        }

        /// <summary>
        /// Verifies if current channel matches specified channel.
        /// </summary>
        public static async Task<bool> VerifyChannel(CommandContext ctx, string channelName)
        {
            var channel = ctx.Guild.Channels
                .FirstOrDefault(kvp => channelName.Equals(kvp.Value.Name)).Value;

            if (channel == null)
            {
                var errorMessage = $"Channel {channelName} has not been found in guild {ctx.Guild.Name}.";

                await ctx.RespondAsync(
                    errorMessage +
                    "This shouldn't happen! Please contact your friendly neighbourhood developers!");

                throw new InvalidOperationException(errorMessage);
            }

            return ctx.Channel.Equals(channel);
        }
    }
}

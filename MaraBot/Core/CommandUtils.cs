using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus.CommandsNext;
using DSharpPlus;
using DSharpPlus.Entities;

namespace MaraBot.Core
{
    using Messages;

    public static class CommandUtils
    {
        const string kFriendlyMessage = "This shouldn't happen! Please contact your friendly neighbourhood developers!";

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
            if (ctx.Guild == null)
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
            if (IsDirectMessage(ctx))
                return;

            if (!(await HasBotPermissions(ctx, Permissions.AddReactions)))
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
            return (await MentionRoleWithoutPing(ctx, new[] {role}))[0];
        }

        /// <summary>
        /// Make mentions without fear of making pinging mentions.
        /// </summary>
        public static async Task<string[]> MentionRoleWithoutPing(CommandContext ctx, IEnumerable<DiscordRole> roles)
        {
            var hasPerms = await HasBotPermissions(ctx, Permissions.MentionEveryone);
            return roles.Select(r => r.IsMentionable || hasPerms ? $"@({r.Name})" : r.Mention).ToArray();
        }

        public static async Task GrantRoleAsync(CommandContext ctx, string roleString)
        {
            var role = ctx.Guild.Roles.Values
                .FirstOrDefault(role => roleString.Equals(role.Name));

            if (role == null)
            {
                var errorMessage = $"Role {roleString} has not been found in guild {ctx.Guild.Name}.";
                await ctx.RespondAsync(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            await GrantRolesAsync(ctx, new [] {ctx.Member}, new [] {role});
        }

        public static async Task GrantRolesAsync(CommandContext ctx, IEnumerable<string> memberStrings, IEnumerable<string> roleStrings)
        {
            var roles = ctx.Guild.Roles.Values
                .Where(role => roleStrings.Contains(role.Name));

            if (!roles.Any())
            {
                var errorMessage = $"No roles matching specified search have been found in guild {ctx.Guild.Name}.";
                await ctx.RespondAsync(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            var allMembers = await ctx.Guild.GetAllMembersAsync();

            var members = allMembers
                .Where(member => memberStrings.Contains(member.Username));

            if (!members.Any())
            {
                var errorMessage = $"No members matching specified search have been found in guild {ctx.Guild.Name}.";
                await ctx.RespondAsync(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            await GrantRolesAsync(ctx, members, roles);
        }

        /// <summary>
        /// Grants a role to a user.
        /// </summary>
        public static async Task GrantRolesAsync(CommandContext ctx, IEnumerable<DiscordMember> members, IEnumerable<DiscordRole> roles)
        {
            var grantTasks = new List<Task>();
            foreach (var role in roles)
            {
                var tasks = members
                    .Select(member => member.GrantRoleAsync(role))
                    .ToList();

                grantTasks.AddRange(tasks);
            }

            while (grantTasks.Any())
            {
                var finishedTask = await Task.WhenAny(grantTasks);
                grantTasks.Remove(finishedTask);
                await finishedTask;
            }
        }

        public static async Task RevokeRolesAsync(CommandContext ctx, IEnumerable<string> roleStrings)
        {
            var roles = ctx.Guild.Roles
                .Where(role => roleStrings.Contains(role.Value.Name))
                .Select(role => role.Value);

            if (!roles.Any())
            {
                var errorMessage = $"No roles matching specified search have been found in guild {ctx.Guild.Name}.";
                await ctx.RespondAsync(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            var allMembers = await ctx.Guild.GetAllMembersAsync();

            var members = allMembers
                .Where(member => member.Roles.Any(role => roles.Contains(role)));

            if (!members.Any())
            {
                var errorMessage = $"No members currently have specified roles in guild {ctx.Guild.Name}.";
                await ctx.RespondAsync(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            await RevokeRolesAsync(ctx, members, roles);
        }

        public static async Task RevokeRolesAsync(CommandContext ctx, IEnumerable<string> memberStrings, IEnumerable<string> roleStrings)
        {
            var roles = ctx.Guild.Roles.Values
                .Where(role => roleStrings.Contains(role.Name));

            if (!roles.Any())
            {
                var errorMessage = $"No roles matching specified search have been found in guild {ctx.Guild.Name}.";
                await ctx.RespondAsync(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            var allMembers = await ctx.Guild.GetAllMembersAsync();

            var members = allMembers
                .Where(member => memberStrings.Contains(member.Username));

            if (!members.Any())
            {
                var errorMessage = $"No members matching specified search have been found in guild {ctx.Guild.Name}.";
                await ctx.RespondAsync(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            await RevokeRolesAsync(ctx, members, roles);
        }

        /// <summary>
        /// Revoke all spoiler roles.
        /// </summary>
        public static async Task RevokeRolesAsync(CommandContext ctx, IEnumerable<DiscordMember> members, IEnumerable<DiscordRole> roles)
        {
            var revokeTasks = new List<Task>();
            foreach (var role in roles)
            {
                var tasks = members
                    .Select(member => member.RevokeRoleAsync(role))
                    .ToList();

                revokeTasks.AddRange(tasks);
            }

            while (revokeTasks.Any())
            {
                var finishedTask = await Task.WhenAny(revokeTasks);
                revokeTasks.Remove(finishedTask);
                await finishedTask;
            }
        }

        public static Task SendToChannelAsync(CommandContext ctx, string channelName, DiscordEmbed embed) =>
            SendToChannelAsync(ctx, channelName, new DiscordMessageBuilder().WithEmbed(embed));

        public static Task SendToChannelAsync(CommandContext ctx, string channelName, string message) =>
            SendToChannelAsync(ctx, channelName, new DiscordMessageBuilder().WithContent(message));

        /// <summary>
        /// Sends a message to a specific channel.
        /// </summary>
        public static async Task SendToChannelAsync(CommandContext ctx, string channelName, DiscordMessageBuilder messageBuilder)
        {
            var channel = ctx.Guild.Channels
                .FirstOrDefault(kvp => channelName.Equals(kvp.Value.Name)).Value;

            if (channel == null)
            {
                var errorMessage = $"Channel {channelName} has not been found in guild {ctx.Guild.Name}.";

                await ctx.RespondAsync(
                    errorMessage + "\n" +
                    kFriendlyMessage);

                throw new InvalidOperationException(errorMessage);
            }

            await channel.SendMessageAsync(messageBuilder);
        }

        /// <summary>
        /// Verifies if current channel matches specified channel.
        /// </summary>
        public static async Task<bool> ChannelExistsInGuild(CommandContext ctx, string channelName)
        {
            var channel = ctx.Guild.Channels
                .FirstOrDefault(kvp => channelName.Equals(kvp.Value.Name)).Value;

            if (channel == null)
            {
                var errorMessage = $"Channel {channelName} has not been found in guild {ctx.Guild.Name}.";

                await ctx.RespondAsync(
                    errorMessage + "\n" +
                    kFriendlyMessage);

                throw new InvalidOperationException(errorMessage);
            }

            return ctx.Channel.Equals(channel);
        }

        /// <summary>
        /// Verifies whether member has a role that is among permitted roles for this command.
        /// </summary>
        public static async Task<bool> MemberHasPermittedRole(CommandContext ctx, IEnumerable<string> permittedRoles)
        {
            if (permittedRoles == null)
                return true;

            // Safety measure to avoid potential misuses of this command. May be revisited in the future.
            if (!ctx.Member.Roles.Any(role => permittedRoles.Contains(role.Name)))
            {
                var guildRoles = ctx.Guild.Roles
                    .Where(role => permittedRoles.Contains(role.Value.Name));

                await ctx.RespondAsync(
                    "Insufficient privileges to execute this command.\n" +
                    "This command is only available to the following roles:\n" +
                    String.Join(
                        ", ",
                        await MentionRoleWithoutPing(ctx, guildRoles.Select(r => r.Value).ToArray())
                    )
                );

                return false;
            }

            return true;
        }

        /// <summary>
        /// Converts a positive integer to an ordinal number string.
        /// </summary>
        public static string IntegerToOrdinal(int n)
        {
            if (n < 1)
                throw new ArgumentOutOfRangeException("Integer must be positive");

            switch (n % 100)
            {
                case 11:
                case 12:
                case 13:
                    return $"{n}th";
            }

            switch (n % 10)
            {
                case 1 : return $"{n}st";
                case 2 : return $"{n}nd";
                case 3 : return $"{n}rd";
                default: return $"{n}th";
            }
        }
    }
}

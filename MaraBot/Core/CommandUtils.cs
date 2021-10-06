using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using DSharpPlus.CommandsNext;
using DSharpPlus;
using DSharpPlus.Entities;

namespace MaraBot.Core
{
    using Messages;
    using IO;

    public static class CommandUtils
    {
        const string kFriendlyMessage = "This shouldn't happen! Please contact your friendly neighbourhood developers!";

        private enum AttachmentFileType
        {
            Invalid,
            JsonPreset,
            LogFile
        };

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

        private static async Task<IEnumerable<DiscordRole>> ConvertRoleNamesToRoles(CommandContext ctx, IEnumerable<string> roleStrings)
        {
            var roles = ctx.Guild.Roles.Values
                .Where(role => roleStrings.Contains(role.Name));

            if (!roles.Any())
            {
                var errorMessage = $"No roles matching specified search have been found in guild {ctx.Guild.Name}.";
                await ctx.RespondAsync(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            return roles;
        }

        private static async Task<IEnumerable<DiscordMember>> ConvertMemberNamesToMembers(CommandContext ctx, IEnumerable<string> memberStrings)
        {
            var allMembers = await ctx.Guild.GetAllMembersAsync();

            var members = allMembers
                .Where(member => memberStrings.Contains(member.Username));

            if (!members.Any())
            {
                var errorMessage = $"No members matching specified search have been found in guild {ctx.Guild.Name}.";
                await ctx.RespondAsync(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            return members;
        }

        /// <summary>
        /// Grants roles to current member.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="roleStrings">Array of role names.</param>
        /// <returns>Returns an asynchronous task.</returns>
        /// <exception cref="InvalidOperationException">There are no matching roles.</exception>
        public static async Task GrantRolesToSelfAsync(CommandContext ctx, IEnumerable<string> roleStrings)
        {
            var rolesTask = ConvertRoleNamesToRoles(ctx, roleStrings);
            await GrantRolesAsync(ctx, new [] {ctx.Member}, await rolesTask);
        }

        /// <summary>
        /// Grants roles to specified members.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="memberStrings">Array of member names.</param>
        /// <param name="roleStrings">Array of role names.</param>
        /// <returns>Returns an asynchronous task.</returns>
        /// <exception cref="InvalidOperationException">There are no matching roles or no matching members.</exception>
        public static async Task GrantRolesAsync(CommandContext ctx, IEnumerable<string> memberStrings, IEnumerable<string> roleStrings)
        {
            var rolesTask = ConvertRoleNamesToRoles(ctx, roleStrings);
            var membersTask = ConvertMemberNamesToMembers(ctx, memberStrings);
            await GrantRolesAsync(ctx, await membersTask, await rolesTask);
        }

        /// <summary>
        /// Grants roles to specified members.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="members">Array of members.</param>
        /// <param name="roles">Array of roles.</param>
        /// <returns>Returns an asynchronous task.</returns>
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

        /// <summary>
        /// Revokes specified roles from all members that have them.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="roleStrings">Array of role names.</param>
        /// <returns>Returns an asynchronous task.</returns>
        /// <exception cref="InvalidOperationException">There are no matching roles.</exception>
        public static async Task RevokeAllRolesAsync(CommandContext ctx, IEnumerable<string> roleStrings)
        {
            var allMembersTask = ctx.Guild.GetAllMembersAsync();

            var roles= await ConvertRoleNamesToRoles(ctx, roleStrings);
            var allMembers = await allMembersTask;

            var members = allMembers
                .Where(member => member.Roles.Any(role => roles.Contains(role)));

            await RevokeRolesAsync(ctx, members, roles);
        }

        /// <summary>
        /// Revokes roles from specified members.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="memberStrings">Array of member names.</param>
        /// <param name="roleStrings">Array of role names.</param>
        /// <returns>Returns an asynchronous task.</returns>
        /// <exception cref="InvalidOperationException">There are no matching roles or no matching members.</exception>
        public static async Task RevokeRolesAsync(CommandContext ctx, IEnumerable<string> memberStrings, IEnumerable<string> roleStrings)
        {
            var roles = ConvertRoleNamesToRoles(ctx, roleStrings);
            var members = ConvertMemberNamesToMembers(ctx, memberStrings);
            await RevokeRolesAsync(ctx, await members, await roles);
        }

        /// <summary>
        /// Revokes roles from specified members.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="members">Array of members.</param>
        /// <param name="roles">Array of roles.</param>
        /// <returns>Returns an asynchronous task.</returns>
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

        private static AttachmentFileType GetAttachmentFileType(CommandContext ctx, out string errorMessage)
        {
            if (ctx.Message.Attachments == null || ctx.Message.Attachments.Count != 1)
            {
                errorMessage = "No attachment provided. You must supply upload a file when using this command.";
                return AttachmentFileType.Invalid;
            }

            var attachment = ctx.Message.Attachments[0];

            // Json Preset
            if (Path.GetExtension(attachment.FileName).ToLower() == ".json")
            {
                errorMessage = String.Empty;
                return AttachmentFileType.JsonPreset;
            }
            // Log Txt File
            else if (Path.GetExtension(attachment.FileName).ToLower() == ".txt")
            {
                var body = attachment.FileName.Remove(attachment.FileName.Length - 4);

                // Should not be the spoiler log.
                if (body.EndsWith("_SPOILER"))
                {
                    errorMessage = "Spoiler log file has been provided. Please, provide non-spoiler log instead.";
                    return AttachmentFileType.Invalid;
                }

                var regex = new Regex("log_[A-F0-9]{16}");
                var match = regex.Match(body);

                // Should match the usual log file format.
                if (match.Length != body.Length)
                {
                    errorMessage = "Spoiler log file name is not standard. Expecting a log_ prefix followed by a 16 digits hexadecimal seed.";
                    return AttachmentFileType.Invalid;
                }

                errorMessage = String.Empty;
                return AttachmentFileType.LogFile;
            }

            errorMessage = "Unrecognized file extension";
            return AttachmentFileType.Invalid;
        }

        private static async Task<Preset> LoadPresetAttachmentAsync(CommandContext ctx, IReadOnlyDictionary<string, Option> options)
        {
            var attachment = ctx.Message.Attachments[0];
            var url = attachment.Url;

            var request = WebRequest.Create(url);
            var response = await request.GetResponseAsync();
            var dataStream = response.GetResponseStream();

            using (StreamReader r = new StreamReader(dataStream))
            {
                var jsonContent = await r.ReadToEndAsync();
                var preset = PresetIO.LoadPreset(jsonContent, options);
                if (preset.Equals(default))
                    throw new InvalidOperationException("Could not parse custom json preset. Please supply a valid json file.");

                return preset;
            }
        }

        private static async Task<(Preset Preset, string Seed)> LoadLogAttachmentAsync(CommandContext ctx, IReadOnlyDictionary<string, Option> options)
        {
            var attachment = ctx.Message.Attachments[0];
            var url = attachment.Url;

            var request = WebRequest.Create(url);
            var response = await request.GetResponseAsync();
            var dataStream = response.GetResponseStream();

            using (StreamReader r = new StreamReader(dataStream))
            {
                await r.ReadLineAsync();
                var seedLineTask = r.ReadLineAsync();
                var optionsLineTask = r.ReadLineAsync();
                await r.ReadLineAsync();
                await r.ReadLineAsync();
                var versionLineTask = r.ReadLineAsync();

                var seedRegex = new Regex("Seed = (?<seed>[A-F0-9]{16})");
                var optionsRegex = new Regex("Options = (?<options>.*)$");
                var versionRegex = new Regex("version (?<version>[0-9].[0-9][0-9])");

                var seedMatch = seedRegex.Match(await seedLineTask);
                var optionsMatch = optionsRegex.Match(await optionsLineTask);
                var versionMatch = versionRegex.Match(await versionLineTask);

                var seed = seedMatch.Success ? seedMatch.Groups["seed"].Value : string.Empty;
                var optionsString = optionsMatch.Success ? optionsMatch.Groups["options"].Value : string.Empty;
                var version = versionMatch.Success ? versionMatch.Groups["version"].Value : String.Empty;

                Preset preset = CreatePresetFromOptionsString( ctx.User.Username,  "DummyName", "DummyDescription", version, optionsString);
                preset.MakeDisplayable(options);
                return (preset, seed);
            }
        }

        /// <summary>
        /// Loads a race attachment
        /// </summary>
        /// <param name="ctx">Command context.</param>
        /// <param name="options">Preset options.</param>
        /// <returns>Tuple of preset and seed string.</returns>
        /// <exception cref="InvalidOperationException">Thrown if there was an error while parsing provided attachment.</exception>
        public static async Task<(Preset Preset, string Seed)> LoadRaceAttachment(CommandContext ctx, IReadOnlyDictionary<string, Option> options)
        {
            var attachmentType = GetAttachmentFileType(ctx, out var errorMessage);

            switch (attachmentType)
            {
                case AttachmentFileType.JsonPreset:
                    var preset = await LoadPresetAttachmentAsync(ctx, options);
                    return (preset, String.Empty);
                case AttachmentFileType.LogFile:
                    return await LoadLogAttachmentAsync(ctx, options);
                default:
                    throw new InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// Creates a preset from raw options string.
        /// </summary>
        /// <param name="author">Author of preset.</param>
        /// <param name="description">Description of preset.</param>
        /// <param name="version">Version of the randomizer.</param>
        /// <param name="optionsString">Raw options string.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown if there was an error while parsing options string.</exception>
        public static Preset CreatePresetFromOptionsString(string author, string name, string description, string version, string optionsString)
        {
            string[] optionsValues = String.IsNullOrEmpty(optionsString) ? new [] { "mode=rando" } : optionsString.Split(' ');

            var options = new Dictionary<string, string>();

            foreach(string option in optionsValues)
            {
                if(!option.Contains('='))
                    throw new InvalidOperationException($"'{option}' is not formatted correctly. Format must be 'key=value'.");

                string[] values = option.Split('=');
                options.Add(values[0], values[1]);
            }

            return new Preset
            {
                Author = author,
                Name = name,
                Description = description,
                Version = version,
                Options = options
            };
        }

        /// <summary>
        /// Validates the preset options.
        /// </summary>
        public static string ValidatePresetOptions(CommandContext ctx, in Preset preset, IReadOnlyDictionary<string, Option> options)
        {
            var validationMessage = "";
            if (preset.Version == PresetValidation.kVersion)
                validationMessage += "**Preset has been validated successfully. Result:**\n";
            else
                validationMessage += $"**Preset randomizer version {preset.Version} doesn't match validator randomizer version {PresetValidation.kVersion}. Validation might be wrong in certain places. Validation Result:**\n";

            List<string> errors = PresetValidation.ValidateOptions(preset.Options, options);
            foreach (var e in errors)
                validationMessage += $"> {e}\n";

            return validationMessage;
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

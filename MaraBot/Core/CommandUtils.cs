using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Net.Http;
using DSharpPlus.CommandsNext;
using DSharpPlus;
using DSharpPlus.Entities;

namespace MaraBot.Core
{
    using Messages;
    using IO;
    using System.Diagnostics;
    using System.Threading;

    public static class CommandUtils
    {
        public const string kFriendlyMessage = "This shouldn't happen! Please contact your friendly neighbourhood developers!";

        public const string kCustomRaceArgsDescription = "\n```" +
                                                         "  --author string      Sets the author of the preset.\n" +
                                                         "  --name string        Sets the name of the preset.\n" +
                                                         "  --description string Sets the description of the preset.\n" +
                                                         "```";

        static readonly HttpClient s_Client = new HttpClient();

        private enum AttachmentFileType
        {
            None,
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
        /// Send a reaction if the bot has the react permission.
        /// The bot won't send reactions in DMs, because there it's way easier
        /// to see if a command completes.
        /// </summary>
        public static async Task SendReaction(CommandContext ctx, string emojiName)
        {
            if (IsDirectMessage(ctx))
                return;

            if (!(await HasBotPermissions(ctx, Permissions.AddReactions)))
                return;

            var emoji = DiscordEmoji.FromName(ctx.Client, emojiName);
            await ctx.Message.CreateReactionAsync(emoji);
        }

        /// <summary>
        /// Send a success reaction.
        /// </summary>
        public static Task SendSuccessReaction(CommandContext ctx)
        {
            return SendReaction(ctx, Display.kValidCommandEmoji);
        }

        /// <summary>
        /// Send a fail reaction.
        /// </summary>
        public static Task SendFailReaction(CommandContext ctx)
        {
            return SendReaction(ctx, Display.kInvalidCommandEmoji);
        }

        /// <summary>
        /// Send a race validated reaction.
        /// </summary>
        public static Task SendRaceValidatedReaction(CommandContext ctx)
        {
            return SendReaction(ctx, Display.kRaceValidatedEmoji);
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


        private static Task<IEnumerable<DiscordRole>> ConvertRoleNamesToRoles(DiscordGuild guild, IEnumerable<string> roleStrings)
        {
            var roles = guild.Roles.Values
                .Where(role => roleStrings.Contains(role.Name));

            if (!roles.Any())
            {
                var errorMessage = $"No roles matching specified search have been found in guild {guild.Name}.";
                throw new InvalidOperationException(errorMessage);
            }

            return Task.FromResult(roles);
        }

        private static async Task<IEnumerable<DiscordMember>> ConvertMemberNamesToMembers(DiscordGuild guild, IEnumerable<string> memberStrings)
        {
            var allMembers = await guild.GetAllMembersAsync();

            var members = allMembers
                .Where(member => memberStrings.Contains(member.Username));

            if (!members.Any())
            {
                var errorMessage = $"No members matching specified search have been found in guild {guild.Name}.";
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
            var rolesTask = ConvertRoleNamesToRoles(ctx.Guild, roleStrings);
            await GrantRolesAsync(new [] {ctx.Member}, await rolesTask);
        }

        /// <summary>
        /// Grants roles to specified members.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="roleStrings">Array of role names.</param>
        /// <returns>Returns an asynchronous task.</returns>
        /// <exception cref="InvalidOperationException">There are no matching roles.</exception>
        public static async Task GrantRolesAsync(DiscordGuild guild, IEnumerable<string> memberStrings, IEnumerable<string> roleStrings)
        {
            var rolesTask = ConvertRoleNamesToRoles(guild, roleStrings);
            var membersTask = ConvertMemberNamesToMembers(guild, memberStrings);
            await GrantRolesAsync(await membersTask, await rolesTask);
        }

        /// <summary>
        /// Grants roles to specified members.
        /// </summary>
        /// <param name="members">Array of members.</param>
        /// <param name="roles">Array of roles.</param>
        /// <returns>Returns an asynchronous task.</returns>
        static async Task GrantRolesAsync(IEnumerable<DiscordMember> members, IEnumerable<DiscordRole> roles)
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
        /// <param name="guild">Discord guild.</param>
        /// <param name="roleStrings">Array of role names.</param>
        /// <returns>Returns an asynchronous task.</returns>
        /// <exception cref="InvalidOperationException">There are no matching roles.</exception>
        public static async Task RevokeAllRolesAsync(DiscordGuild guild, IEnumerable<string> roleStrings)
        {
            var allMembersTask = guild.GetAllMembersAsync();

            var roles= await ConvertRoleNamesToRoles(guild, roleStrings);
            var allMembers = await allMembersTask;

            var members = allMembers
                .Where(member => member.Roles.Any(role => roles.Contains(role)))
                .Where(member => member.Roles.All(role =>
                    guild.CurrentMember.Roles.Any(clientRole => clientRole.Position > role.Position)));

            await RevokeRolesAsync(members, roles);
        }

        /// <summary>
        /// Revokes roles from specified members.
        /// </summary>
        /// <param name="guild">Discord guild.</param>
        /// <param name="memberStrings">Array of member names.</param>
        /// <param name="roleStrings">Array of role names.</param>
        /// <returns>Returns an asynchronous task.</returns>
        /// <exception cref="InvalidOperationException">There are no matching roles or no matching members.</exception>
        public static async Task RevokeRolesAsync(DiscordGuild guild, IEnumerable<string> memberStrings, IEnumerable<string> roleStrings)
        {
            var roles = ConvertRoleNamesToRoles(guild, roleStrings);
            var members = ConvertMemberNamesToMembers(guild, memberStrings);
            await RevokeRolesAsync(await members, await roles);
        }

        /// <summary>
        /// Revokes roles from specified members.
        /// </summary>
        /// <param name="ctx">Command Context.</param>
        /// <param name="members">Array of members.</param>
        /// <param name="roles">Array of roles.</param>
        /// <returns>Returns an asynchronous task.</returns>
        public static async Task RevokeRolesAsync(IEnumerable<DiscordMember> members, IEnumerable<DiscordRole> roles)
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

        /// <summary>
        /// Sends a message to a specific channel.
        /// </summary
        public static Task SendToChannelAsync(CommandContext ctx, string channelName, DiscordEmbed embed) =>
            SendToChannelAsync(ctx, channelName, new DiscordMessageBuilder().WithEmbed(embed));

        /// <summary>
        /// Sends a message to a specific channel.
        /// </summary
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
        /// Sends a message to a specific channel.
        /// </summary>
        public static Task SendToChannelAsync(DiscordGuild guild, string channelName, DiscordEmbed embed) =>
            SendToChannelAsync(guild, channelName, new DiscordMessageBuilder().WithEmbed(embed));

        /// <summary>
        /// Sends a message to a specific channel.
        /// </summary
        public static Task SendToChannelAsync(DiscordGuild guild, string channelName, string message) =>
            SendToChannelAsync(guild, channelName, new DiscordMessageBuilder().WithContent(message));

        /// <summary>
        /// Sends a message to a specific channel.
        /// </summary
        public static async Task SendToChannelAsync(DiscordGuild guild, string channelName, DiscordMessageBuilder messageBuilder)
        {
            var channel = guild.Channels
                .FirstOrDefault(kvp => channelName.Equals(kvp.Value.Name)).Value;

            if (channel == null)
            {
                var errorMessage = $"Channel {channelName} has not been found in guild {guild.Name}.";
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
        /// Converts a username to a user mention string if the user exists in the guild.
        /// </summary>
        /// <param name="guild">Discord guild</param>
        /// <param name="username">Username</param>
        /// <param name="mention">User mention</param>
        /// <returns>True if user exists in guild, false otherwise.</returns>
        public static bool UsernameToUserMention(IEnumerable<DiscordMember> members, string username, out string mention)
        {
            mention = String.Empty;

            var member = members.FirstOrDefault(member => member.Username.Equals(username));
            if (member == null)
                return false;

            mention = member.Mention;
            return true;
        }

        private static IEnumerable<string> Split(this string str,
            Func<char, bool> controller)
        {
            int nextPiece = 0;

            for (int c = 0; c < str.Length; c++)
            {
                if (controller(str[c]))
                {
                    yield return str.Substring(nextPiece, c - nextPiece);
                    nextPiece = c + 1;
                }
            }

            yield return str.Substring(nextPiece);
        }

        private static string TrimMatchingQuotes(this string input, char quote)
        {
            if ((input.Length >= 2) &&
                (input[0] == quote) && (input[input.Length - 1] == quote))
                return input.Substring(1, input.Length - 2);

            return input;
        }

        public static void ParseCustomRaceCommandLineArguments(string rawArgs, out string author, out string name, out string description)
        {
            bool inQuotes = false;

            author = name = description = String.Empty;

            if (rawArgs == null)
                return;

            var args = rawArgs.Split(c =>
                {
                    if (c == '\"')
                        inQuotes = !inQuotes;

                    return !inQuotes && c == ' ';
                })
                .Select(arg => arg.Trim().TrimMatchingQuotes('\"'))
                .Where(arg => !string.IsNullOrEmpty(arg))
                .ToArray();

            var index = 0;
            while (index + 1 < args.Length)
            {
                switch (args[index])
                {
                    case "--author":
                        author = args[++index];
                        break;
                    case "--name":
                        name = args[++index];
                        break;
                    case "--description":
                        description = args[++index];
                        break;
                    default:
                        throw new InvalidOperationException($"Unrecognized option '{args[index]}'");
                }

                ++index;
            }

            if (index < args.Length)
            {
                throw new InvalidOperationException($"Unrecognized option '{args[index]}'");
            }
        }

        private static AttachmentFileType GetAttachmentFileType(CommandContext ctx, out string errorMessage)
        {
            if (ctx.Message.Attachments == null || ctx.Message.Attachments.Count != 1)
            {
                errorMessage = String.Empty;
                return AttachmentFileType.None;
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

        private static async Task<Preset> LoadPresetAttachmentAsync(CommandContext ctx, IReadOnlyDictionary<string, Option> options, Config config)
        {
            var attachment = ctx.Message.Attachments[0];
            var url = attachment.Url;

            var dataStream = await s_Client.GetStreamAsync(url);

            using (StreamReader r = new StreamReader(dataStream))
            {
                var jsonContent = await r.ReadToEndAsync();
                var preset = PresetIO.LoadPreset(jsonContent, options, config);
                if (preset.Equals(default))
                    throw new InvalidOperationException("Could not parse custom json preset. Please supply a valid json file.");

                return preset;
            }
        }

        private static async Task<(Preset Preset, string Seed, string ValidationHash)> LoadLogAttachmentAsync(
            CommandContext ctx,
            string author,
            string name,
            string description,
            IReadOnlyDictionary<string, Option> options)
        {
            // Load attachment file.
            var attachment = ctx.Message.Attachments[0];
            var url = attachment.Url;

            var dataStream = await s_Client.GetStreamAsync(url);

            if (dataStream == null)
                throw new InvalidOperationException($"Could not open attachment file {url}");

            return await LoadLogStreamAsync(dataStream, author, name, description, options);
        }

        public static async Task<(Preset Preset, string Seed, string ValidationHash)> LoadLogFileAsync(
            string logFilePath,
            string author,
            string name,
            string description,
            IReadOnlyDictionary<string, Option> options)
        {
            using var fileStream = File.OpenRead(logFilePath);

            return await LoadLogStreamAsync(fileStream, author, name, description, options);
        }

        private static async Task<(Preset Preset, string Seed, string ValidationHash)> LoadLogStreamAsync(
            Stream stream,
            string author,
            string name,
            string description,
            IReadOnlyDictionary<string, Option> options)
        {
            using (StreamReader r = new StreamReader(stream))
            {
                var allFileTask = r.ReadToEndAsync();

                var seedRegex = new Regex("Seed = (?<seed>[A-F0-9]{1,16})");
                var optionsRegex = new Regex("[[]ALL_OPTIONS[]] = <(?<options>[^>\n]*)");
                var validationHashRegex = new Regex("Hash check value: (?<validationHash>[A-F0-9]{1,8})");

                var allFile = await allFileTask;

                var seedMatch = seedRegex.Match(allFile);
                var optionsMatch = optionsRegex.Match(allFile);
                var validationHashMatch = validationHashRegex.Match(allFile);

                var seed = seedMatch.Success ? seedMatch.Groups["seed"].Value : string.Empty;
                var optionsString = optionsMatch.Success ? optionsMatch.Groups["options"].Value : string.Empty;
                string validationHash = validationHashMatch.Success ? validationHashMatch.Groups["validationHash"].Value : string.Empty;

                var preset = CreatePresetFromOptionsString(String.IsNullOrEmpty(author) ? "Mara" : author,  name, description, optionsString);

                preset.MakeDisplayable(options);
                return (preset, seed, validationHash);
            }
        }

        class MysterySettingSorter : IComparer<KeyValuePair<string, MysterySetting>>
        {
            public int Compare(KeyValuePair<string, MysterySetting> x, KeyValuePair<string, MysterySetting> y)
            {
                var xNoRequirement = String.IsNullOrEmpty(x.Value.Requirement);
                var yNoRequirement = String.IsNullOrEmpty(y.Value.Requirement);

                if (xNoRequirement && yNoRequirement)
                    return x.Key.CompareTo(y.Key);

                if (xNoRequirement)
                    return -1;

                if (yNoRequirement)
                    return 1;

                // x is a requirement of y
                var xRegex = new Regex(x.Key);
                if (xRegex.IsMatch(y.Value.Requirement))
                    return -1;

                // y is a requirement of x
                var yRegex = new Regex(y.Key);
                if (yRegex.IsMatch(x.Value.Requirement))
                    return 1;

                return 0;
            }
        }


        private static async Task<(Preset Preset, string Seed, string ValidationHash)> GenerateMysteryRaceAsync(
            CommandContext ctx,
            string author,
            string name,
            string description,
            IReadOnlyDictionary<string, MysterySetting> mysterySettings,
            IReadOnlyDictionary<string, Option> options)
        {
            var seed = WeeklyUtils.GetRandomSeed();
            var optionsString = String.Empty;

            await Task.Run(() =>
            {
                var sortedMysterySettings = mysterySettings.ToList();
                sortedMysterySettings.Sort(new MysterySettingSorter());

                foreach (var setting in sortedMysterySettings)
                {
                    if (!String.IsNullOrEmpty(setting.Value.Requirement))
                    {
                        // Skip setting if requirement is not met.
                        var regex = new Regex(setting.Value.Requirement);
                        if (!regex.IsMatch(optionsString))
                            continue;
                    }

                    var randomIndex = WeeklyUtils.GetRandomIndex(1, 100);

                    var weight = 0;

                    foreach (var settingValue in setting.Value.Values)
                    {
                        weight += settingValue.Value;
                        if (weight >= randomIndex)
                        {
                            if (!String.IsNullOrEmpty(optionsString)) optionsString += " ";
                            optionsString += $"{setting.Key}={settingValue.Key}";
                            break;
                        }
                    }
                }
            });

            var preset = CreatePresetFromOptionsString( String.IsNullOrEmpty(author) ? "Mara" : author,  name, description, optionsString);

            preset.MakeDisplayable(options);
            return (preset, seed, String.Empty);
        }

        /// <summary>
        /// Loads a race attachment
        /// </summary>
        /// <param name="ctx">Command context.</param>
        /// <param name="rawArgs">Command line arguments</param>
        /// <param name="options">Preset options.</param>
        /// <returns>Tuple of preset and seed string.</returns>
        /// <exception cref="InvalidOperationException">Thrown if there was an error while parsing provided attachment.</exception>
        public static async Task<(Preset Preset, string Seed, string ValidationHash)> GenerateRace(
            CommandContext ctx,
            string author,
            string name,
            string description,
            IReadOnlyDictionary<string, MysterySetting> mysterySettings,
            IReadOnlyDictionary<string, Option> options,
            Config config)
        {
            var attachmentType = GetAttachmentFileType(ctx, out var errorMessage);

            switch (attachmentType)
            {
                case AttachmentFileType.JsonPreset:
                    var preset = await LoadPresetAttachmentAsync(ctx, options, config);
                    return (preset, String.Empty, String.Empty);
                case AttachmentFileType.LogFile:
                    return await LoadLogAttachmentAsync(ctx, author, name, description, options);
                case AttachmentFileType.None:
                    return await GenerateMysteryRaceAsync(ctx, author, name, description, mysterySettings, options);

                default:
                    throw new InvalidOperationException(errorMessage);
            }
        }

        /// <summary>
        /// Generates the validation hash using SoMAncientCave.exe.
        /// </summary>
        public static async Task<(Preset Preset, string Seed, string ValidationHash)> GenerateValidationHash(
            Preset preset,
            string seed,
            Config config,
            IReadOnlyDictionary<string, Option> options,
            MutexRegistry mutexRegistry)
        {
            if (!File.Exists(config.RandomizerExecutablePath))
                throw new ArgumentException("Could not find randomizer executable.");

            if (!File.Exists(config.RomPath))
                throw new ArgumentException("Could not find rom.");

            using var mutexLock = await MutexLock.WaitAsync(mutexRegistry.RandomizerExecutableMutex);

            var tempFolder =
                (Environment.OSVersion.Platform == PlatformID.Unix ||
                 Environment.OSVersion.Platform == PlatformID.MacOSX) ? "/tmp" : "%TEMP%";

            var rawOptionsString = string.Join(" ",
                preset.Options.Select(kvp => $"{kvp.Key}={kvp.Value}")
            );

            if (Environment.OSVersion.Platform == PlatformID.Unix ||
                Environment.OSVersion.Platform == PlatformID.MacOSX)
            {
                Process xvfbProcess = null;
                var framebufferFile = "/tmp/Xvfb_screen0";

                var displayEnv = Environment.GetEnvironmentVariable("DISPLAY");
                if (String.IsNullOrEmpty(displayEnv))
                {
                    var displayNumber = 1;

                    xvfbProcess = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "Xvfb",
                            ArgumentList =
                            {
                                $":{displayNumber}",
                                "-fbdir",
                                "/tmp/"
                            }
                        }
                    };

                    try
                    {
                        xvfbProcess.Start();
                    }
                    catch(Exception exception)
                    {
                        xvfbProcess.Dispose();
                        throw new InvalidOperationException(
                            "This feature requires Xvfb to setup a virtual display.\n" +
                            $"Exception: {exception.Message}"
                        );
                    }

                    var timeout = TimeSpan.FromMilliseconds(5000);
                    DateTimeOffset timeoutAt = DateTimeOffset.UtcNow + timeout;
                    var fileExists = false;
                    while (true)
                    {
                        if (File.Exists(framebufferFile))
                        {
                            fileExists = true;
                            break;
                        }
                        if (DateTimeOffset.UtcNow >= timeoutAt) break;
                        await Task.Delay(10);
                    }

                    if (fileExists)
                        displayEnv = $":{displayNumber}";
                    else
                    {
                        xvfbProcess?.Kill();
                        xvfbProcess?.Dispose();

                        throw new InvalidOperationException(
                            $"Could not find Xvfb framebuffer file {framebufferFile}."
                        );
                    }
                }

                var tcs = new TaskCompletionSource<int>();
                var randomizerProcess = new Process
                {
                    StartInfo =
                    {
                        WorkingDirectory = tempFolder,
                        FileName = "mono",
                        ArgumentList =
                        {
                            $"{config.RandomizerExecutablePath}",
                            $"srcRom={config.RomPath}",
                            $"dstRom={tempFolder}/{seed}.sfc",
                            $"seed={seed}",
                            $"options={rawOptionsString}"
                        }
                    },
                    EnableRaisingEvents = true
                };

                randomizerProcess.StartInfo.EnvironmentVariables["DISPLAY"] = displayEnv;

                randomizerProcess.Exited += (sender, args) =>
                {
                    tcs.SetResult(randomizerProcess.ExitCode);
                    randomizerProcess.Dispose();
                };

                try
                {
                    randomizerProcess.Start();
                }
                catch (Exception exception)
                {
                    xvfbProcess?.Dispose();
                    throw new InvalidOperationException(
                        "This feature requires mono to run the randomizer executable.\n" +
                        $"Exception: {exception.Message}"
                    );
                }

                await tcs.Task;

                File.Delete(framebufferFile);

                xvfbProcess?.Kill();
                xvfbProcess?.Dispose();
            }
            else
            {
                var tcs = new TaskCompletionSource<int>();
                var randomizerProcess = new Process
                {
                    StartInfo =
                    {
                        WorkingDirectory = tempFolder,
                        FileName = $"{config.RandomizerExecutablePath}",
                        ArgumentList =
                        {
                            $"srcRom={config.RomPath}",
                            $"dstRom={tempFolder}/{seed}.sfc",
                            $"seed={seed}",
                            $"options={rawOptionsString}"
                        }
                    },
                    EnableRaisingEvents = true
                };

                randomizerProcess.Start();

                randomizerProcess.Exited += (sender, args) =>
                {
                    tcs.SetResult(randomizerProcess.ExitCode);
                    randomizerProcess.Dispose();
                };

                await tcs.Task;
            }

            return await LoadLogFileAsync($"{tempFolder}/log_{seed}.txt", preset.Author, preset.Name, preset.Description, options);
        }


        /// <summary>
        /// Creates a preset from raw options string.
        /// </summary>
        /// <param name="author">Author of preset.</param>
        /// <param name="name">Name of preset.</param>
        /// <param name="description">Description of preset.</param>
        /// <param name="version">Version of the randomizer.</param>
        /// <param name="optionsString">Raw options string.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown if there was an error while parsing options string.</exception>
        public static Preset CreatePresetFromOptionsString(string author, string name, string description, string optionsString)
        {
            // special case for opRole
            optionsString = Regex.Replace(
                optionsString,
                "opRole=(?<setting>[^\\s]*)",
                "opBoyRole=${setting} opGirlRole=${setting} opSpriteRole=${setting}");

            string[] optionsValues = String.IsNullOrEmpty(optionsString) ? new [] { "mode=rando" } : optionsString.Split(' ');

            var options = new Dictionary<string, string>();

            foreach(string option in optionsValues)
            {
                if(!option.Contains('='))
                    throw new InvalidOperationException($"'{option}' is not formatted correctly. Format must be 'key=value'.");

                string[] values = option.Split('=');
                options[values[0]] = values[1];
            }

            return new Preset(name, description, author, options);
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

        const string k_RabiteStartEmoji = "<:rabiteD:889669696687845377>";
        const string k_RabiteMiddleEmoji = "<:rabiteB:889669135905202268>";
        const string k_RabiteEndEmoji = "<:rabiteE:889669742749704272>";

        public static string IntegerToRabiteEmojis(int n)
        {
            if (n < 1)
                throw new ArgumentOutOfRangeException("Integer must be positive");

            var str = k_RabiteStartEmoji;

            for (int i = 1; i < n; ++i)
            {
                str += k_RabiteMiddleEmoji;
            }

            str += k_RabiteEndEmoji;

            return str;
        }
    }
}

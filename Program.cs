using System;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;

namespace BountyBot
{
    public class Program
    {
        private static bool debugging = false;
        static string botToken;
        const ulong guildID = 944438370308870235;
        const ulong testingGuildID = 854506072457871361;
        static readonly string tokenPath = Directory.GetCurrentDirectory() + "\\token.tok";

        const ulong bountyHunterRoleID = 944815621865111632;
        const ulong oathMsgId = 947299195289731113;
        static void Main(string[] args)
        {
            if (args.Contains("DEBUG"))
                debugging = true;
            if (!File.Exists(tokenPath))
                throw new FileNotFoundException("Please add the bot token to " + tokenPath);
            botToken = File.ReadAllText(tokenPath);
            BountyManager.Init();
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            DiscordClient client = new(new DiscordConfiguration()
            {
                Token = botToken,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All
            });


            var slash = client.UseSlashCommands(); { 
                if (!debugging)
                {
                    slash.RegisterCommands<Commands.TopLevelCommands>(guildID);
                    slash.RegisterCommands<Commands.BountyCommands>(guildID);
                }
                slash.RegisterCommands<Commands.TopLevelCommands>(testingGuildID);
                slash.RegisterCommands<Commands.BountyCommands>(testingGuildID);
            }

            // Event Listeners
            {
                // Oath Check (+,-)
                client.MessageReactionAdded += async (s, e) =>
                {
                    if (e.Message.Id == oathMsgId)
                    {
                        var user = await e.Guild.GetMemberAsync(e.User.Id);
                        var role = e.Guild.Roles[bountyHunterRoleID];
                        if (!user.Roles.Contains(role))
                        {
                            await user.GrantRoleAsync(role);
                            Log.Out("GrantRole", "Acted", ConsoleColor.DarkCyan, "Granted Bounty Hunter Role to " + user.Username + '#' + user.Discriminator + '.');
                        }
                    }
                };
                client.MessageReactionRemoved += async (s, e) =>
                {
                    if (e.Message.Id == oathMsgId)
                    {
                        var user = await e.Guild.GetMemberAsync(e.User.Id);
                        var role = e.Guild.Roles[bountyHunterRoleID];
                        if (user.Roles.Contains(role))
                        {
                            await user.RevokeRoleAsync(role);
                            Log.Out("RevokeRole", "Acted", ConsoleColor.DarkCyan, "Revoked Bounty Hunter Role from " + user.Username + '#' + user.Discriminator + '.');
                        }
                    }
                };

                // Slash Command Errors
                slash.SlashCommandErrored += async (s, e) =>
                {
                    if (e.Exception is SlashExecutionChecksFailedException slex)
                    {
                        foreach (var check in slex.FailedChecks)
                            if (check is Attributes.RequireRoleAttribute att)
                                await e.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(":x: **You do not have permission to run this command.**"));
                    }
                };
            }


            
            await client.ConnectAsync(new DiscordActivity("A Bounty Hunt",ActivityType.Competing));
            await Task.Delay(-1);
        }
    }
}
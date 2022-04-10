using System;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;

namespace BountyBot;
public class Program
{
    private static bool debugging = false;
    static string botToken;
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
        Managers.BountyManager.Init();
        Managers.GuildManager.Init();
        MainAsync(args).GetAwaiter().GetResult();
    }

    static async Task MainAsync(string[] args)
    {
        DiscordClient client = new(new DiscordConfiguration()
        {
            Token = botToken,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All
        });

        var slash = client.UseSlashCommands();
        if (debugging)
            foreach (var guild in Managers.GuildManager.Guilds.Where(x => x.deployment == false))
            {
                slash.RegisterCommands<Commands.TopLevelCommands>(guild.id);
                slash.RegisterCommands<Commands.BountyCommands>(guild.id);
            }
        else
            foreach (var guild in Managers.GuildManager.Guilds)
            {
                slash.RegisterCommands<Commands.TopLevelCommands>(guild.id);
                slash.RegisterCommands<Commands.BountyCommands>(guild.id);
            }

        // Event Listeners
        {
            // Oath Check (+,-)
            client.MessageReactionAdded += AcceptOath;
            client.MessageReactionRemoved += RenounceOath;
            // Slash Command Errors
            slash.SlashCommandErrored += OnError;
        }
        await client.ConnectAsync(new DiscordActivity("A Bounty Hunt", ActivityType.Competing));
        await Task.Delay(-1);
    }

    public static async Task AcceptOath(DiscordClient _, DSharpPlus.EventArgs.MessageReactionAddEventArgs e)
    {
        if (e.Message.Id != oathMsgId)
            return;
        var user = await e.Guild.GetMemberAsync(e.User.Id);
        var role = e.Guild.Roles[bountyHunterRoleID];
        if (user.Roles.Contains(role))
            return;
        await user.GrantRoleAsync(role);
        Log.Out("GrantRole", "Acted", ConsoleColor.DarkCyan, "Granted Bounty Hunter Role to " + user.Username + '#' + user.Discriminator + '.');
    }

    public static async Task RenounceOath(DiscordClient _, DSharpPlus.EventArgs.MessageReactionRemoveEventArgs e)
    {
        if (e.Message.Id != oathMsgId)
            return;
        var user = await e.Guild.GetMemberAsync(e.User.Id);
        var role = e.Guild.Roles[bountyHunterRoleID];
        if (!user.Roles.Contains(role))
            return;
        await user.RevokeRoleAsync(role);
        Log.Out("RevokeRole", "Acted", ConsoleColor.DarkCyan, "Revoked Bounty Hunter Role from " + user.Username + '#' + user.Discriminator + '.');
    }

    public static async Task OnError(SlashCommandsExtension s, DSharpPlus.SlashCommands.EventArgs.SlashCommandErrorEventArgs e)
    {
        if (e.Exception is SlashExecutionChecksFailedException slex)
        {
            foreach (var check in slex.FailedChecks)
                if (check is Attributes.RequireRoleAttribute)
                    await e.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(":x: **You do not have permission to run this command.**"));
        }
    }
}
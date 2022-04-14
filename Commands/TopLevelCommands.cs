using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using static BountyBot.Managers.BountyManager;
using BountyBot.Entities;
using static BountyBot.Entities.Bounty;
using BountyBot.Attributes;

#pragma warning disable CA1822

namespace BountyBot.Commands
{
    internal class TopLevelCommands : ApplicationCommandModule
    {
        private const string committeeRole = "Committee of Bounties";

        [SlashCommand("Bounties", "Shows a list of bounties.")]
        public async Task ListBounties(InteractionContext ctx, [Option("Filter", "Status to filter entries by")] StatusLevel status = StatusLevel.All)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            Bounty[] bounties = status switch
            {
                StatusLevel.All => Bounties,
                StatusLevel.Proposed => ProposedBounties,
                _ => Bounties.Where(x => x.Status == status).ToArray()
            };
            DiscordEmbedBuilder embed = new() { Title = "Bounty Board - " + status.ToString(), Timestamp = DateTime.Now, Color = DiscordColor.Red };
            if (bounties.Length > 0)
                foreach (Bounty bounty in bounties)
                    embed.AddField(bounty.Title, bounty.Body);
            else
                embed.AddField("No bounties posted.", "Talk to a committee member to suggest one.");
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
        }

        [SlashCommand("Points", "Gets the amount of points a user has.")]
        public async Task GetPoints(InteractionContext ctx, [Option("User", "The user to check the points of. Leave blank to check your own.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            string responseString = user.Username + '#' + user.Discriminator + " has " + GetPointsByPlayer(user.Id) + " points.";
            var response = new DiscordWebhookBuilder().WithContent(responseString);
            await ctx.EditResponseAsync(response);
        }

        [SlashCommand("Help", "Displays the help menu.")]
        public async Task ShowHelp(InteractionContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            var embed = new DiscordEmbedBuilder().WithTitle("BountyBot Help Menu - Main Menu")
                .AddField("General", "View bot and quickstart information.")
                .AddField("Top Level Commands","A list of all top level commands and some information on them.")
                .AddField("Bounty Commands","All commands directly affecting bounties.");
            var options = new DiscordSelectComponentOption[]
            {
                new DiscordSelectComponentOption("General", "helpGeneral", "View bot and quickstart information."),
                new DiscordSelectComponentOption("Top Level Commands", "helpTLC", "A list of all top level commands and some information on them."),
                new DiscordSelectComponentOption("Bounty Commands", "helpBounty", "All commands directly affecting bounties."),
                new DiscordSelectComponentOption("Exit", "helpExit", "Exit the help menu.")
            };
            DiscordSelectComponent dropdown = new("helpDropdown", "Navigate...", options);
            var msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(dropdown));
            while (true)
            {
                var res = await msg.WaitForSelectAsync("helpDropdown");
                if (res.TimedOut)
                {
                    dropdown = new("helpDropdown.TO", "Select a submenu...", options, true);
                    await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed.WithFooter("Timed out.")).AddComponents(dropdown));
                    return;
                }
                await res.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                switch (res.Result.Values[0])
                {
                    case "helpGeneral":
                        embed = new DiscordEmbedBuilder().WithTitle("BountyBot Help Menu - General")
                            .AddField("Credits", "Made by WordlessSafe1#0001.")
                            .AddField("Quickstart Info", "Coming Soon");
                        msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(dropdown));
                        continue;
                    case "helpTLC":
                        embed = new DiscordEmbedBuilder().WithTitle("BountyBot Help Menu - Top Level Commands")
                            .AddField("/Help", "Displays this help menu.")
                            .AddField("/Bounties", "Shows a list of bounties.\r\n_Filter_: The status to filter the list by. Leave blank for all bounties.")
                            .AddField("/Points", "Gets the amount of points a user has.\r\n_User_: The user to check the points of. Leave blank to check your own.");
                        msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(dropdown));
                        continue;
                    case "helpBounty":
                        embed = new DiscordEmbedBuilder().WithTitle("BountyBot Help Menu - Bounty Commands")
                            .AddField("/Bounty Propose", "Propose a bounty.");
                        if (ctx.Member.Roles.Where(x => x.Name.ToLower() == committeeRole.ToLower()).Any())
                            embed = embed.AddField($"**The following commands require:**",$"{ctx.Guild.Roles.Where(x => x.Value.Name.ToLower() == committeeRole.ToLower()).FirstOrDefault().Value.Mention}", true)
                                .AddField("/Bounty Set", "Set a bounty on a player.")
                                .AddField("/Bounty Close", "Change the status of a bounty.")
                                .AddField("/Bounty Assign", "Assign a bounty to a user.")
                                .AddField("/Bounty Unassign", "Unassign a user friom a bounty.")
                                .AddField("/Bounty Review", "Review proposed bounties.")
                                .AddField("/Bounty Approve", "Approve a proposed bounty.")
                                .AddField("/Bounty Reject", "Reject a proposed bounty.");
                        msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(dropdown));
                        continue;
                    case "helpExit":
                        await ctx.DeleteResponseAsync();
                        break;
                }
                break;
            }
        }
    }
}

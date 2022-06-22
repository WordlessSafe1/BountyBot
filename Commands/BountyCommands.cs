using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using BountyBot.Attributes;
using BountyBot.Entities;
using static BountyBot.Entities.Bounty;
using static BountyBot.Managers.BountyManager;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;

#pragma warning disable CA1822 // Mark members static - The SlashCommands API does not permit static command functions.

namespace BountyBot.Commands;

[SlashCommandGroup("Bounty", "Manage individual bounties.")]
internal class BountyCommands : ApplicationCommandModule
{
    private const string committeeRole = "Committee of Bounties";

    [SlashCommand("Close", "Closes a bounty by ID."), RequireRole(committeeRole)]
    public async Task CompleteBounty(InteractionContext ctx, [Option("BountyID", "The ID of the bounty to close.")] long longId, [Option("Status", "The status to set. Defaults to Success.")] StatusLevel success = StatusLevel.Success)
    {
        int id = (int)longId;
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        SetBountyStatus(id, success);
        string responseString = "Bounty [" + id + "] has been marked as " + success.ToString();
        var response = new DiscordWebhookBuilder().WithContent(responseString);
        await ctx.EditResponseAsync(response);
        Log.Out("BountyMod", "Noted", ConsoleColor.Blue, "Bounty [" + id + "] marked as " + success.ToString());
    }

    [SlashCommand("Assign", "Assign a bounty to a user."), RequireRole(committeeRole)]
    public async Task AssignBounty(InteractionContext ctx, [Option("BountyID", "The ID of the bounty to assign.")] long bountyID, [Option("User", "The user to assign to the bounty.")] DiscordUser user)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        AssignToBounty((int)bountyID, user.Id);
        string responseString = "Assigned user " + user.Mention + " to bounty [" + bountyID + "].";
        var response = new DiscordWebhookBuilder().WithContent(responseString);
        await ctx.EditResponseAsync(response);
    }

    [SlashCommand("Set", "Set a bounty on a player"), RequireRole(committeeRole)]
    public async Task SetBounty(InteractionContext ctx, [Option("Target", "The person this bounty should target.")] string target, [Option("Value", "The amount this bounty is worth.")] long bountyAmount, [Option("User", "The user to assign to the bounty.")] DiscordUser user = null)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        Bounty bounty = (user == null) ? CreateBounty(target, (int)bountyAmount, ctx.User.Id) : CreateBounty(target, (int)bountyAmount, ctx.User.Id, user.Id);
        string responseString = "A bounty (ID " + bounty.ID + ") has been placed on " + bounty.Target + " for " + bounty.Value + (user == null ? '.' : (". It has been assigned to " + string.Join(", ", bounty.AssignedTo.Select(x => "<@!" + x + ">")) + '.'));
        var response = new DiscordWebhookBuilder().WithContent(responseString);
        await ctx.EditResponseAsync(response);
        Log.Out("BountySet", "Noted", ConsoleColor.Blue, "Bounty [" + bounty.ID + "] created by " + ctx.User.Username + '#' + ctx.User.Discriminator + '.');
    }

    [SlashCommand("Unassign", "Unassign a user friom a bounty."), RequireRole(committeeRole)]
    public async Task UnassignBounty(InteractionContext ctx, [Option("BountyID", "The ID of the affected bounty.")] long bountyID, [Option("User", "The user to assign to the bounty.")] DiscordUser user)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        bool acted = RemoveFromBounty((int)bountyID, user.Id);
        string responseString = acted ? "Unassigned user " + user.Mention + " from bounty [" + bountyID + "]." : "User " + user.Mention + " not found on bounty [" + bountyID + "].";
        var response = new DiscordWebhookBuilder().WithContent(responseString);
        await ctx.EditResponseAsync(response);
    }

    [SlashCommand("Propose", "Propose a bounty.")]
    public async Task ProposeABounty(InteractionContext ctx, [Option("Target", "The person this bounty should target.")] string target, [Option("Value", "The amount this bounty is worth.")] long bountyAmount)
    {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            Bounty bounty = ProposeBounty(target, (int)bountyAmount, ctx.User.Id);
            string responseString = "A bounty (ID " + bounty.ID + ") has been proposed against " + bounty.Target + " for " + bounty.Value + '.';
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent(responseString));
            var userSnowflake = await ctx.Guild.GetMemberAsync(bounty.Author);
            await userSnowflake.SendMessageAsync($"Your proposal for bounty (ID {bounty.ID}) against {bounty.Target} has been submitted!\r\nYou'll receive a DM once it's been reviewed!");
            Log.Out("BountyProposed", "Noted", ConsoleColor.Blue, "Bounty [" + bounty.ID + "] proposed by " + ctx.User.Username + '#' + ctx.User.Discriminator + '.');
    }

    [SlashCommand("Review", "Review proposed bounties"), RequireRole(committeeRole)]
    public async Task ReviewProposals(InteractionContext ctx, [Option("ID","The specific ID to review. Leave blank to review all.")] long bountyID = -1)
    {
        int count = 0;
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        Bounty[] bounties = (bountyID >= 0) ? new[] { ProposedBounties.Where(x => x.ID == bountyID).First() } : ProposedBounties;
        if (!ProposedBounties.Any())
        {
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("No proposals to review!"));
            return;
        }
        foreach (Bounty proposal in bounties)
        {
            var embed = new DiscordEmbedBuilder().AddField(proposal.Title, proposal.Body);
            DiscordMessage msg = await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed).AddComponents(new DiscordComponent[]
             {
                new DiscordButtonComponent(ButtonStyle.Success, "approveProposal", "Approve"),
                new DiscordButtonComponent(ButtonStyle.Danger, "rejectProposal", "Reject"),
                new DiscordButtonComponent(ButtonStyle.Primary, "skipProposal", "Skip"),
                new DiscordButtonComponent(ButtonStyle.Secondary, "exitEncounter", "Exit")
             }));
            var button = await msg.WaitForButtonAsync();
            if (button.TimedOut)
            {
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Timeout reached."));
                return;
            }
            count++;
            await button.Result.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
            Bounty bounty;
            switch (button.Result.Id)
            {
                case "approveProposal":
                    bounty = ApproveBounty(proposal.ID, ctx.User.Id);
                    Log.Out("BountySet", "Noted", ConsoleColor.Blue, $"Bounty [{bounty.ID}] approved by {ctx.User.Username}#{ctx.User.Discriminator}.");
                    await (await ctx.Guild.GetMemberAsync(proposal.Author)).SendMessageAsync($"Your proposal (ID {proposal.ID}) has been approved as bounty [{bounty.ID}]!");
                    continue;
                case "rejectProposal":
                    bounty = RejectBounty(proposal.ID, ctx.User.Id);
                    Log.Out("BountySet", "Noted", ConsoleColor.Blue, $"Bounty [P{bounty.ID}] rejected by {ctx.User.Username}#{ctx.User.Discriminator}.");
                    await (await ctx.Guild.GetMemberAsync(proposal.Author)).SendMessageAsync($"Your proposal (ID {proposal.ID}) has been rejected.");
                    continue;
                case "skipProposal":
                    count--;
                    continue;
                case "exitEncounter":
                default:
                    break;
            }
            break;
        }
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Reviewed {count} bounties!"));
    }
}


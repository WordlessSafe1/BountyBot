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
using static BountyBot.BountyManager;

#pragma warning disable CA1822 // Mark members static - The SlashCommands API does not permit static command functions.

namespace BountyBot.Commands
{
    [SlashCommandGroup("Bounty", "Manage individual bounties.")]
    internal class BountyCommands : SlashCommandModule
    {
        [SlashCommand("Close", "Closes a bounty by ID."), RequireRole( "Committee of Bounties")]
        public async Task CompleteBounty(InteractionContext ctx, [Option("BountyID","The ID of the bounty to close.")] long longId, [Option("Status", "The status to set. Defaults to Success.")] SuccessLevel success = SuccessLevel.Success)
        {
            int id = (int)longId;
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            CloseBounty(id, success);
            string responseString = "Bounty [" + id + "] has been marked as " + success.ToString();
            var response = new DiscordWebhookBuilder().WithContent(responseString);
            await ctx.EditResponseAsync(response);
            Log.Out("BountyMod", "Noted", ConsoleColor.Blue, "Bounty [" + id + "] marked as " + success.ToString());
        }

        [SlashCommand("Assign", "Assign a bounty to a user."), RequireRole("Committee of Bounties")]
        public async Task AssignBounty(InteractionContext ctx, [Option("BountyID", "The ID of the bounty to assign.")] long bountyID, [Option("User", "The user to assign to the bounty.")] DiscordUser user)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            AssignToBounty((int)bountyID, user.Id);
            string responseString = "Assigned user " + user.Mention + " to bounty [" + bountyID + "].";
            var response = new DiscordWebhookBuilder().WithContent(responseString);
            await ctx.EditResponseAsync(response);
        }

        [SlashCommand("Set", "Set a bounty on a player"), RequireRole("Committee of Bounties")]
        public async Task SetBounty(InteractionContext ctx, [Option("Target", "The person this bounty should target.")] string target, [Option("Value", "The amount this bounty is worth.")] long bountyAmount, [Option("User", "The user to assign to the bounty.")] DiscordUser user = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            Bounty bounty = (user == null) ? CreateBounty(target, (int)bountyAmount) : CreateBounty(target, (int)bountyAmount, user.Id);
            string responseString = "A bounty (ID " + bounty.ID + ") has been placed on " + bounty.Target + " for " + bounty.Value + (user == null ? '.' : (". It has been assigned to " + string.Join(", ", bounty.AssignedTo.Select(x => "<@!" + x + ">")) + '.'));
            var response = new DiscordWebhookBuilder().WithContent(responseString);
            await ctx.EditResponseAsync(response);
            Log.Out("BountySet", "Noted", ConsoleColor.Blue, "Bounty [" + bounty.ID + "] created by " + ctx.User.Username + '#' + ctx.User.Discriminator + '.');
        }
        
        [SlashCommand("Unassign", "Unassign a user friom a bounty.")]
        public async Task UnassignBounty(InteractionContext ctx, [Option("BountyID", "The ID of the affected bounty.")] long bountyID, [Option("User", "The user to assign to the bounty.")] DiscordUser user)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            bool acted = RemoveFromBounty((int)bountyID, user.Id);
            string responseString = acted ? "Unassigned user " + user.Mention + " from bounty [" + bountyID + "]." : "User " + user.Mention + " not found on bounty [" + bountyID + "].";
            var response = new DiscordWebhookBuilder().WithContent(responseString);
            await ctx.EditResponseAsync(response);
        }
    }
}

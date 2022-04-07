using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands.Attributes;
using DSharpPlus.SlashCommands;
using static BountyBot.Managers.BountyManager;
using BountyBot.Entities;
using static BountyBot.Entities.Bounty;
using BountyBot.Attributes;

#pragma warning disable CA1822

namespace BountyBot.Commands
{
    internal class TopLevelCommands : ApplicationCommandModule
    {
        [SlashCommand("Bounties", "Shows a list of all bounties.")]
        public async Task ListBounties(InteractionContext ctx, [Option("Filter", "Status to filter entries by")] SuccessLevel status = SuccessLevel.All)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            Bounty[] bounties = (status == SuccessLevel.All ? Bounties : Bounties.Where(x => x.Completed == status).ToArray());
            DiscordEmbedBuilder embed = new()
            {
                Title = "Bounty Board - " + status.ToString(),
                Timestamp = DateTime.Now,
                Color = DiscordColor.Red
            };
            if (bounties.Length > 0)
                foreach (Bounty bounty in bounties)
                { 
                    // (Don't look too hard, or you'll go insane...)
                    // Adds a line for each bounty in the following format:
                    // [Title]:  [ID] (StatusIcon) (Target)
                    // [body]: Worth (Value) | (AssignedTo)
                    embed.AddField(
                        // Title
                        "[" + bounty.ID + "] " + 
                        (bounty.Completed == SuccessLevel.Success ? ":white_check_mark: " : bounty.Completed == SuccessLevel.InProgress ? ":hourglass_flowing_sand: " : ":x: ")
                        + bounty.Target,
                        // Content
                        "Worth " + bounty.Value +
                        (bounty.AssignedTo.Length == 0 ? " | Unassigned" : (" | Assigned to: " + string.Join(", ", bounty.AssignedTo.Select(x => "<@!" + x + ">"))))
                        );
                }
            else
                embed.AddField("No bounties posted.", "Talk to a committee member to suggest one.");
            var response = new DiscordWebhookBuilder().AddEmbed(embed);
            await ctx.EditResponseAsync(response);
        }

        [SlashCommand("Points", "Get the amount of points a user has.")]
        public async Task GetPoints(InteractionContext ctx, [Option("User", "The user to check the points of. Leave blank to check your own.")] DiscordUser user = null)
        {
            if (user == null)
                user = ctx.User;
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            string responseString = user.Username + '#' + user.Discriminator + " has " + GetPointsByPlayer(user.Id) + " points.";
            var response = new DiscordWebhookBuilder().WithContent(responseString);
            await ctx.EditResponseAsync(response);
        }
    }
}

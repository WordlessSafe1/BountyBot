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
        public async Task ListBounties(InteractionContext ctx, [Option("Filter", "Status to filter entries by")] StatusLevel status = StatusLevel.All)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            Bounty[] bounties = (status == StatusLevel.All ? Bounties : Bounties.Where(x => x.Status == status).ToArray());
            DiscordEmbedBuilder embed = new()
            {
                Title = "Bounty Board - " + status.ToString(),
                Timestamp = DateTime.Now,
                Color = DiscordColor.Red
            };
            if (bounties.Length > 0)
                foreach (Bounty bounty in bounties)
                    embed.AddField(bounty.Title, bounty.Body);
            else
                embed.AddField("No bounties posted.", "Talk to a committee member to suggest one.");
            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(embed));
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

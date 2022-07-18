using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using static BountyBot.Managers.BountyManager;
using BountyBot.Entities;

namespace BountyBot.Commands.Providers
{

    public class BountyProvider : IAutocompleteProvider
    {
        /// <summary>
        /// Provides suggestions from bounties.
        /// </summary>
        /// <param name="ctx">The autocomplete context.</param>
        /// <returns>The recommendations for the user.</returns>
        public static async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            List<DiscordAutoCompleteChoice> choices = new();
            string input = (string)ctx.Options.Where(x => x.Focused).First().Value;

            IEnumerable<(Bounty bounty, double similarity)> a = Bounties.Select(x => (
                x, 
                ComputeRcOSim(input, x.PlainTitle)
                ));
            double avg = a.Average(x => x.similarity);
            a = a.Where(x => x.similarity >= avg)
                .OrderByDescending(x => x.similarity);
            foreach (Bounty bounty in a.Select(x => x.bounty))
                choices.Add(new(bounty.PlainTitle, bounty.ID.ToString()));

            return choices;
        }

        // This is only here because the compiler requires an explicit implementation of the interface, but DSharpPlus.SlashCommands fails to hook it. I have no idea why.
        async Task<IEnumerable<DiscordAutoCompleteChoice>> IAutocompleteProvider.Provider(AutocompleteContext ctx) =>
            await Provider(ctx);

        /// <summary>
        /// Computes the similarity between two strings.
        /// </summary>
        /// <param name="a">The input <see cref="string"/>.</param>
        /// <param name="b">The target <see cref="string"/>.</param>
        /// <returns>A <see cref="double"/> representing the similarity of <paramref name="a"/> and <paramref name="b"/>. Higher values are more similar.</returns>
        private static double ComputeRcOSim(string a, string b) =>
            2 * (double)(a.Intersect(b).Count()) /
                   (double)(a.Length + b.Length);
    }
}

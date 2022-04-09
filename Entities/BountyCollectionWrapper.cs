using System;
using System.Text.Json.Serialization;

namespace BountyBot.Entities
{
    /// <summary>
    /// Glorified <see cref="ValueTuple"/> usable for JSON serialization.
    /// </summary>
    internal struct BountyCollectionWrapper
    {
        [JsonInclude]
        public Bounty[] bounties;
        [JsonInclude]
        public Bounty[] proposedBounties;

        [JsonConstructor]
        public BountyCollectionWrapper(Bounty[] bounties, Bounty[] proposedBounties) =>
            (this.bounties, this.proposedBounties) = (bounties, proposedBounties);

        public ValueTuple<Bounty[], Bounty[]> AsTuple() => this;

        // Implicit Conversions
        public static implicit operator ValueTuple<Bounty[], Bounty[]>(BountyCollectionWrapper wrapper) => (wrapper.bounties, wrapper.proposedBounties);
        public static implicit operator BountyCollectionWrapper(ValueTuple<Bounty[], Bounty[]> tuple) => new(tuple.Item1, tuple.Item2);
    }
}
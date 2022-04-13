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
        [JsonInclude]
        public Bounty[] archivedBounties;

        [JsonConstructor]
        public BountyCollectionWrapper(Bounty[] bounties, Bounty[] proposedBounties, Bounty[] archivedBounties) =>
            (this.bounties, this.proposedBounties, this.archivedBounties) = (bounties, proposedBounties, archivedBounties);

        public ValueTuple<Bounty[], Bounty[], Bounty[]> AsTuple() => this;

        // Implicit Conversions
        public static implicit operator ValueTuple<Bounty[], Bounty[], Bounty[]>(BountyCollectionWrapper wrapper) => (wrapper.bounties, wrapper.proposedBounties, wrapper.archivedBounties);
        public static implicit operator BountyCollectionWrapper(ValueTuple<Bounty[], Bounty[], Bounty[]> tuple) => new(tuple.Item1, tuple.Item2, tuple.Item3);
    }
}
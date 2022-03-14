using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BountyBot.Entities
{
    internal class Player
    {
        // Fields
        private readonly ulong uid;
        private int[] bounties;
        private int points;

        // Properties
        public ulong UID { get => uid; }
        public int[] Bounties { get => bounties; }
        public int Points { get => points; }

        // Constructors
        public Player(ulong uid) => this.uid = uid;
        public Player(ulong uid, int[] bounties) => (this.uid, this.bounties) = (uid, bounties);
        [System.Text.Json.Serialization.JsonConstructor]
        public Player(ulong uid, int[] bounties, int points) => (this.uid, this.bounties, this.points) = (uid, bounties, points);

        // Methods
        public void AlterPoints(int pointsToAdd) => points += pointsToAdd;
        public void ResetPoints() => points = 0;
        public void AddBounty(int id) => bounties = bounties.Append(id).ToArray();
        public void AddBounty(int[] ids) => bounties = bounties.Union(ids).ToArray();
        public bool RemoveBounty(int id)
        {
            if (!bounties.Contains(id))
                return false;
            bounties = bounties.Where(x => x != id).ToArray();
            return true;
        }
    }
}

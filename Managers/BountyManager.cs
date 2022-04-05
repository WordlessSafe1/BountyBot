using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using System.IO;
using System.Text.Json;
using BountyBot.Entities;

namespace BountyBot.Managers
{
    static internal class BountyManager
    {
        private static readonly string recordPath = Directory.GetCurrentDirectory() + "\\bounties.dat";

        private static Bounty[] bounties;
        public static Bounty[] Bounties { get => bounties; }

        private static Bounty[] proposedBounties;
        public static Bounty[] ProposedBounties { get => proposedBounties; }

        // Load bounties from file
        public static void Init()
        {
            if (!File.Exists(recordPath))
                SaveBounties(Array.Empty<Bounty>());
            LoadBounties();
        }

        // Fuctions
        public static Bounty CreateBounty(string target, int value, ulong author, params ulong[] assignedTo)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            LoadBounties();
            Bounty bounty = new(bounties.Length, target, value, author, assignedTo);
            bounties = bounties.Append(bounty).ToArray();
            SaveBounties();
            return bounty;
        }

        public static void CloseBounty(int id, Bounty.SuccessLevel success)
        {
            LoadBounties();
            bounties[id].Complete(success);
            SaveBounties();
        }

        public static void AssignToBounty(int id, ulong user)
        {
            LoadBounties();
            bounties[id].AssignUser(user);
            SaveBounties();
        }

        public static bool RemoveFromBounty(int id, ulong user)
        {
            LoadBounties();
            bool success = bounties[id].RemoveUser(user);
            SaveBounties();
            return success;
        }

        public static Bounty ApproveBounty(int id, ulong reviewer)
        {
            LoadBounties();
            Bounty approvedBounty = new(bounties.Length, proposedBounties[id], reviewer);
            proposedBounties = proposedBounties.Where(x => x.ID != id).ToArray();
            bounties = bounties.Append(approvedBounty).ToArray();
            SaveBounties();
            return approvedBounty;
        }

        public static Bounty ProposeBounty(string target, int value, ulong author)
        {
            if(value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            LoadBounties();
            Bounty newBounty = new(proposedBounties.Length, target, value, author);
            proposedBounties = proposedBounties.Append(newBounty).ToArray();
            SaveBounties();
            return newBounty;
        }

        // TEMPORARY Functions
        // Replace with dedicated Player Manager class for scaling purposes - May not be neccessary; Small audience
        public static Bounty[] GetBountiesByPlayer(ulong player) =>
            Bounties.Where(x => x.AssignedTo.Contains(player)).ToArray();
        public static int GetPointsByPlayer(ulong player) =>
            GetBountiesByPlayer(player).Where(x => x.Completed == Bounty.SuccessLevel.Success).Select(x => x.Value).Sum();

        // JSON Functions
        public static Bounty[] LoadBounties() =>
            bounties = JsonSerializer.Deserialize<Bounty[]>(File.ReadAllText(recordPath));
        public static void SaveBounties() =>
            File.WriteAllText(recordPath, JsonSerializer.Serialize(bounties));
        public static void SaveBounties(Bounty[] bounties) =>
            File.WriteAllText(recordPath, JsonSerializer.Serialize(bounties));
    }
}

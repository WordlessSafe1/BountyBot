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
        // Static Definitions
        private static readonly string recordPath = Directory.GetCurrentDirectory() + "\\bounties.dat";

        // Fields
        private static Bounty[] bounties;
        private static Bounty[] proposedBounties;
        private static Bounty[] archivedBounties;

        // Properties
        /// <summary>
        /// Gets all approved <see cref="Bounty"/> objects.
        /// </summary>
        /// <returns>An <see cref="Array"/> of <see cref="Bounty"/> objects.</returns>
        public static Bounty[] ProposedBounties { get => proposedBounties; }
        /// <summary>
        /// Gets all proposed <see cref="Bounty"/> objects.
        /// </summary>
        /// <returns>An <see cref="Array"/> of <see cref="Bounty"/> objects.</returns>
        public static Bounty[] Bounties { get => bounties; }
        /// <summary>
        /// Reserved for future use.
        /// </summary>
        /// <exception cref="NotImplementedException"/>
        public static Bounty[] ArchivedBounties { get => throw new NotImplementedException(); }
        public static Dictionary<int, Bounty> BountiesDB { get; set; } = new();

        // Load bounties from file
        public static void Init()
        {
            if (!File.Exists(recordPath))
                SaveBountiesLegacy((Array.Empty<Bounty>(),Array.Empty<Bounty>(),Array.Empty<Bounty>()));
            try { LoadBountiesFromLocal(); }
            catch (JsonException)
            {
                Log.Out("Init\\BntyMngr", "Info", ConsoleColor.DarkCyan, "JSON format discrepancy detected. Attempting to load from legacy...");
                LoadBountiesFromLocalLegacy();
                proposedBounties = Array.Empty<Bounty>();
                archivedBounties = Array.Empty<Bounty>();
                Log.Out("Init\\BntyMngr", "Info", ConsoleColor.DarkCyan, "Sucessfully loaded from legacy. Attempting to update records...\r\n");
                SaveBountiesLegacy();
                Log.Out("Init\\BntyMngr", "Info", ConsoleColor.DarkCyan, "Sucessfully converted records.");
            }
        }

        // Fuctions
        /// <summary>
        /// Creates a <see cref="Bounty"/>, and adds it to <see cref="Bounties"/>.
        /// </summary>
        /// <param name="target">The username the <see cref="Bounty"/> targets.</param>
        /// <param name="value">The amount of points the <see cref="Bounty"/> is worth.</param>
        /// <param name="author">The discord id of the creator/proposer of the <see cref="Bounty"/>.</param>
        /// <param name="assignedTo">The discord id(s) of the users assigned to the <see cref="Bounty"/>.</param>
        /// <returns>The created <see cref="Bounty"/> object.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Bounty CreateBounty(string target, int value, ulong author, params ulong[] assignedTo)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            LoadBountiesFromLocal();
            Bounty bounty = new(bounties.Length, author, target, value, assignedTo);
            bounties = bounties.Append(bounty).ToArray();
            SaveBountiesLegacy();
            return bounty;
        }
        /// <summary>
        /// Selects a <see cref="Bounty"/> by <paramref name="id"/>, and sets the status to the value of <paramref name="success"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Bounty"/> to alter.</param>
        /// <param name="success">The <see cref="Bounty.StatusLevel"/> to set the status of the <see cref="Bounty"/> to.</param>
        public static void SetBountyStatus(int id, Bounty.StatusLevel success)
        {
            LoadBountiesFromLocal();
            bounties[id].SetStatus(success);
            SaveBountiesLegacy();
        }
        /// <summary>
        /// Assigns a <paramref name="user"/> to a <see cref="Bounty"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Bounty"/> to alter.</param>
        /// <param name="user">The discord id of the <paramref name="user"/> to assign to the <see cref="Bounty"/>.</param>
        public static void AssignToBounty(int id, ulong user)
        {
            LoadBountiesFromLocal();
            bounties[id].AssignUser(user);
            SaveBountiesLegacy();
        }
        /// <summary>
        /// Removes a <paramref name="user"/> from a <see cref="Bounty"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Bounty"/> to alter.</param>
        /// <param name="user">The discord id of the <paramref name="user"/> to remove from the <see cref="Bounty"/>.</param>
        /// <returns>A <see cref="bool"/> reflecting whether the <paramref name="user"/> was found on the <see cref="Bounty"/> identified by <paramref name="id"/>.</returns>
        public static bool RemoveFromBounty(int id, ulong user)
        {
            LoadBountiesFromLocal();
            bool success = bounties[id].RemoveUser(user);
            SaveBountiesLegacy();
            return success;
        }
        /// <summary>
        /// Approves a proposed <see cref="Bounty"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Bounty"/> to alter.</param>
        /// <param name="reviewer">The discord id of the user that approved the <see cref="Bounty"/>.</param>
        /// <returns>A <see cref="Bounty"/> object.</returns>
        public static Bounty ApproveBounty(int id, ulong reviewer)
        {
            LoadBountiesFromLocal();
            Bounty approvedBounty = new(bounties.Length, proposedBounties.Where(x => x.ID == id).First(), reviewer);
            proposedBounties = proposedBounties.Where(x => x.ID != id).ToArray();
            bounties = bounties.Append(approvedBounty).ToArray();
            SaveBountiesLegacy();
            return approvedBounty;
        }
        /// <summary>
        /// Creates a <see cref="Bounty"/> proposal, and adds it to <see cref="ProposedBounties"/>.
        /// </summary>
        /// <param name="target">The username the proposed <see cref="Bounty"/> targets.</param>
        /// <param name="value">The amount of points the proposed <see cref="Bounty"/> is worth.</param>
        /// <param name="author">The discord id of the proposer of the <see cref="Bounty"/>.</param>
        /// <returns>A <see cref="Bounty"/> object.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="NullReferenceException"></exception>
        public static Bounty ProposeBounty(string target, int value, ulong author)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            if (proposedBounties is null)
                throw new NullReferenceException("'proposedBounties' returned null.");
            LoadBountiesFromLocal();
            Bounty newBounty = new( (ProposedBounties.Any() ? proposedBounties.Last().ID + 1 : 0) , author, target, value, Bounty.StatusLevel.Proposed);
            proposedBounties = proposedBounties.Append(newBounty).ToArray();
            SaveBountiesLegacy();
            return newBounty;
        }
        /// <summary>
        /// Rejects a proposed <see cref="Bounty"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Bounty"/> to alter.</param>
        /// <param name="reviewer">The discord id of the user that approved the <see cref="Bounty"/>.</param>
        /// <returns>A <see cref="Bounty"/> object.</returns>
        public static Bounty RejectBounty(int id, ulong reviewer)
        {
            LoadBountiesFromLocal();
            Bounty rejectedBounty = new(id,proposedBounties.Where(x => x.ID == id).First(), reviewer, Bounty.StatusLevel.Rejected);
            proposedBounties = proposedBounties.Where(x => x.ID != id).ToArray();
            SaveBountiesLegacy();
            return rejectedBounty;
        }

        // TEMPORARY Functions
        // Replace with dedicated Player Manager class for scaling purposes - May not be neccessary; Small audience
        /// <summary>
        /// Gets all <see cref="Bounty"/> objects assigned to a <paramref name="player"/>.
        /// </summary>
        /// <param name="player">The discord id of the user to search for.</param>
        /// <returns>An <see cref="Array"/> of <see cref="Bounty"/> objects.</returns>
        public static Bounty[] GetBountiesByPlayer(ulong player) =>
            Bounties.Where(x => x.AssignedTo.Contains(player)).ToArray();
        /// <summary>
        /// Gets the sum of points from all completed <see cref="Bounty"/> objects to which the <paramref name="player"/> is assigned.
        /// </summary>
        /// <param name="player">The discord id of the user to search for.</param>
        /// <returns>An <see cref="int"/> value.</returns>
        public static int GetPointsByPlayer(ulong player) =>
            GetBountiesByPlayer(player).Where(x => x.Status == Bounty.StatusLevel.Success).Select(x => x.Value).Sum();

        // JSON Functions
        /// <summary>
        /// Loads all approved and proposed <see cref="Bounty"/> objects from the disk.
        /// </summary>
        /// <returns>A <see cref="ValueTuple"/>&lt;<see cref="Bounty"/>[], <see cref="Bounty"/>[]&gt; containing the <see cref="Array"/> from <see cref="Bounties"/> and <see cref="ProposedBounties"/>.</returns>
        [Obsolete]
        public static (Bounty[] bounties, Bounty[] proposedBounties, Bounty[] archivedBounties) LoadBountiesFromLocal() =>
            (bounties, proposedBounties, archivedBounties) = JsonSerializer.Deserialize<BountyCollectionWrapper>(File.ReadAllText(recordPath)).AsTuple();
        /// <summary>
        /// <b>*Deprecated*</b> Loads all approved <see cref="Bounty"/> objects from the disk.
        /// </summary>
        /// <returns>An <see cref="Array"/> of <see cref="Bounty"/> objects.</returns>
        [Obsolete]
        public static Bounty[] LoadBountiesFromLocalLegacy() =>
            bounties = JsonSerializer.Deserialize<Bounty[]>(File.ReadAllText(recordPath).Replace("Completed","Status"));
        /// <summary>
        /// Saves all  approved and proposed <see cref="Bounty"/> objects to the disk.
        /// </summary>
        [Obsolete]
        public static void SaveBountiesLegacy() =>
            File.WriteAllText(recordPath, JsonSerializer.Serialize<BountyCollectionWrapper>((bounties, proposedBounties, archivedBounties)));
        /// <summary>
        /// Saves the specified approved and proposed <see cref="Bounty"/> objects to the disk.
        /// </summary>
        /// <param name="records">A <see cref="ValueTuple"/>&lt;<see cref="Bounty"/>[], <see cref="Bounty"/>[]&gt; containing the approved and proposed <see cref="Bounty"/> objects to save.</param>
        [Obsolete]
        public static void SaveBountiesLegacy((Bounty[], Bounty[], Bounty[]) records) =>
            File.WriteAllText(recordPath, JsonSerializer.Serialize<BountyCollectionWrapper>(records));
        public static Bounty CreateBountyInDB(string target, int value, ulong author, params ulong[] assignedTo)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            var bounties = LoadBountiesFromDB();
            Bounty bounty = DatabaseManager.AddBountyToDB(new(bounties.Count, author, target, value, assignedTo));
            bounties.Add(bounty.ID, bounty);
            return bounty;
        }
        public static Dictionary<int,Bounty> LoadBountiesFromDB() =>  BountiesDB = DatabaseManager.GetBounties().ToDictionary(x => x.ID);
    }
}

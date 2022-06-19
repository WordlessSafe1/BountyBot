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
        // Properties
        /// <summary>
        /// Gets all approved <see cref="Bounty"/> objects.
        /// </summary>
        /// <returns>An <see cref="Array"/> of <see cref="Bounty"/> objects.</returns>
        public static Bounty[] ProposedBounties { get => GetBountiesByStatus(Bounty.StatusLevel.Proposed); }
        /// <summary>
        /// Gets all proposed <see cref="Bounty"/> objects.
        /// </summary>
        /// <returns>An <see cref="Array"/> of <see cref="Bounty"/> objects.</returns>
        public static Bounty[] Bounties { get => GetBounties(); }
        /// <summary>
        /// Reserved for future use.
        /// </summary>
        /// <exception cref="NotImplementedException"/>
        public static Bounty[] ArchivedBounties { get => throw new NotImplementedException(); }

        // Load bounties from file
        public static void Init()
        {
            DatabaseManager.CreateTables();
        }

        // Fuctions
        /// <summary>
        /// Selects a <see cref="Bounty"/> by <paramref name="id"/>, and sets the status to the value of <paramref name="success"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Bounty"/> to alter.</param>
        /// <param name="success">The <see cref="Bounty.StatusLevel"/> to set the status of the <see cref="Bounty"/> to.</param>
        public static void SetBountyStatus(int id, Bounty.StatusLevel success)
        {
            Bounty bounty = DatabaseManager.GetBountyByID(id);
            bounty.SetStatus(success);
            UpdateBounty(bounty);
        }
        /// <summary>
        /// Assigns a <paramref name="user"/> to a <see cref="Bounty"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Bounty"/> to alter.</param>
        /// <param name="user">The discord id of the <paramref name="user"/> to assign to the <see cref="Bounty"/>.</param>
        public static void AssignToBounty(int id, ulong user)
        {
            Bounty bounty = DatabaseManager.GetBountyByID(id);
            bounty.AssignUser(user);
            UpdateBounty(bounty);
        }
        /// <summary>
        /// Removes a <paramref name="user"/> from a <see cref="Bounty"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Bounty"/> to alter.</param>
        /// <param name="user">The discord id of the <paramref name="user"/> to remove from the <see cref="Bounty"/>.</param>
        /// <returns>A <see cref="bool"/> reflecting whether the <paramref name="user"/> was found on the <see cref="Bounty"/> identified by <paramref name="id"/>.</returns>
        public static bool RemoveFromBounty(int id, ulong user)
        {
            Bounty bounty = DatabaseManager.GetBountyByID(id);
            bounty.RemoveUser(user);
            var success = bounty.RemoveUser(user);
            if (success)
                UpdateBounty(bounty);
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
            Bounty bounty = DatabaseManager.GetBountyByID(id);
            bounty = new(bounty, reviewer);
            return UpdateBounty(bounty);
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
            Bounty bounty = new(author, target, value, Bounty.StatusLevel.Proposed);
            return DatabaseManager.AddBountyToDB(bounty);
        }
        /// <summary>
        /// Rejects a proposed <see cref="Bounty"/>.
        /// </summary>
        /// <param name="id">The id of the <see cref="Bounty"/> to alter.</param>
        /// <param name="reviewer">The discord id of the user that approved the <see cref="Bounty"/>.</param>
        /// <returns>A <see cref="Bounty"/> object.</returns>
        public static Bounty RejectBounty(int id, ulong reviewer)
        {
            Bounty bounty = DatabaseManager.GetBountyByID(id);
            Bounty rejectedBounty = new(bounty, reviewer, Bounty.StatusLevel.Rejected);
            return UpdateBounty(rejectedBounty);
        }

        // TEMPORARY Functions
        // Replace with dedicated Player Manager class for scaling purposes - May not be neccessary; Small audience
        /// <summary>
        /// Gets all <see cref="Bounty"/> objects assigned to a <paramref name="player"/>.
        /// </summary>
        /// <param name="player">The discord id of the user to search for.</param>
        /// <returns>An <see cref="Array"/> of <see cref="Bounty"/> objects.</returns>
        public static Bounty[] GetBountiesByPlayer(ulong player) =>
            DatabaseManager.GetBountiesAssignedToPlayer(player);
        /// <summary>
        /// Gets the sum of points from all completed <see cref="Bounty"/> objects to which the <paramref name="player"/> is assigned.
        /// </summary>
        /// <param name="player">The discord id of the user to search for.</param>
        /// <returns>An <see cref="int"/> value.</returns>
        public static int GetPointsByPlayer(ulong player) =>
            GetBountiesByPlayer(player).Where(x => x.Status == Bounty.StatusLevel.Success).Select(x => x.Value).Sum();

        // Database Functions
        public static Bounty CreateBounty(string target, int value, ulong author, params ulong[] assignedTo)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(nameof(value));
            return DatabaseManager.AddBountyToDB(new(author, target, value, assignedTo));
        }
        public static Bounty UpdateBounty(int id, Bounty bounty) => DatabaseManager.UpdateBounty(id, bounty);
        public static Bounty UpdateBounty(Bounty bounty) => DatabaseManager.UpdateBounty(bounty.ID, bounty);
        private static Bounty[] GetBounties() =>  DatabaseManager.GetBounties();
        private static Bounty[] GetBountiesByStatus(Bounty.StatusLevel status) => DatabaseManager.GetBountiesByStatus(status);
    }
}

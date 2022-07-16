using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
namespace BountyBot.Entities
{
    internal class Bounty
    {
        // Static Definitions

        /// <summary>
        /// Defines <see cref="Bounty"/> status levels.
        /// </summary>
        public enum StatusLevel { None = -99, Fail = -1, InProgress, Success, Active = 10, Proposed = 80, Rejected, All = 99 }
        /// <summary>
        /// Defines a discord emoji <see cref="string"/> for each <see cref="StatusLevel"/>.
        /// </summary>
        public static readonly Dictionary<StatusLevel, string> Icons = new() {
            { StatusLevel.Success, ":white_check_mark:" },
            { StatusLevel.InProgress, ":hourglass_flowing_sand:" },
            { StatusLevel.Fail, ":x:" },
            { StatusLevel.Proposed, ":question:" },
            // Unused
            { StatusLevel.Rejected, ":-1:" },
            { StatusLevel.All, "" },
            { StatusLevel.Active, "" },
            { StatusLevel.None, "" }
        };

        // Fields
        private int id = -1;
        private readonly string target;
        private readonly DateTime createdAt;
        private readonly int value;
        private StatusLevel status; // -1 = Fail, 0 = In Progress, 1 = Success
        private ulong[] assignedTo;
        private readonly ulong author;
        private readonly ulong reviewer;

        // Properties
        /// <summary>
        /// The ID of the <see cref="Bounty"/>.
        /// </summary>
        public int ID { get => id; internal set => id = value; }
        /// <summary>
        /// The person targeted by the <see cref="Bounty"/>.
        /// </summary>
        public string Target { get => target; }
        /// <summary>
        /// When this <see cref="Bounty"/> was created.
        /// </summary>
        public DateTime CreatedAt { get => createdAt; }
        /// <summary>
        /// How many points this <see cref="Bounty"/> is worth.
        /// </summary>
        public int Value { get => value; }
        /// <summary>
        /// The current status of this <see cref="Bounty"/>.
        /// </summary>
        public StatusLevel Status { get => status; }
        /// <summary>
        /// Gets an array containing the users assigned to this <see cref="Bounty"/>.
        /// </summary>
        public ulong[] AssignedTo { get => assignedTo; }
        /// <summary>
        /// Gets the discord id of the user that created or proposed this <see cref="Bounty"/>.
        /// </summary>
        public ulong Author { get => author; }
        /// <summary>
        /// Gets the discord id of the user that approved this <see cref="Bounty"/>.
        /// </summary>
        public ulong Reviewer { get => reviewer; }

        // Computing Properties

        /// <summary>
        /// Gets the <see cref="string"/> used in the title of an embed.
        /// </summary>
        /// <returns>A <see cref="string"/>.</returns>
        public string Title { get => $"[{ID}] {Icon} {Target}"; }
        public string PlainTitle { get => $"[{ID}] {Target}"; }
        /// <summary>
        /// Gets the <see cref="string"/> used in the body of an embed.
        /// </summary>
        /// <returns>A <see cref="string"/>.</returns>
        public string Body { get => $"Worth {Value} | {Assignments}"; }
        /// <summary>
        /// Gets the icon associated with the bounty's <see cref="StatusLevel"/>.
        /// </summary>
        /// <returns>A discord emoji <see cref="string"/>.</returns>
        public string Icon { get => Icons[Status]; }
        /// <summary>
        /// Gets a <see cref="string"/> mentioning users assigned to the bounty.
        /// </summary>
        private string Assignments { get => (AssignedTo.Length == 0) ? "Unassigned" : "Assigned to: " + string.Join(", ", AssignedTo.Select(x => "<@!" + x + ">")); }

        // Constructors
        internal Bounty(int id, string target, DateTime createdAt, int value, StatusLevel status, ulong author, ulong reviewer, ulong[] assignedTo) =>
            (this.id, this.target, this.createdAt, this.value, this.status, this.author, this.reviewer, this.assignedTo) =
            (id, target, createdAt, value, status, author, reviewer, assignedTo);

        public Bounty(ulong author, string target, int value, params ulong[] assignedTo) =>
            (this.target, this.value, this.author, this.reviewer, this.assignedTo, createdAt, status) =
            (target, value, author, author, assignedTo, DateTime.Now, 0);
        public Bounty(ulong author, string target, int value, StatusLevel status, params ulong[] assignedTo) =>
            (this.target, this.value, this.author, this.reviewer, this.status, this.assignedTo, createdAt) =
            (target, value, author, author, status, assignedTo, DateTime.Now);
        public Bounty(Bounty bounty, ulong reviewer, StatusLevel status = 0) =>
            (this.id, this.target, this.value, this.author, this.assignedTo, createdAt, this.status, this.reviewer) =
            (bounty.id, bounty.target, bounty.value, bounty.author, bounty.AssignedTo, DateTime.Now, status, reviewer);
        public Bounty(string target, DateTime createdAt, int value, StatusLevel status, ulong author, ulong reviewer, ulong[] assignedTo) =>
            (this.target, this.createdAt, this.value, this.status, this.author, this.reviewer, this.assignedTo) =
            (target, createdAt, value, status, author, reviewer, assignedTo);

        // Methods

        /// <summary>
        /// Adds a user to <see cref="AssignedTo"/>.
        /// </summary>
        /// <param name="user">The ID of the discord user to assign the bounty.</param>
        public void AssignUser(ulong user) =>
            assignedTo = assignedTo.Append(user).ToArray();
        /// <summary>
        /// Adds users to <see cref="AssignedTo"/>.
        /// </summary>
        /// <param name="user">The IDs of the discord users to assign the bounty.</param>
        public void AssignUser(params ulong[] user) =>
            assignedTo = assignedTo.Union(user).ToArray();
        /// <summary>
        /// Sets <see cref="Status"/>.
        /// </summary>
        /// <param name="level">The <see cref="StatusLevel"/> to set the bounty at.</param>
        public void SetStatus(StatusLevel level) => status = level;
        /// <summary>
        /// Unassigns a user from the bounty.
        /// </summary>
        /// <param name="user">The ID of the discord user to remove from the bounty.</param>
        /// <returns>A <see cref="bool"/> reflecting whether or not <paramref name="user"/> was found on the bounty.</returns>
        public bool RemoveUser(ulong user)
        {
            if (!assignedTo.Contains(user))
                return false;
            assignedTo = assignedTo.Where(x => x != user).ToArray();
            return true;
        }
        
    }
}

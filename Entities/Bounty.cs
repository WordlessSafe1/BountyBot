using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using System.Text.Json.Serialization;
namespace BountyBot.Entities
{
    internal class Bounty
    {
        // Static Definitions

        /// <summary>
        /// Defines <see cref="Bounty"/> status levels.
        /// </summary>
        public enum SuccessLevel { Fail = -1, InProgress, Success, Proposed = 80, All = 99 }
        /// <summary>
        /// Defines a discord emoji <see cref="string"/> for each <see cref="SuccessLevel"/>.
        /// </summary>
        public static readonly Dictionary<SuccessLevel, string> Icons = new() {
            { SuccessLevel.Success, ":white_check_mark:" },
            { SuccessLevel.InProgress, ":hourglass_flowing_sand:" },
            { SuccessLevel.Fail, ":x:" },
            { SuccessLevel.Proposed, "" },
            { SuccessLevel.All, "" } // Unused
        };

        // Fields
        private readonly int id;
        private readonly string target;
        private readonly DateTime createdAt;
        private readonly int value;
        private SuccessLevel completed; // -1 = Fail, 0 = In Progress, 1 = Success
        private ulong[] assignedTo;
        private ulong author;
        private ulong reviewer;

        // Properties
        /// <summary>
        /// The ID of the <see cref="Bounty"/>.
        /// </summary>
        public int ID { get => id; }
        /// <summary>
        /// The person targetted by the <see cref="Bounty"/>.
        /// </summary>
        public string Target { get => target; }
        /// <summary>
        /// When this <see cref="Bounty"/> was created.
        /// </summary>
        public DateTime CreatedAt { get => createdAt; }
        /// <summary>
        /// How much this <see cref="Bounty"/> is worth.
        /// </summary>
        public int Value { get => value; }
        /// <summary>
        /// The current status of this <see cref="Bounty"/>.
        /// </summary>
        public SuccessLevel Completed { get => completed; }
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
        [JsonIgnore]
        public string Title { get => $"[{ID}] {Icon} {Target}"; }
        /// <summary>
        /// Gets the <see cref="string"/> used in the body of an embed.
        /// </summary>
        /// <returns>A <see cref="string"/>.</returns>
        [JsonIgnore]
        public string Body { get => $"Worth {Value} | {Assignments}"; }
        /// <summary>
        /// Gets the icon associated with the bounty's <see cref="SuccessLevel"/>.
        /// </summary>
        /// <returns>A discord emoji <see cref="string"/>.</returns>
        [JsonIgnore]
        public string Icon { get => Icons[Completed]; }
        /// <summary>
        /// Gets a <see cref="string"/> mentioning users assigned to the bounty.
        /// </summary>
        [JsonIgnore]
        private string Assignments { get => (AssignedTo.Length == 0) ? "Unassigned" : "Assigned to: " + string.Join(", ", AssignedTo.Select(x => "<@!" + x + ">")); }

        // Constructors
        public Bounty(int id, ulong author, string target, int value, params ulong[] assignedTo) =>
            (this.id, this.target, this.value, this.author, this.reviewer, this.assignedTo, createdAt, completed) =
            (id, target, value, author, author, assignedTo, DateTime.Now, 0);
        public Bounty(int id, ulong author, string target, int value, SuccessLevel status, params ulong[] assignedTo) =>
            (this.id, this.target, this.value, this.author, this.reviewer, this.completed, this.assignedTo, createdAt, completed) =
            (id, target, value, author, author, status, assignedTo, DateTime.Now, 0);
        public Bounty(int id, Bounty bounty, ulong reviewer) =>
            (this.id, this.target, this.value, this.author, this.assignedTo, createdAt, completed, this.reviewer) =
            (id, bounty.target, bounty.value, bounty.author, bounty.AssignedTo, DateTime.Now, 0, reviewer);

        [JsonConstructor]
        public Bounty(int id, string target, DateTime createdAt, int value, SuccessLevel completed, ulong author, ulong reviewer, ulong[] assignedTo) =>
            (this.id, this.target, this.createdAt, this.value, this.completed, this.author, this.reviewer, this.assignedTo) =
            (id, target, createdAt, value, completed, author, reviewer, assignedTo);

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
        /// Sets <see cref="Completed"/>.
        /// </summary>
        /// <param name="level">The <see cref="SuccessLevel"/> to set the bounty at.</param>
        public void Complete(SuccessLevel level) => completed = level;
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

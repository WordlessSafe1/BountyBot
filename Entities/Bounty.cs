using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
namespace BountyBot.Entities
{
    internal class Bounty
    {
        // Static Definitions
        public enum SuccessLevel { Fail = -1, InProgress, Success, All = 99 }

        // Fields
        private int id;
        private readonly string target;
        private readonly DateTime createdAt;
        private readonly int value;
        private SuccessLevel completed; // -1 = Fail, 0 = In Progress, 1 = Success
        private ulong[] assignedTo;
        private ulong author;
        private ulong reviewer;

        // Properties
        public int ID { get => id; }
        public string Target { get => target; }
        public DateTime CreatedAt { get => createdAt; }
        public int Value { get => value; }
        public SuccessLevel Completed { get => completed; }
        public ulong[] AssignedTo { get => assignedTo; }
        public ulong Author { get => author; }
        public ulong Reviewer { get => reviewer; }

        // Constructors
        public Bounty(int id, string target, int value, ulong author, params ulong[] assignedTo) =>
            (this.id, this.target, this.value, this.author, this.assignedTo, createdAt, completed) =
            (id, target, value, author, assignedTo, DateTime.Now, 0);
        public Bounty(int id, Bounty bounty, ulong reviewer) =>
            (this.id, this.target, this.value, this.author, this.assignedTo, createdAt, completed, this.reviewer) =
            (id, bounty.target, bounty.value, bounty.author, bounty.AssignedTo, DateTime.Now, 0, reviewer);

        [System.Text.Json.Serialization.JsonConstructor]
        public Bounty(int id, string target, DateTime createdAt, int value, SuccessLevel completed, ulong[] assignedTo, ulong author, ulong reviewer) =>
            (this.id, this.target, this.createdAt, this.value, this.completed, this.assignedTo, this.author, this.reviewer) =
            (id, target, createdAt, value, completed, assignedTo, author, reviewer);

        // Methods
        public void AssignUser(ulong user) =>
            assignedTo = assignedTo.Append(user).ToArray();
        public void AssignUser(params ulong[] user) =>
            assignedTo = assignedTo.Union(user).ToArray();
        public void Complete(SuccessLevel level) => completed = level;
        public bool RemoveUser(ulong user)
        {
            if (!assignedTo.Contains(user))
                return false;
            assignedTo = assignedTo.Where(x => x != user).ToArray();
            return true;
        }
    }
}

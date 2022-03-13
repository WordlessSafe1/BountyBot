using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Entities;
namespace BountyBot
{
    internal class Bounty
    {
        // Static Definitions
        public enum SuccessLevel { Fail = -1, InProgress, Success, All = 99 }

        // Fields
        private readonly int id;
        private readonly string target;
        private readonly DateTime createdAt;
        private readonly int value;
        private SuccessLevel completed; // -1 = Fail, 0 = In Progress, 1 = Success
        private ulong[] assignedTo;

        // Properties
        public int ID { get => id; }
        public string Target { get => target; }
        public DateTime CreatedAt { get => createdAt; }
        public int Value { get => value; }
        public SuccessLevel Completed { get => completed; }
        public ulong[] AssignedTo { get => assignedTo; }

        // Constructors
        public Bounty() =>
            assignedTo = Array.Empty<ulong>();
        public Bounty(int id, string target, int value) =>
            (this.id, this.target, this.value, createdAt, completed, assignedTo) =
            (id, target, value, DateTime.Now, 0, Array.Empty<ulong>());
        public Bounty(int id, string target, int value, params ulong[] assignedTo) =>
            (this.id, this.target, this.value, this.assignedTo, createdAt, completed) =
            (id, target, value, assignedTo, DateTime.Now, 0);

        [System.Text.Json.Serialization.JsonConstructor]
        public Bounty(int id, string target, DateTime createdAt, int value, SuccessLevel completed, ulong[] assignedTo) =>
            (this.id, this.target, this.createdAt, this.value, this.completed, this.assignedTo) =
            (id, target, createdAt, value, completed, assignedTo);

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

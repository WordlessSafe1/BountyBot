using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using System.Collections.Generic;

namespace BountyBot.Attributes
{
    [Obsolete]
    public class RequireRoleAttribute : SlashCheckBaseAttribute
    {
        public string roleName;

        public RequireRoleAttribute(string roleName) => this.roleName = roleName.ToLower();

#pragma warning disable CS1998 // Await operator not present - Roles must be retrieved synchronously
        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx) =>
            ctx.Member.Roles.Select(x => x.Name.ToLower()).Contains(roleName);
#pragma warning restore CS1998 // Await operator not present
    }

    public class RequireRolesAttribute : SlashCheckBaseAttribute
    {
        public enum CheckMethod
        {
            /// <summary>
            /// Command only runs if the user does not have any of the specified roles.
            /// </summary>
            None,
            /// <summary>
            /// Command only runs if the user has any of the specified roles.
            /// </summary>
            Any,
            /// <summary>
            /// Command only runs if the user has exactly the specified roles -- no more, no less.
            /// </summary>
            Exactly,
            /// <summary>
            /// Command only runs if the user has all of the specified roles.
            /// </summary>
            All
        };
        private CheckMethod checkMethod;
        private IEnumerable<string> roleNames;
        private IEnumerable<ulong> roleIDs;

        public RequireRolesAttribute(CheckMethod checkMethod, params string[] roleNames) =>
            (this.roleNames, this.checkMethod) = (roleNames?.Select(x => x.ToLower()), checkMethod);
        public RequireRolesAttribute(CheckMethod checkMethod, params ulong[] roleIDs) =>
            (this.roleIDs, this.checkMethod) = (roleIDs, checkMethod);

#pragma warning disable CS1998
        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            if (roleIDs is null && roleNames is null)
                throw new ArgumentNullException("roleNames or roleIDs must be specified.");
            var userRoles = ctx.Member.Roles;
            bool useID = roleIDs is not null;
            return (useID) ? CheckID(userRoles.Select(x => x.Id)) : CheckName(userRoles.Select(x => x.Name.ToLower()));
        }
#pragma warning restore CS1998

        private bool CheckID(IEnumerable<ulong> expectedRoles) =>
            checkMethod switch
            {
                CheckMethod.All => AllCheckID(expectedRoles),
                CheckMethod.Exactly => ExactlyCheckID(expectedRoles),
                CheckMethod.Any => AnyCheckID(expectedRoles),
                CheckMethod.None => NoneCheckID(expectedRoles),
                _ => throw new NotImplementedException()
            };

        private bool AllCheckID(IEnumerable<ulong> expectedRoles) =>
            roleIDs.Count() == expectedRoles.Count() && roleIDs.Where(x => expectedRoles.Contains(x)).Count() == expectedRoles.Count();
        private bool ExactlyCheckID(IEnumerable<ulong> expectedRoles)
        {
            bool pass = true;
            roleIDs.ToList().ForEach(x => pass &= expectedRoles.Contains(x));
            expectedRoles.ToList().ForEach(x => pass &= roleIDs.Contains(x));
            return pass;
        }
        private bool AnyCheckID(IEnumerable<ulong> expectedRoles) =>
            roleIDs.Where(x => expectedRoles.Contains(x)).Any();
        private bool NoneCheckID(IEnumerable<ulong> expectedRoles) =>
            !roleIDs.Where(x => expectedRoles.Contains(x)).Any();


        private bool CheckName(IEnumerable<string> expectedRoles) =>
            checkMethod switch
            {
                CheckMethod.All => AllCheckName(expectedRoles),
                CheckMethod.Exactly => ExactlyCheckName(expectedRoles),
                CheckMethod.Any => AnyCheckName(expectedRoles),
                CheckMethod.None => NoneCheckName(expectedRoles),
                _ => throw new NotImplementedException()
            };

        private bool AllCheckName(IEnumerable<string> expectedRoles) =>
            roleNames.Count() == expectedRoles.Count() && roleNames.Where(x => expectedRoles.Contains(x)).Count() == expectedRoles.Count();
        private bool ExactlyCheckName(IEnumerable<string> expectedRoles)
        {
            bool pass = true;
            roleNames.ToList().ForEach(x => pass &= expectedRoles.Contains(x));
            expectedRoles.ToList().ForEach(x => pass &= roleNames.Contains(x));
            return pass;
        }
        private bool AnyCheckName(IEnumerable<string> expectedRoles) =>
            roleNames.Where(x => expectedRoles.Contains(x)).Any();
        private bool NoneCheckName(IEnumerable<string> expectedRoles) =>
            !roleNames.Where(x => expectedRoles.Contains(x)).Any();
    }
}

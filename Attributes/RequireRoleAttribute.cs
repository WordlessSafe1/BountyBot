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
        public RequireRolesAttribute(params string[] roleNames) =>
            (this.checkMethod, this.roleNames) = (CheckMethod.All, roleNames);
        public RequireRolesAttribute(params ulong[] roleIDs) =>
            (this.checkMethod, this.roleIDs) = (CheckMethod.All, roleIDs);

#pragma warning disable CS1998
        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx)
        {
            if (roleIDs is null && roleNames is null)
                throw new ArgumentNullException("roleNames or roleIDs must be specified.");
            var userRoles = ctx.Member.Roles;
            bool useID = roleIDs is not null;
            return useID ? GenCheck(roleIDs, userRoles.Select(x => x.Id)) : GenCheck(roleNames, userRoles.Select(x => x.Name));
        }
#pragma warning restore CS1998

        private bool GenCheck<T>(IEnumerable<T> expected, IEnumerable<T> actual) =>
            checkMethod switch
            {
                CheckMethod.All => GenCheckAll(expected, actual),
                CheckMethod.Exactly => GenCheckExactly(expected, actual),
                CheckMethod.Any => GenCheckAny(expected, actual),
                CheckMethod.None => GenCheckNone(expected, actual),
                _ => throw new NotImplementedException()
            };

        private static bool GenCheckAll<T>(IEnumerable<T> expected, IEnumerable<T> actual) =>
            actual.All(expected.Contains);
        private static bool GenCheckAny<T>(IEnumerable<T> expected, IEnumerable<T> actual) =>
            actual.Any(expected.Contains);
        private static bool GenCheckExactly<T>(IEnumerable<T> expected, IEnumerable<T> actual) =>
            actual.All(expected.Contains) && actual.Count() == expected.Count();
        private static bool GenCheckNone<T>(IEnumerable<T> expected, IEnumerable<T> actual) =>
            actual.All(x => !expected.Contains(x));
    }
}

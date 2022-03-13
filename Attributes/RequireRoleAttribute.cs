using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;

namespace BountyBot.Attributes
{
    public class RequireRoleAttribute : SlashCheckBaseAttribute
    {
        public string roleName;

        public RequireRoleAttribute(string roleName) => this.roleName = roleName.ToLower();

#pragma warning disable CS1998
        public override async Task<bool> ExecuteChecksAsync(InteractionContext ctx) =>
            ctx.Member.Roles.Select(x => x.Name.ToLower()).Contains(roleName);
#pragma warning restore CS1998
    }
}

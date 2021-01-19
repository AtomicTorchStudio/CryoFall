// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Faction
{
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.Faction;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ConsoleFactionSetRole : BaseConsoleCommand
    {
        public override string Description =>
            "Changes the faction role of the provided character. Cannot change to leader and cannot change from leader to another role.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "faction.setRole";

        public string Execute([CurrentCharacterIfNull] ICharacter player, FactionMemberRole role)
        {
            var faction = FactionSystem.ServerGetFaction(player);
            if (faction is null)
            {
                return $"Player {player.Name} has no faction";
            }

            if (!Api.IsEditor)
            {
                var currentRole = FactionSystem.ServerGetRole(player);
                if (currentRole == FactionMemberRole.Leader)
                {
                    return "Cannot change the role of the faction leader";
                }

                if (role == FactionMemberRole.Leader)
                {
                    return "Cannot assign a leader role to anyone";
                }
            }

            FactionSystem.ServerSetMemberRoleNoChecks(player.Name, faction, role);
            return "Role changed to " + role;
        }
    }
}
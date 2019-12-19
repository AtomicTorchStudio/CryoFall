// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using System.Text;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ConsoleAdminListLandClaims : BaseConsoleCommand
    {
        public override string Description =>
            "List all the land claims of the specified player or of all the players.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.listLandClaims";

        public string Execute()
        {
            var result = new StringBuilder("List of all players with their owned land claims:")
                .AppendLine();

            foreach (var p in Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: false))
            {
                result.AppendLine();
                AppendInfoAboutPlayerLandClaims(p, result);
            }

            return result.ToString();
        }

        public string Execute(ICharacter player)
        {
            var result = new StringBuilder();
            if (!AppendInfoAboutPlayerLandClaims(player, result))
            {
                result.Append(player).Append(" doesn't have access to any land claim");
            }

            return result.ToString();
        }

        private static bool AppendInfoAboutPlayerLandClaims(ICharacter player, StringBuilder result)
        {
            var ownedLandClaimAreas = PlayerCharacter.GetPrivateState(player)
                                                     .OwnedLandClaimAreas;

            if (ownedLandClaimAreas is null
                || ownedLandClaimAreas.Count == 0)
            {
                return false;
            }

            var worldBoundsOffset = Server.World.WorldBounds.Offset;
            result.Append("Owned land claims by ")
                  .Append(player)
                  .Append(":");

            foreach (var ownedLandClaimArea in ownedLandClaimAreas)
            {
                result.AppendLine();
                var publicState = LandClaimArea.GetPublicState(ownedLandClaimArea);
                result.AppendFormat(" * {0} at {1}",
                                    publicState.ProtoObjectLandClaim.ShortId,
                                    publicState.LandClaimCenterTilePosition - worldBoundsOffset);
            }

            return true;
        }
    }
}
// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using System.Linq;
    using System.Text;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;

    public class ConsoleAdminListLandClaimsOwners : BaseConsoleCommand
    {
        public override string Description =>
            "List all the land claims with their owners.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.listLandClaimsOwners";

        public string Execute(byte minOwnersNumber = 1)
        {
            if (minOwnersNumber < 1)
            {
                minOwnersNumber = 1;
            }

            var result = new StringBuilder("List of all land claims with their access lists: (with at least ")
                         .Append(minOwnersNumber)
                         .Append(" owner(s))")
                         .AppendLine();

            var worldBoundsOffset = Server.World.WorldBounds.Offset;
            foreach (var area in LandClaimSystem.SharedEnumerateAllAreas())
            {
                var privateState = LandClaimArea.GetPrivateState(area);
                var landClaimOwners = privateState.ServerGetLandOwners();
                var ownersCount = landClaimOwners.Count();
                if (ownersCount < minOwnersNumber)
                {
                    continue;
                }

                var publicState = LandClaimArea.GetPublicState(area);
                result.AppendLine()
                      .Append(publicState.ProtoObjectLandClaim.ShortId)
                      .Append(" at ")
                      .Append(publicState.LandClaimCenterTilePosition - worldBoundsOffset)
                      .Append(" — ");

                var factionClanTag = LandClaimSystem.SharedGetAreaOwnerFactionClanTag(area);
                if (!string.IsNullOrEmpty(factionClanTag))
                {
                    result.AppendFormat("faction [{0}] — ",
                                        factionClanTag);
                }

                result.Append(ownersCount)
                      .Append(" owner(s)");

                foreach (var ownerName in landClaimOwners)
                {
                    result.AppendLine()
                          .Append(" * ")
                          .Append(ownerName);
                }
            }

            return result.ToString();
        }
    }
}
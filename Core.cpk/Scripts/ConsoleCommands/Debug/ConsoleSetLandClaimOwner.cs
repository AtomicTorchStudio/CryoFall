namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ConsoleSetLandClaimOwner : BaseConsoleCommand
    {
        public override string Description => "Sets the land claim founder and resets the access list.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "debug.setLandClaimOwner";

        public static string ServerSetOwner(ushort x, ushort y, string newOwnerName)
        {
            using var tempList = Api.Shared.GetTempList<ILogicObject>();
            LandClaimSystem.SharedGetAreasInBounds(new RectangleInt(x, y, 1, 1), tempList, addGracePadding: false);

            var landClaimsModified = 0;
            foreach (var area in tempList.AsList())
            {
                var privateState = LandClaimArea.GetPrivateState(area);
                privateState.LandClaimFounder = newOwnerName;
                privateState.LandOwners.Clear();
                privateState.LandOwners.Add(newOwnerName);
                landClaimsModified++;
            }

            return $"Modified {landClaimsModified} land claims";
        }

        public string Execute(ushort x, ushort y, ICharacter newOwner)
        {
            var newFounderName = newOwner.Name;
            return ServerSetOwner(x, y, newFounderName);
        }

        public string Execute([CurrentCharacterIfNull] ICharacter nearCharacter, ICharacter newOwner)
        {
            return this.Execute(nearCharacter.TilePosition.X,
                                nearCharacter.TilePosition.Y,
                                newOwner);
        }
    }
}
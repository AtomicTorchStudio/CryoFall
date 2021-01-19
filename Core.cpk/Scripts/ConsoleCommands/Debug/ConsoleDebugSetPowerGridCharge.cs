// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.PowerGridSystem;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ConsoleDebugSetPowerGridCharge : BaseConsoleCommand
    {
        public override string Description =>
            "Modifies charge amount of the power grid for the power grid located near the player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "debug.setPowerGridCharge";

        public string Execute(
            double chargePercent = 100,
            [CurrentCharacterIfNull] ICharacter character = null)
        {
            var chargeFraction = MathHelper.Clamp(chargePercent / 100, min: 0, max: 1);

            using var tempLandClaims = Api.Shared.GetTempList<ILogicObject>();
            LandClaimSystem.SharedGetAreasInBounds(
                new RectangleInt(character.TilePosition, (1, 1)),
                tempLandClaims,
                addGracePadding: false);

            var landClaim = tempLandClaims.AsList().FirstOrDefault();
            if (landClaim is null)
            {
                return "No power grid exist near " + character.Name;
            }

            var landClaimAreasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(landClaim);
            var powerGrid = LandClaimAreasGroup.GetPrivateState(landClaimAreasGroup).PowerGrid;
            var powerGridState = PowerGrid.GetPublicState(powerGrid);
            powerGridState.ElectricityAmount = powerGridState.ElectricityCapacity * chargeFraction;

            return $"Charge amount of the power grid modified to {chargeFraction * 100}%";
        }
    }
}
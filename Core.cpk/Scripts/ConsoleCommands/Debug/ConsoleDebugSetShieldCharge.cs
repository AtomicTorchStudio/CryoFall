// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Debug
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaimShield;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ConsoleDebugSetShieldCharge : BaseConsoleCommand
    {
        public override string Description =>
            "Modifies charge amount of the S.H.I.E.L.D. (PvP) for the base located near the player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "debug.setBaseShieldCharge";

        public string Execute(
            double chargePercent = 100,
            [CurrentCharacterIfNull] ICharacter character = null)
        {
            if (!LandClaimShieldProtectionConstants.SharedIsEnabled)
            {
                return "S.H.I.E.L.D. protection is not available";
            }

            var chargeFraction = MathHelper.Clamp(chargePercent / 100, min: 0, max: 1);

            using var tempLandClaims = Api.Shared.GetTempList<ILogicObject>();
            LandClaimSystem.SharedGetAreasInBounds(
                new RectangleInt(character.TilePosition, (1, 1)),
                tempLandClaims,
                addGracePadding: false);

            var landClaim = tempLandClaims.AsList().FirstOrDefault();
            if (landClaim is null)
            {
                return "No base exist near " + character.Name;
            }

            var landClaimAreasGroup = LandClaimArea.GetPublicState(landClaim).LandClaimAreasGroup;
            LandClaimShieldProtectionSystem.SharedGetShieldProtectionMaxStatsForBase(landClaimAreasGroup,
                                                                                     out _,
                                                                                     out var electricityCapacity);

            var privateState = LandClaimAreasGroup.GetPrivateState(landClaimAreasGroup);
            privateState.ShieldProtectionCurrentChargeElectricity = electricityCapacity * chargeFraction;
            privateState.ShieldProtectionCooldownExpirationTime = 0; // reset the cooldown

            return $"Charge amount of the S.H.I.E.L.D. modified to {chargeFraction * 100}% and cooldown reset.";
        }
    }
}
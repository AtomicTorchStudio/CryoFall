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
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ConsoleDebugSetShieldStatus : BaseConsoleCommand
    {
        public override string Description =>
            "Modifies status of the S.H.I.E.L.D. protection (PvP) for the base located near the player.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "debug.setBaseShieldStatus";

        public string Execute(
            ShieldProtectionStatus status,
            [CurrentCharacterIfNull] ICharacter character = null)
        {
            if (!LandClaimShieldProtectionConstants.SharedIsEnabled)
            {
                return "S.H.I.E.L.D. protection is not available";
            }

            if (status == ShieldProtectionStatus.Active)
            {
                status = ShieldProtectionStatus.Activating;
            }

            using var tempAreas = Api.Shared.GetTempList<ILogicObject>();
            LandClaimSystem.SharedGetAreasInBounds(
                new RectangleInt(character.TilePosition, (1, 1)),
                tempAreas,
                addGracePadding: false);

            var area = tempAreas.AsList().FirstOrDefault();
            if (area is null)
            {
                return "No base exist near " + character.Name;
            }

            var areasGroup = LandClaimSystem.SharedGetLandClaimAreasGroup(area);
            LandClaimShieldProtectionSystem.SharedGetShieldProtectionMaxStatsForBase(areasGroup,
                out _,
                out _);

            var privateState = LandClaimAreasGroup.GetPrivateState(areasGroup);
            privateState.ShieldProtectionCooldownExpirationTime = 0;

            var publicState = LandClaimAreasGroup.GetPublicState(areasGroup);
            publicState.Status = status;
            publicState.ShieldActivationTime = Server.Game.FrameTime;

            return $"Status of the S.H.I.E.L.D. changed to {status}.";
        }
    }
}
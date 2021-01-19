namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// This class tracks hostility done by any player character to player bases.
    /// </summary>
    public static class ServerLandClaimAreasGroupHostileCharactersTracker
    {
        private const double HostileMarkCooldown = 18 * 60 * 60; // 18 hours

        private static readonly Dictionary<uint, Dictionary<ICharacter, double>>
            LandClaimAreasGroupHostilePlayerCharacters = new();

        public static bool IsHostileCharacter(
            ICharacter character,
            ILogicObject landClaimAreasGroup)
        {
            if (character is null)
            {
                return false;
            }

            if (character.IsNpc)
            {
                return true;
            }

            if (landClaimAreasGroup is null
                || PveSystem.ServerIsPvE)
            {
                return false;
            }

            return LandClaimAreasGroupHostilePlayerCharacters.TryGetValue(
                       landClaimAreasGroup.Id,
                       out var hostilePlayerCharacters)
                   && hostilePlayerCharacters.TryGetValue(character, out var lastHostileTime)
                   && (Api.Server.Game.FrameTime - lastHostileTime) < HostileMarkCooldown;
        }

        public static void OnHostility(ICharacter character, Vector2Ushort attackedTilePosition)
        {
            if (character is null
                || character.IsNpc
                || PveSystem.ServerIsPvE)
            {
                return;
            }

            OnHostility(character,
                        LandClaimSystem.SharedGetLandClaimAreasGroup(attackedTilePosition));
        }

        public static void OnHostility(ICharacter character, ILogicObject landClaimAreasGroup)
        {
            if (landClaimAreasGroup is null
                || character is null
                || character.IsNpc
                || PveSystem.ServerIsPvE)
            {
                return;
            }

            if (!LandClaimAreasGroupHostilePlayerCharacters.TryGetValue(
                    landClaimAreasGroup.Id,
                    out var hostilePlayerCharacters))
            {
                hostilePlayerCharacters = new Dictionary<ICharacter, double>();
                LandClaimAreasGroupHostilePlayerCharacters[landClaimAreasGroup.Id] = hostilePlayerCharacters;
            }

            hostilePlayerCharacters[character] = Api.Server.Game.FrameTime;
            //Api.Logger.Dev($"Hostility reported: {character} in {landClaimAreasGroup}");
        }

        private class Bootstrapper : BaseBootstrapper
        {
            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                LandClaimSystem.ServerRaidBlockStartedOrExtended += ServerRaidBlockStartedOrExtendedHandler;
            }

            private static void ServerRaidBlockStartedOrExtendedHandler(
                ILogicObject area,
                ICharacter raiderCharacter,
                bool isNewRaidBlock)
            {
                OnHostility(raiderCharacter, LandClaimSystem.SharedGetLandClaimAreasGroup(area));
            }
        }
    }
}
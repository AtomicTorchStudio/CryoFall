namespace AtomicTorch.CBND.CoreMod.Systems.CharacterDespawnSystem
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Helpers;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterDamageTrackingSystem;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// Player characters should be despawned on PvE servers after some delay.
    /// They could respawn after that.
    /// </summary>
    public class CharacterDespawnSystem : ProtoSystem<CharacterDespawnSystem>
    {
        public const double OfflineDurationToDespawnWhenInFreeLand = 60 * 60; // 1 hour

        public const double OfflineDurationToDespawnWhenInsideNotOwnedClaim = 10 * 60; // 10 minutes

        public static bool ClientIsDespawned => ClientCurrentCharacterHelper.PrivateState.IsDespawned;

        /// <summary>
        /// Moves the character to service area so a respawn will be required on login.
        /// There will be no penalties (such as loot drop or "weakened" status effect).
        /// </summary>
        public static void DespawnCharacter(ICharacter character)
        {
            var privateState = PlayerCharacter.GetPrivateState(character);
            if (privateState.IsDespawned)
            {
                return;
            }

            VehicleSystem.ServerCharacterExitCurrentVehicle(character, force: true);
            CharacterDamageTrackingSystem.ServerClearStats(character);

            ServerTeleportPlayerCharacterToServiceArea(character);

            privateState.LastDeathPosition = Vector2Ushort.Zero;
            privateState.LastDeathTime = null;
        }

        public static Vector2Ushort ServerGetServiceAreaPosition()
        {
            var worldBounds = Server.World.WorldBounds;

            // teleport to bottom right corner of the map
            var position = new Vector2Ushort((ushort)(worldBounds.Offset.X + worldBounds.Size.X - 1),
                                             (ushort)(worldBounds.Offset.Y + 1));
            return position;
        }

        /// <summary>
        /// Service area (aka "graveyard") is an area in the bottom right corner of the map.
        /// </summary>
        public static void ServerTeleportPlayerCharacterToServiceArea(ICharacter character)
        {
            if (character.IsNpc)
            {
                return;
            }

            // disable the visual scope so the player cannot see anyone and nobody could see the player
            Api.Server.Characters.SetViewScopeMode(character, isEnabled: false);

            var privateState = PlayerCharacter.GetPrivateState(character);
            if (privateState.IsDespawned)
            {
                // already despawned, only fix the position
                MoveToServiceArea();
                return;
            }

            privateState.IsDespawned = true;
            VehicleSystem.ServerCharacterExitCurrentVehicle(character, force: true);
            MoveToServiceArea();

            Api.Logger.Important("Character despawned", character);

            // recreate physics (as despawned character doesn't have any physics)
            character.ProtoCharacter.SharedCreatePhysics(character);

            void MoveToServiceArea()
            {
                var serviceAreaPosition = ServerGetServiceAreaPosition();
                if (character.TilePosition != serviceAreaPosition)
                {
                    Server.World.SetPosition(character, (Vector2D)serviceAreaPosition);
                }
            }
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                return;
            }

            if (!PveSystem.ServerIsPvE)
            {
                return;
            }

            if (SharedLocalServerHelper.IsLocalServer
                || Api.IsEditor)
            {
                // don't despawn offline players on the local server and in Editor
                return;
            }

            // setup timer (tick every frame)
            TriggerEveryFrame.ServerRegister(
                callback: ServerGlobalUpdate,
                name: "System." + this.ShortId);
        }

        private static void ServerGlobalUpdate()
        {
            var serverTime = Server.Game.FrameTime;
            // perform update once per 10 seconds per player
            const double spreadDeltaTime = 10;

            using var tempListPlayers = Api.Shared.GetTempList<ICharacter>();
            PlayerCharacter.Instance
                           .EnumerateGameObjectsWithSpread(tempListPlayers.AsList(),
                                                           spreadDeltaTime: spreadDeltaTime,
                                                           Server.Game.FrameNumber,
                                                           Server.Game.FrameRate);

            foreach (var character in tempListPlayers.AsList())
            {
                if (ServerIsNeedDespawn(character, serverTime))
                {
                    DespawnCharacter(character);
                }
            }
        }

        private static bool ServerIsCharacterInsideOwnedBase(ICharacter character, out bool isInsideNotOwnedBase)
        {
            var currentBase = LandClaimSystem.SharedGetLandClaimAreasGroup(character.TilePosition);
            if (currentBase is null)
            {
                isInsideNotOwnedBase = false;
                return false;
            }

            foreach (var area in LandClaimAreasGroup.GetPrivateState(currentBase)
                                                    .ServerLandClaimsAreas)
            {
                if (LandClaimSystem.SharedIsOwnedArea(area, character, requireFactionPermission: false))
                {
                    isInsideNotOwnedBase = false;
                    return true;
                }
            }

            isInsideNotOwnedBase = true;
            return false;
        }

        private static bool ServerIsNeedDespawn(ICharacter character, double serverTime)
        {
            if (character.ServerIsOnline)
            {
                return false;
            }

            var privateState = PlayerCharacter.GetPrivateState(character);
            if (privateState.IsDespawned)
            {
                return false;
            }

            if (character.ProtoCharacter is PlayerCharacterSpectator
                || Server.Characters.IsSpectator(character))
            {
                return false;
            }

            if (ServerIsCharacterInsideOwnedBase(character, out var isInsideNotOwnedBase))
            {
                return false;
            }

            var durationToDespawn = isInsideNotOwnedBase
                                        ? OfflineDurationToDespawnWhenInsideNotOwnedClaim
                                        : OfflineDurationToDespawnWhenInFreeLand;

            if (serverTime < privateState.ServerLastActiveTime + durationToDespawn)
            {
                // despawn timeout is not reached yet
                return false;
            }

            return true;
        }
    }
}
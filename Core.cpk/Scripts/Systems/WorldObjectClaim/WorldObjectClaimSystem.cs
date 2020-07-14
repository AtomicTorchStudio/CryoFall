namespace AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim
{
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    /// <summary>
    /// This is a PvE-only system to claim (or tag/reserve) world objects.
    /// Due to open world game nature, sometimes it's important to ensure that certain resources
    /// are temporarily belong to the first player that found it.
    /// E.g. if one players started mining a mineral in PvE, it's claimed for this player temporarily.
    /// </summary>
    public class WorldObjectClaimSystem : ProtoSystem<WorldObjectClaimSystem>
    {
        private static readonly TextureResource TextureResourceClaimedObject
            = IsClient
                  ? new TextureResource("FX/Special/IconClaimedObject.png")
                  : null;

        // this system is available only in PvE
        public static bool SharedIsEnabled => PveSystem.SharedIsPve(false);

        [NotLocalizable]
        public override string Name => "World object claim system";

        public static ICharacter ServerGetCharacterByClaim(ILogicObject objectClaim)
        {
            if (!SharedIsEnabled)
            {
                return null;
            }

            if (objectClaim is null)
            {
                return null;
            }

            var playerCharacterId = WorldObjectClaim.GetPublicState(objectClaim).PlayerCharacterId;
            return Server.World.GetGameObjectById<ICharacter>(GameObjectType.Character, playerCharacterId);
        }

        public static void ServerRemoveClaim(IWorldObject worldObject)
        {
            if (!SharedIsEnabled)
            {
                return;
            }

            var worldObjectPublicState = worldObject?.AbstractPublicState as IWorldObjectPublicStateWithClaim;
            if (worldObjectPublicState?.WorldObjectClaim != null)
            {
                // please note - the property will clean during the ServerDestroy method of WorldObjectClaim
                Server.World.DestroyObject(worldObjectPublicState.WorldObjectClaim);
            }
        }

        public static void ServerTryClaim(
            IWorldObject worldObject,
            ICharacter character,
            double durationSeconds)
        {
            if (!SharedIsEnabled)
            {
                return;
            }

            if (worldObject is null
                || character is null
                || character.IsNpc
                || durationSeconds <= 0)
            {
                return;
            }

            var worldObjectPublicState = worldObject.AbstractPublicState as IWorldObjectPublicStateWithClaim;
            if (worldObjectPublicState is null)
            {
                Logger.Warning(
                    $"{worldObject} doesn't implement {nameof(IWorldObjectPublicStateWithClaim)} - cannot claim it");
                return;
            }

            var objectClaim = worldObjectPublicState.WorldObjectClaim;
            if (objectClaim != null)
            {
                // already claimed
                WorldObjectClaim.ServerTryExtendClaim(objectClaim, character, durationSeconds);
                return;
            }

            objectClaim = Server.World.CreateLogicObject<WorldObjectClaim>();
            WorldObjectClaim.ServerSetupClaim(objectClaim, character, worldObject, durationSeconds);
            worldObjectPublicState.WorldObjectClaim = objectClaim;
            //Logger.Dev("World object claim added: " + worldObject);
        }

        public static bool SharedIsAllowInteraction(
            ICharacter character,
            IStaticWorldObject worldObject,
            bool showClientNotification)
        {
            if (!SharedIsEnabled)
            {
                return true;
            }

            if (character is null
                || !PveSystem.SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false))
            {
                return true;
            }

            var worldObjectPublicState = worldObject.AbstractPublicState as IWorldObjectPublicStateWithClaim;
            if (worldObjectPublicState?.WorldObjectClaim is null
                || WorldObjectClaim.SharedIsClaimedForPlayer(worldObjectPublicState.WorldObjectClaim, character))
            {
                return true;
            }

            if (IsClient && showClientNotification)
            {
                NotificationSystem.ClientShowNotification(
                    CoreStrings.WorldObjectClaim_Title,
                    CoreStrings.WorldObjectClaim_Description
                    + "[br]"
                    + CoreStrings.WorldObjectClaim_Description2,
                    NotificationColor.Bad,
                    icon: TextureResourceClaimedObject);
            }

            return false;
        }
    }
}
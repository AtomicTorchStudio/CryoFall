namespace AtomicTorch.CBND.CoreMod.Systems.WorldObjectClaim
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Party;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.WorldObjectClaim;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class WorldObjectClaim
        : ProtoGameObject<ILogicObject,
              EmptyPrivateState,
              WorldObjectClaim.PublicState,
              WorldObjectClaim.ClientState>,
          IProtoLogicObject
    {
        private const bool DisplayLockEvenForCurrentPlayer = false;

        public override double ClientUpdateIntervalSeconds => 0.1;

        [NotLocalizable]
        public override string Name => "World object claim";

        public override double ServerUpdateIntervalSeconds => 1;

        public static void ServerSetupClaim(
            ILogicObject objectClaim,
            ICharacter character,
            IWorldObject worldObject,
            double durationSeconds,
            bool claimForPartyMembers)
        {
            var publicState = GetPublicState(objectClaim);
            publicState.PlayerCharacterId = character.Id;
            publicState.PlayerPartyId = claimForPartyMembers
                                            ? PartySystem.ServerGetParty(character)?.Id ?? 0
                                            : 0;
            publicState.ExpirationTime = Server.Game.FrameTime + durationSeconds;
            publicState.WorldObject = worldObject;
        }

        public static bool ServerTryExtendClaim(
            ILogicObject objectClaim,
            ICharacter character,
            in double durationSeconds)
        {
            if (!SharedIsClaimedForPlayer(objectClaim, character))
            {
                return false;
            }

            var publicState = GetPublicState(objectClaim);
            publicState.ExpirationTime = Math.Max(publicState.ExpirationTime,
                                                  Server.Game.FrameTime + durationSeconds);
            return true;
        }

        public static bool SharedIsClaimedForPlayer(ILogicObject objectClaim, ICharacter character)
        {
            var publicState = GetPublicState(objectClaim);
            if (publicState.PlayerCharacterId == character.Id)
            {
                return true;
            }

            // perform the same party check to allow players from the same party access to it
            var claimPartyId = publicState.PlayerPartyId;
            if (claimPartyId == 0)
            {
                return false;
            }

            var worldObject = publicState.WorldObject;
            if (worldObject is null)
            {
                // should not be possible
                return true;
            }

            if (IsServer)
            {
                return claimPartyId == PartySystem.ServerGetParty(character)?.Id;
            }

            // client does know only a party ID for the current character and its party members
            return (character.IsCurrentClientCharacter
                    || PartySystem.ClientIsPartyMember(character.Name))
                   && claimPartyId == PartySystem.ClientCurrentParty?.Id;
        }

        public override void ClientDeinitialize(ILogicObject gameObject)
        {
            base.ClientDeinitialize(gameObject);

            GetClientState(gameObject).ComponentAttachedControl?.Destroy();
        }

        public override void ServerOnDestroy(ILogicObject gameObject)
        {
            var publicState = GetPublicState(gameObject);

            var worldObject = publicState.WorldObject;
            if (worldObject is null
                || worldObject.IsDestroyed)
            {
                return;
            }

            publicState.WorldObject = null;

            var worldObjectPublicState = worldObject.GetPublicState<IWorldObjectPublicStateWithClaim>();
            if (ReferenceEquals(worldObjectPublicState.WorldObjectClaim, gameObject))
            {
                // remove the current claim
                worldObjectPublicState.WorldObjectClaim = null;
            }
        }

        protected override void ClientUpdate(ClientUpdateData data)
        {
            var clientState = data.ClientState;
            var shouldDisplay = ClientShouldDisplayIndicator(data.GameObject);
            var isDisplaying = clientState.ComponentAttachedControl?.IsEnabled ?? false;

            if (isDisplaying == shouldDisplay)
            {
                return;
            }

            if (shouldDisplay)
            {
                clientState.ComponentAttachedControl
                    = WorldObjectClaimIndicator.AttachTo(data.PublicState.WorldObject);
            }
            else
            {
                clientState.ComponentAttachedControl?.Destroy();
                clientState.ComponentAttachedControl = null;
            }
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            if (!data.IsFirstTimeInit)
            {
                Server.World.DestroyObject(data.GameObject);
            }
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var publicState = data.PublicState;
            var worldObject = publicState.WorldObject;
            if (worldObject.IsDestroyed)
            {
                // the claim is no longer valid
                Server.World.DestroyObject(data.GameObject);
                return;
            }

            if (Server.Game.FrameTime > publicState.ExpirationTime)
            {
                //Logger.Dev("World object claim expired: " + worldObject);
                WorldObjectClaimSystem.ServerRemoveClaim(worldObject);
            }
        }

        private static bool ClientShouldDisplayIndicator(ILogicObject objectClaim)
        {
            var publicState = GetPublicState(objectClaim);
            if (publicState.WorldObject is null
                || !publicState.WorldObject.IsInitialized)
            {
                return false;
            }

            return DisplayLockEvenForCurrentPlayer
                   || !SharedIsClaimedForPlayer(objectClaim,
                                                ClientCurrentCharacterHelper.Character);
        }

        public class ClientState : BaseClientState
        {
            public IComponentAttachedControl ComponentAttachedControl { get; set; }
        }

        public class PublicState : BasePublicState
        {
            [SyncToClient(DeliveryMode.ReliableSequenced, maxUpdatesPerSecond: 1)]
            public double ExpirationTime { get; set; }

            [SyncToClient]
            public uint PlayerCharacterId { get; set; }

            [SyncToClient]
            public uint PlayerPartyId { get; set; }

            [SyncToClient]
            public IWorldObject WorldObject { get; set; }
        }
    }
}
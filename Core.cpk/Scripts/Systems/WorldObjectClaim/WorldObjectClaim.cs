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

        public override double ClientUpdateIntervalSeconds => 0;

        [NotLocalizable]
        public override string Name => "World object claim";

        public override double ServerUpdateIntervalSeconds => 1;

        public static void ServerSetupClaim(
            ILogicObject objectClaim,
            ICharacter character,
            IWorldObject worldObject,
            double durationSeconds)
        {
            var publicState = GetPublicState(objectClaim);
            publicState.PlayerCharacterId = character.Id;
            publicState.PlayerPartyId = PartySystem.ServerGetParty(character)?.Id ?? 0;
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

            var claimPartyId = publicState.PlayerPartyId;
            if (claimPartyId == 0)
            {
                return false;
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

        protected override void ClientInitialize(ClientInitializeData data)
        {
            var objectClaim = data.GameObject;
            var publicState = data.PublicState;
            var worldObject = publicState.WorldObject;

            if (worldObject is null)
            {
                // reinitialize when the claimed world object will arrive
                publicState.ClientSubscribe(_ => _.WorldObject,
                                            () => objectClaim.ClientInitialize(),
                                            data.ClientState);
                return;
            }

            if (!worldObject.IsInitialized)
            {
                // force reinitialization a bit later
                ClientTimersSystem.AddAction(delaySeconds: 0.1,
                                             () => objectClaim.ClientInitialize());
                return;
            }

            if (DisplayLockEvenForCurrentPlayer
                || !SharedIsClaimedForPlayer(objectClaim,
                                             ClientCurrentCharacterHelper.Character))
            {
                data.ClientState.ComponentAttachedControl =
                    WorldObjectClaimIndicator.AttachTo(objectClaim, worldObject);
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

        public class ClientState : BaseClientState
        {
            public IComponentAttachedControl ComponentAttachedControl { get; set; }
        }

        public class PublicState : BasePublicState
        {
            [SyncToClient(DeliveryMode.ReliableSequenced, maxUpdatesPerSecond: 1)]
            [TempOnly]
            public double ExpirationTime { get; set; }

            [SyncToClient]
            [TempOnly]
            public uint PlayerCharacterId { get; set; }

            [SyncToClient]
            [TempOnly]
            public uint PlayerPartyId { get; set; }

            [SyncToClient]
            [TempOnly]
            public IWorldObject WorldObject { get; set; }
        }
    }
}
﻿namespace AtomicTorch.CBND.CoreMod.Characters.Player
{
    using System;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.Items.Implants;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleSystem;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;

    [RemoteAuthorizeServerOperator]
    public class PlayerCharacterSpectator : PlayerCharacter
    {
        public override float CharacterWorldHeight => 0;

        public override string Name => "Player character in Spectator mode";

        public override double PhysicsBodyAccelerationCoef => 14;

        public override double PhysicsBodyFriction => 84;

        public override double StatMoveSpeed => 6;

        public override double StatMoveSpeedRunMultiplier => 2.333;

        public override double StatRunningStaminaConsumptionUsePerSecond => 0;

        public static void ServerSwitchToPlayerMode(ICharacter character)
        {
            if (character.ProtoCharacter.GetType() == typeof(PlayerCharacter))
            {
                return;
            }

            if (character.ProtoCharacter.GetType() != typeof(PlayerCharacterSpectator))
            {
                throw new Exception("Incorrect proto");
            }

            // the order of calls is important here
            Server.Characters.SetSpectatorMode(character, isSpectator: false, reinitialize: false);
            Server.Characters.SetProto(character, GetProtoEntity<PlayerCharacter>());
        }

        public static void ServerSwitchToSpectatorMode(ICharacter character)
        {
            if (character.ProtoCharacter.GetType() == typeof(PlayerCharacterSpectator))
            {
                return;
            }

            VehicleSystem.ServerCharacterExitCurrentVehicle(character, force: true);

            // restore stamina so the spectator can "run"
            var stats = character.GetPublicState<PlayerCharacterPublicState>()
                                 .CurrentStatsExtended;
            stats.SharedSetStaminaCurrent(stats.StaminaMax);

            // the order of calls is important here
            Server.Characters.SetSpectatorMode(character, isSpectator: false, reinitialize: false);
            Server.Characters.SetProto(character, GetProtoEntity<PlayerCharacterSpectator>());
            character.ServerRemoveAllStatusEffects();
        }

        public static bool SharedIsSpectator(ICharacter character) 
            => character.ProtoCharacter is PlayerCharacterSpectator;

        public override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IWorldObject targetObject,
            double damagePreMultiplier,
            double damagePostMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            // doesn't receive any damage
            obstacleBlockDamageCoef = 0;
            damageApplied = 0;
            return false;
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            var character = data.GameObject;
            if (!character.IsCurrentClientCharacter)
            {
                return;
            }

            // use artificial retina effect for current spectator character
            // to help with the night-time vision
            character.ClientSceneObject
                     .AddComponent<ItemImplantArtificialRetina.ClientComponentArtificialRetinaEffect>();

            base.ClientInitialize(data);
        }

        protected override void ClientUpdate(ClientUpdateData data)
        {
            var character = data.GameObject;
            if (!character.IsCurrentClientCharacter)
            {
                return;
            }

            var privateState = data.PrivateState;
            var publicState = data.PublicState;

            this.SharedRebuildFinalCacheIfNeeded(privateState, publicState);
            this.SharedApplyInput(character, privateState, publicState);
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            var character = data.GameObject;
            var publicState = data.PublicState;
            var privateState = data.PrivateState;

            publicState.IsOnline = false; // spectators are always offline

            this.SharedRebuildFinalCacheIfNeeded(privateState, publicState);
            this.SharedApplyInput(character, privateState, publicState);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody.Reset();

            if (IsServer
                || data.GameObject.IsCurrentClientCharacter)
            {
                data.PhysicsBody.AddShapeCircle(
                    radius: 10,
                    group: CollisionGroups.CharacterInteractionArea);
            }
        }

        protected override void SharedGetSkeletonProto(
            ICharacter character,
            out ProtoCharacterSkeleton protoSkeleton,
            ref double scale)
        {
            protoSkeleton = GetProtoEntity<SkeletonSpectator>();
            scale = 0;
        }
    }
}
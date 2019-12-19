namespace AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Bushes
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Server;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectBush
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectGatherableVegetation
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectBush
        where TPrivateState : ObjectBushPrivateState, new()
        where TPublicState : ObjectBushPublicState, new()
        where TClientState : ObjectBushClientState, new()
    {
        public const string ErrorNoBerries = "No berries!";

        public override bool IsAutoDestroyOnGather => false;

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Vegetation;

        public override double ObstacleBlockDamageCoef => 0.25;

        public override float StructurePointsMax => 100;

        public float TimeToGiveHarvestTotalSeconds { get; private set; }

        protected virtual string InteractionFailedNoFruitsMessage => ErrorNoBerries;

        protected abstract TimeSpan TimeToGiveHarvest { get; }

        public override byte ClientGetTextureAtlasColumn(
            IStaticWorldObject worldObject,
            VegetationPublicState statePublic)
        {
            var bushPublicState = (ObjectBushPublicState)statePublic;
            if (bushPublicState.HasHarvest)
            {
                // return last column in texture
                return (byte)(this.GrowthStagesCount + 1);
            }

            return base.ClientGetTextureAtlasColumn(worldObject, statePublic);
        }

        public override bool SharedCanInteract(ICharacter character, IStaticWorldObject worldObject, bool writeToLog)
        {
            if (!base.SharedCanInteract(character, worldObject, writeToLog))
            {
                return false;
            }

            // can interact only when has harvest
            var serverState = GetPublicState(worldObject);
            if (serverState.HasHarvest)
            {
                return true;
            }

            if (writeToLog)
            {
                Logger.Warning(
                    $"Character cannot interact with {worldObject} - there are other objects on the way.",
                    character);

                if (IsClient)
                {
                    ClientOnCannotInteract(worldObject, this.InteractionFailedNoFruitsMessage);
                }
            }

            return false;
        }

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return (0.5, 0.2);
        }

        public override bool SharedIsCanGather(IStaticWorldObject staticWorldObject)
        {
            return GetPublicState(staticWorldObject).HasHarvest;
        }

        public override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            double damagePreMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            if (!base.SharedOnDamage(
                    weaponCache,
                    targetObject,
                    damagePreMultiplier,
                    out obstacleBlockDamageCoef,
                    out damageApplied))
            {
                // not hit
                return false;
            }

            obstacleBlockDamageCoef = this.ObstacleBlockDamageCoef;

            if (IsServer)
            {
                // bush was hit
                // remove harvest and reset harvest growing progress
                this.ServerResetHarvestGrownState(targetObject);
            }

            return true;
        }

        protected override byte CalculateGrowthStagesCount()
        {
            var result = base.CalculateGrowthStagesCount() - 1;
            if (result <= 0)
            {
                return 0;
            }

            return (byte)result;
        }

        protected override void ClientAddShadowRenderer(ClientInitializeData data)
        {
            base.ClientAddShadowRenderer(data);
            data.ClientState.RendererShadow.PositionOffset = (0.5, 0.2);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.25;
        }

        protected override void ClientUpdate(ClientUpdateData data)
        {
            var clientState = data.ClientState;
            var publicState = data.PublicState;
            if (clientState.LastGrowthStage == publicState.GrowthStage
                && clientState.LastHasHarvest == publicState.HasHarvest)
            {
                // no need to refresh
                return;
            }

            SystemVegetation.ClientRefreshVegetationRendering(this, data.GameObject, clientState, publicState);
            clientState.LastHasHarvest = publicState.HasHarvest;
        }

        protected override void PrepareDroplistOnDestroy(DropItemsList droplist)
        {
            // default droplist for all bushes that don't have it specified
            droplist.Add<ItemTwigs>(count: 1, countRandom: 2);
        }

        protected override void PrepareProtoGatherableVegetation()
        {
            this.TimeToGiveHarvestTotalSeconds = (float)this.TimeToGiveHarvest.TotalSeconds;
        }

        protected override bool ServerTryGatherByCharacter(ICharacter who, IStaticWorldObject vegetationObject)
        {
            if (!base.ServerTryGatherByCharacter(who, vegetationObject))
            {
                return false;
            }

            this.ServerResetHarvestGrownState(vegetationObject);
            return true;
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            // base server update (from ProtoVegetation) will perform vegetation growing
            base.ServerUpdate(data);

            var publicState = data.PublicState;
            if (publicState.HasHarvest)
            {
                return;
            }

            if (publicState.GrowthStage < this.GrowthStagesCount)
            {
                // not full grown - cannot grow harvest
                return;
            }

            // full grown and need to grow harvest
            var privateState = data.PrivateState;
            if (Server.Game.FrameTime >= privateState.ServerTimeProduceHarvest)
            {
                publicState.HasHarvest = true;
            }
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(
                    radius: 0.35,
                    center: (0.5, 0.45))
                .AddShapeRectangle(
                    size: (0.8, 0.7),
                    offset: (0.1, 0.1),
                    group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(
                    size: (0.9, 0.8),
                    offset: (0.05, 0.05),
                    group: CollisionGroups.HitboxRanged)
                .AddShapeCircle(
                    radius: 0.4,
                    center: (0.5, 0.5),
                    group: CollisionGroups.ClickArea);
        }

        private void ServerResetHarvestGrownState(IStaticWorldObject worldObject)
        {
            var publicState = GetPublicState(worldObject);
            var privateState = GetPrivateState(worldObject);

            publicState.HasHarvest = false;
            privateState.ServerTimeProduceHarvest = Server.Game.FrameTime
                                                    + ServerSpawnRateScaleHelper.AdjustDurationByRate(
                                                        this.TimeToGiveHarvestTotalSeconds);
        }
    }

    public abstract class ProtoObjectBush
        : ProtoObjectBush<
            ObjectBushPrivateState,
            ObjectBushPublicState,
            ObjectBushClientState>
    {
    }
}
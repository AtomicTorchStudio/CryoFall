namespace AtomicTorch.CBND.CoreMod.StaticObjects.Deposits
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Special;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.Systems.WorldMapResourceMarks;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public abstract class ProtoObjectDeposit
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoStaticWorldObject
          <TPrivateState,
              TPublicState,
              TClientState>, IProtoObjectDeposit
        where TPrivateState : ObjectDepositPrivateState, new()
        where TPublicState : StaticObjectPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        public const string NotificationNoDamageToDepositUnderCooldown_Description
            = "The resource deposit has only just appeared and cannot be claimed or destroyed yet.";

        public const string NotificationNoDamageToDepositUnderCooldown_TitleFormat
            = "No damage dealt to {0}";

        public abstract double DecaySpeedMultiplierWhenExtractingActive { get; }

        public override string InteractionTooltipText => null; // non-interactive

        // we don't want to see any decals under it
        public override StaticObjectKind Kind => StaticObjectKind.Structure;

        /// <summary>
        /// Total lifetime of the deposit.
        /// </summary>
        public abstract double LifetimeTotalDurationSeconds { get; }

        public sealed override double ObstacleBlockDamageCoef => 0;

        public sealed override double ServerUpdateIntervalSeconds => this.ServerDecayIntervalSeconds;

        /// <summary>
        /// How often this deposit will attempt to decay.
        /// </summary>
        protected virtual int ServerDecayIntervalSeconds => 60; // every minute

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            base.ServerOnDestroy(gameObject);
            WorldMapResourceMarksSystem.ServerRemoveMark(gameObject);
        }

        public virtual void ServerOnExtractorDestroyedForDeposit(IStaticWorldObject objectDeposit)
        {
            if (!objectDeposit.IsDestroyed)
            {
                this.ServerOnStaticObjectZeroStructurePoints(null, null, objectDeposit);
            }
        }

        public override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            double damagePreMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            if (weaponCache.ProtoExplosive != null)
            {
                // allow to explode only a resource deposit which could be claimed
                if (WorldMapResourceMarksSystem.SharedCalculateTimeRemainsToClaimCooldownSeconds(targetObject)
                    <= 0)
                {
                    // accept explosive damage
                    return base.SharedOnDamage(weaponCache,
                                               targetObject,
                                               damagePreMultiplier,
                                               out obstacleBlockDamageCoef,
                                               out damageApplied);
                }

                using var tempScopedBy = Api.Shared.GetTempList<ICharacter>();
                Server.World.GetScopedByPlayers(targetObject, tempScopedBy);
                this.CallClient(tempScopedBy.AsList(),
                                _ => _.ClientRemote_NoDamageToDepositUnderCooldown(weaponCache.ProtoExplosive));
            }

            // only damage from explosives is accepted
            obstacleBlockDamageCoef = 0;
            damageApplied = 0; // no damage
            return false;      // no hit
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            var worldObject = data.GameObject;
            var clientState = data.ClientState;

            this.ClientSetupSubstrateRenderer(data);

            clientState.Renderer = Client.Rendering.CreateSpriteRenderer(
                worldObject,
                this.DefaultTexture,
                drawOrder: DrawOrder.Floor);

            clientState.Renderer.SortByWorldPosition = false;

            this.ClientSetupRenderer(clientState.Renderer);

            if (data.GameObject.OccupiedTile.StaticObjects.Any(
                o => o.ProtoStaticWorldObject is IProtoObjectExtractor
                     && !o.IsDestroyed))
            {
                // there are other static objects so don't create structure points bar and hide the renderer
                clientState.Renderer.IsEnabled = false;
            }
            else
            {
                this.ClientAddAutoStructurePointsBar(data);
            }
        }

        protected override void ClientObserving(ClientObjectData data, bool isObserving)
        {
            base.ClientObserving(data, isObserving);

            if (isObserving)
            {
                // display tooltip only if this is the only object in tile
                isObserving = data.GameObject.OccupiedTile.StaticObjects.All(o => o.ProtoStaticWorldObject == this);
            }

            ClientObjectDepositTooltipHelper.Refresh(data.GameObject, isObserving);
        }

        protected virtual void ClientSetupSubstrateRenderer(ClientInitializeData data)
        {
            var renderer = Client.Rendering.CreateSpriteRenderer(
                data.GameObject,
                new TextureResource("StaticObjects/Deposits/ObjectDepositSubstrate"),
                drawOrder: DrawOrder.GroundDecalsOver,
                positionOffset: this.Layout.Center,
                spritePivotPoint: (0.5, 0.5));

            renderer.SortByWorldPosition = false;
        }

        protected override void PrepareTileRequirements(ConstructionTileRequirements tileRequirements)
        {
            base.PrepareTileRequirements(tileRequirements);
            tileRequirements.Add(ConstructionTileRequirements.ValidatorNotRestrictedAreaEvenForServer);
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            var privateState = data.PrivateState;
            if (data.IsFirstTimeInit)
            {
                privateState.ServerSpawnTime = this.LifetimeTotalDurationSeconds > 0
                                                   ? Server.Game.FrameTime
                                                   : 0;
            }

            WorldMapResourceMarksSystem.ServerAddMark(data.GameObject,
                                                      privateState.ServerSpawnTime);
        }

        protected virtual void ServerOnDecayCompleted(IStaticWorldObject worldObject)
        {
            this.ServerOnStaticObjectZeroStructurePoints(null, null, worldObject);
        }

        protected override void ServerOnStaticObjectDestroyedByCharacter(
            ICharacter byCharacter,
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject)
        {
            base.ServerOnStaticObjectDestroyedByCharacter(byCharacter, weaponCache, targetObject);
            this.ServerOnExtractorDestroyedForDeposit(targetObject);
        }

        protected override void ServerOnStaticObjectZeroStructurePoints(
            WeaponFinalCache weaponCache,
            ICharacter byCharacter,
            IWorldObject targetObject)
        {
            Server.World.CreateStaticWorldObject<ObjectDepletedDeposit>(targetObject.TilePosition);

            base.ServerOnStaticObjectZeroStructurePoints(weaponCache, byCharacter, targetObject);
        }

        protected virtual void ServerTryDecay(
            double dataDeltaTime,
            IStaticWorldObject worldObject,
            TPublicState publicState)
        {
            var lifetimeTotalDurationSeconds = this.LifetimeTotalDurationSeconds;
            if (lifetimeTotalDurationSeconds <= 0)
            {
                // no decay
                return;
            }

            var damage = this.StructurePointsMax
                         * dataDeltaTime
                         / lifetimeTotalDurationSeconds;

            if (this.DecaySpeedMultiplierWhenExtractingActive > 1)
            {
                // find an extractor object built on top of this deposit
                // (currently all the extractors are inheriting IProtoObjectManufacturer)
                var objectExtractor = worldObject.OccupiedTile.StaticObjects.FirstOrDefault(
                    o => o.ProtoStaticWorldObject is IProtoObjectManufacturer);

                if (objectExtractor != null
                    && objectExtractor.GetPublicState<ObjectManufacturerPublicState>().IsActive)
                {
                    // decay faster as the extraction/manufacturing is going on
                    damage *= this.DecaySpeedMultiplierWhenExtractingActive;
                }
            }

            var newStructurePoints = (float)(publicState.StructurePointsCurrent - damage);
            if (newStructurePoints > 0)
            {
                publicState.StructurePointsCurrent = newStructurePoints;
            }
            else
            {
                // decayed completely - destroy
                publicState.StructurePointsCurrent = newStructurePoints = 0;
                this.ServerOnDecayCompleted(worldObject);
            }
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            // please note this update is called rarely (defined by ServerUpdateIntervalSeconds)
            this.ServerTryDecay(data.DeltaTime,
                                data.GameObject,
                                data.PublicState);
        }

        protected override bool SharedIsAllowedObjectToInteractThrough(IWorldObject worldObject)
        {
            return true;
        }

        private void ClientRemote_NoDamageToDepositUnderCooldown(IProtoGameObject protoObjectExplosive)
        {
            var icon = protoObjectExplosive switch
            {
                IProtoObjectExplosive objectExplosive => objectExplosive.Icon,
                IProtoItem item                       => item.Icon,
                _                                     => null
            };

            NotificationSystem.ClientShowNotification(
                string.Format(NotificationNoDamageToDepositUnderCooldown_TitleFormat, this.Name),
                NotificationNoDamageToDepositUnderCooldown_Description,
                color: NotificationColor.Bad,
                icon);
        }
    }

    public abstract class ProtoObjectDeposit
        : ProtoObjectDeposit
            <ObjectDepositPrivateState,
                StaticObjectPublicState,
                StaticObjectClientState>
    {
    }
}
namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls
{
    using AtomicTorch.CBND.CoreMod.Helpers.Client.Walls;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Doors;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectWall
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectStructure
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectWall
        where TPrivateState : StructurePrivateState, new()
        where TPublicState : StaticObjectPublicState, new()
        where TClientState : ObjectWallClientState, new()
    {
        // do not update walls on the server
        public override double ServerUpdateIntervalSeconds => double.MaxValue;

        public override float StructurePointsMaxForConstructionSite
            => this.StructurePointsMax / 25;

        public TextureAtlasResource TextureAtlasDestroyed { get; private set; }

        public TextureAtlasResource TextureAtlasPrimary { get; private set; }

        public virtual string TextureAtlasPrimaryPath => this.GenerateTexturePath();

        public void ClientRefreshRenderer(IStaticWorldObject worldObject)
        {
            ProtoObjectWallHelper.ClientRefreshRenderer(worldObject);
        }

        public override void ClientSetupBlueprint(
            Tile tile,
            IClientBlueprint blueprint)
        {
            ProtoObjectWallHelper.ClientSetupBlueprint(this, tile, blueprint);
        }

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            base.ServerOnDestroy(gameObject);

            SharedWallConstructionRefreshHelper.SharedRefreshNeighborObjects(gameObject.OccupiedTile,
                                                                             isDestroy: true);
        }

        protected override ITextureResource ClientCreateIcon()
        {
            using (var result = WallTextureChunkSelector.GetRegion(NeighborsPattern.None, NeighborsPattern.None))
            {
                return this.TextureAtlasPrimary.Chunk((byte)result.Primary.AtlasChunkPosition.X,
                                                      (byte)result.Primary.AtlasChunkPosition.Y);
            }
        }

        protected override void ClientDeinitializeStructure(IStaticWorldObject gameObject)
        {
            base.ClientDeinitializeStructure(gameObject);
            SharedWallConstructionRefreshHelper.SharedRefreshNeighborObjects(gameObject.OccupiedTile,
                                                                             isDestroy: true);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            // don't use base implementation
            //base.ClientInitialize(data);

            this.ClientAddAutoStructurePointsBar(data);

            var worldObject = data.GameObject;
            var clientState = GetClientState(worldObject);

            // create renderers
            clientState.Renderer = Client.Rendering.CreateSpriteRenderer(worldObject);

            this.ClientRefreshRenderer(worldObject);

            SharedWallConstructionRefreshHelper.SharedRefreshNeighborObjects(worldObject.OccupiedTile,
                                                                             isDestroy: false);

            // ensure that the destroyed texture atlas is preloaded - in case the wall is destroyed we need this texture ASAP
            Client.Rendering.PreloadTextureAsync(this.TextureAtlasDestroyed);
        }

        protected override void ClientObserving(ClientObjectData data, bool isObserving)
        {
            base.ClientObserving(data, isObserving);
            StructureLandClaimIndicatorManager.ClientObserving(data.GameObject, isObserving);
        }

        protected sealed override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();

            var texturePrimaryAtlasPath = this.TextureAtlasPrimaryPath;
            this.TextureAtlasPrimary = new TextureAtlasResource(
                texturePrimaryAtlasPath,
                columns: 7,
                rows: 3,
                isTransparent: true);

            this.TextureAtlasDestroyed = new TextureAtlasResource(
                texturePrimaryAtlasPath + "Destroyed",
                columns: 7,
                rows: 3,
                isTransparent: true);

            this.PrepareProtoWall();
        }

        protected virtual void PrepareProtoWall()
        {
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            if (data.IsFirstTimeInit)
            {
                // just constructed - refresh nearby door types (horizontal/vertical)
                DoorHelper.RefreshNeighborDoorType(data.GameObject.OccupiedTile);
            }

            SharedWallConstructionRefreshHelper.SharedRefreshNeighborObjects(
                data.GameObject.OccupiedTile,
                isDestroy: false);
        }

        protected override void ServerOnStaticObjectZeroStructurePoints(
            WeaponFinalCache weaponCache,
            ICharacter byCharacter,
            IWorldObject targetObject)
        {
            var tilePosition = targetObject.TilePosition;
            base.ServerOnStaticObjectZeroStructurePoints(weaponCache, byCharacter, targetObject);

            if (weaponCache != null)
            {
                // wall was destroyed (and not deconstructed by a crowbar or any other means)
                ObjectWallDestroyed.ServerSpawnDestroyedWall(tilePosition, this);
                LandClaimSystem.ServerOnRaid(((IStaticWorldObject)targetObject).Bounds,
                                             byCharacter);
            }
        }

        protected override double SharedCalculateDamageByWeapon(
            WeaponFinalCache weaponCache,
            double damagePreMultiplier,
            IStaticWorldObject targetObject,
            out double obstacleBlockDamageCoef)
        {
            if (IsServer)
            {
                damagePreMultiplier = LandClaimSystem.ServerAdjustDamageToUnprotectedStrongBuilding(weaponCache,
                                                                                                    targetObject,
                                                                                                    damagePreMultiplier);
            }

            var damage = base.SharedCalculateDamageByWeapon(weaponCache,
                                                            damagePreMultiplier,
                                                            targetObject,
                                                            out obstacleBlockDamageCoef);
            return damage;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            ProtoObjectWallHelper.SharedCalculateNeighborsPattern(
                data.GameObject.OccupiedTile,
                protoWall: this,
                out var sameTypeNeighbors,
                out _,
                isConsiderDestroyed: true,
                isConsiderConstructionSites: true);

            foreach (var pattern in WallPatterns.PatternsPrimary)
            {
                if (pattern.IsPass(sameTypeNeighbors))
                {
                    pattern.SetupPhysicsNormal?.Invoke(data.PhysicsBody);
                    break;
                }
            }

            // hitboxes
            data.PhysicsBody
                .AddShapeRectangle(
                    new Vector2D(1, 0.7),
                    new Vector2D(0, 0.3),
                    group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(
                    new Vector2D(1, 1.27),
                    new Vector2D(0, 0.15),
                    group: CollisionGroups.HitboxRanged);
        }
    }

    public abstract class ProtoObjectWall
        : ProtoObjectWall
            <StructurePrivateState,
                StaticObjectPublicState,
                ObjectWallClientState>
    {
    }
}
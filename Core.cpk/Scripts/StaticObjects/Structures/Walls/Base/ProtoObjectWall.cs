namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls
{
    using AtomicTorch.CBND.CoreMod.Helpers.Client.Walls;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

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
            using var result = WallTextureChunkSelector.GetRegion(NeighborsPattern.None, NeighborsPattern.None);
            return this.TextureAtlasPrimary.Chunk((byte)result.Primary.AtlasChunkPosition.X,
                                                  (byte)result.Primary.AtlasChunkPosition.Y);
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

            var occupiedTile = data.GameObject.OccupiedTile;
            SharedWallConstructionRefreshHelper.SharedRefreshNeighborObjects(occupiedTile,
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

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            ProtoObjectWallHelper.SharedCalculateNeighborsPattern(
                tile: data.GameObject.OccupiedTile,
                protoWall: this,
                sameTypeNeighbors: out var sameTypeNeighbors,
                compatibleTypeNeighbors: out _,
                isConsiderDestroyed: true,
                isConsiderConstructionSites: true);

            var physicsBody = data.PhysicsBody;

            foreach (var pattern in WallPatterns.PatternsPrimary)
            {
                if (pattern.IsPass(sameTypeNeighbors))
                {
                    pattern.SetupPhysicsNormal?.Invoke(physicsBody);
                    break;
                }
            }

            // setup hitboxes
            const double paddingIfNoNeighbor = 0.2;
            double width = 1.0,
                   offsetX = 0.0;

            if ((sameTypeNeighbors & NeighborsPattern.Left) != NeighborsPattern.Left)
            {
                width -= paddingIfNoNeighbor;
                offsetX += paddingIfNoNeighbor;
            }

            if ((sameTypeNeighbors & NeighborsPattern.Right) != NeighborsPattern.Right)
            {
                width -= paddingIfNoNeighbor;
            }

            if ((sameTypeNeighbors & (NeighborsPattern.Bottom)) != 0)
            {
                // has another wall (or door) below - use full height hitboxes
                physicsBody
                    .AddShapeRectangle((width, 1),    offset: (offsetX, 0),    group: CollisionGroups.HitboxMelee)
                    .AddShapeRectangle((width, 1.27), offset: (offsetX, 0.15), group: CollisionGroups.HitboxRanged);
            }
            else // "half"-height hitboxes
            {
                physicsBody
                    .AddShapeRectangle((width, 0.25), offset: (offsetX, 0.75), group: CollisionGroups.HitboxMelee)
                    .AddShapeRectangle((width, 0.57), offset: (offsetX, 0.85), group: CollisionGroups.HitboxRanged);
            }
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
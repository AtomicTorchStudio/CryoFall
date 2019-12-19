namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Floors
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Helpers.Client.Floors;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public abstract class ProtoObjectFloor
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectStructure
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectFloor
        where TPrivateState : StructurePrivateState, new()
        where TPublicState : StaticObjectPublicState, new()
        where TClientState : ObjectFloorClientState, new()
    {
        private ITextureAtlasResource proceduralTextureAtlasPrimary;

        private TextureAtlasResource texturePrimaryAtlas;

        /// <summary>
        /// Characters can move faster on the floor.
        /// </summary>
        public virtual double CharacterMoveSpeedMultiplier => 1.1;

        public override double ClientUpdateIntervalSeconds => double.MaxValue;

        /// <summary>
        /// Gets sound material of ground (used for movement footsteps and similar sounds).
        /// </summary>
        public abstract GroundSoundMaterial GroundSoundMaterial { get; }

        public override StaticObjectKind Kind => StaticObjectKind.Floor;

        public sealed override double ObstacleBlockDamageCoef => 0;

        public override double ServerUpdateIntervalSeconds => double.MaxValue;

        public void ClientRefreshRenderer(IStaticWorldObject worldObject)
        {
            if (worldObject.ProtoGameObject != this)
            {
                return;
            }

            var clientState = GetClientState(worldObject);
            var renderer = clientState?.Renderer;
            if (renderer == null)
            {
                // not initialized yet
                return;
            }

            var tile = worldObject.OccupiedTile;
            var textureResource = this.ClientGetTextureForTile(tile);
            clientState.Renderer.TextureResource = textureResource;
        }

        public override void ClientSetupBlueprint(Tile tile, IClientBlueprint blueprint)
        {
            blueprint.SpriteRenderer.TextureResource = this.ClientGetTextureForTile(tile);
        }

        protected override ITextureResource ClientCreateIcon()
        {
            this.InitProceduralTextureAtlas();
            var chunkPreset = FloorTextureChunkSelector.GetRegion(NeighborsPattern.None);
            return this.proceduralTextureAtlasPrimary.Chunk(chunkPreset.TargetColumn, chunkPreset.TargetRow);
        }

        protected override void ClientDeinitializeStructure(IStaticWorldObject gameObject)
        {
            base.ClientDeinitializeStructure(gameObject);
            ClientFloorRefreshHelper.SharedRefreshNeighborObjects(gameObject, isDestroy: true);
        }

        protected virtual ITextureResource ClientGetTextureForTile(Tile tile)
        {
            this.InitProceduralTextureAtlas();

            var primaryChunk = this.GetAtlasTextureChunkPosition(tile);
            if (primaryChunk == null)
            {
                return TextureResource.NoTexture;
            }

            return this.proceduralTextureAtlasPrimary.Chunk(
                primaryChunk.TargetColumn,
                primaryChunk.TargetRow);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            // don't use base implementation
            //base.ClientInitialize(data);

            this.ClientAddAutoStructurePointsBar(data);

            var gameObject = data.GameObject;
            var worldObject = gameObject;
            var clientState = GetClientState(worldObject);

            // create renderers
            clientState.Renderer = Client.Rendering.CreateSpriteRenderer(
                worldObject,
                textureResource: null,
                drawOrder: DrawOrder.Floor);

            this.ClientRefreshRenderer(gameObject);
            ClientFloorRefreshHelper.SharedRefreshNeighborObjects(gameObject, isDestroy: false);
        }

        protected sealed override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryBuildings>();

            // can build only if there is no floor or floor of different prototype
            var validatorNoFloorOfTheSameType
                = new ConstructionTileRequirements.Validator(
                    ConstructionTileRequirements.ErrorCannotBuildOnFloor,
                    c => c.Tile.StaticObjects.All(
                        o => o.ProtoStaticWorldObject != this));

            // Tile requirements is not overridable for floors implementations:
            // require solid ground, no built floor, no static (non-structure) objects, no farms
            tileRequirements
                .Clear()
                .Add(ConstructionTileRequirements.BasicRequirements)
                .Add(ConstructionTileRequirements.ValidatorNoStaticObjectsExceptPlayersStructures)
                .Add(ConstructionTileRequirements.ValidatorNoFarmPlot)
                .Add(validatorNoFloorOfTheSameType)
                .Add(ConstructionTileRequirements.ValidatorNotRestrictedArea)
                .Add(ConstructionTileRequirements.ValidatorNoNpcsAround)
                .Add(ConstructionTileRequirements.ValidatorNoPlayersNearby)
                .Add(LandClaimSystem.ValidatorIsOwnedOrFreeArea)
                .Add(LandClaimSystem.ValidatorNoRaid);

            build.StagesCount = 1;
            this.PrepareFloorConstructionConfig(build, repair);
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            this.texturePrimaryAtlas = new TextureAtlasResource(
                GenerateTexturePath(thisType),
                5,
                2,
                isTransparent: true);

            return this.texturePrimaryAtlas;
        }

        protected abstract void PrepareFloorConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair);

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            if (!data.IsFirstTimeInit)
            {
                return;
            }

            using var tempList = Api.Shared.WrapInTempList(data.GameObject.OccupiedTile.StaticObjects);
            foreach (var occupiedTileStaticObject in tempList)
            {
                if (occupiedTileStaticObject != data.GameObject
                    && occupiedTileStaticObject.ProtoStaticWorldObject is IProtoObjectFloor)
                {
                    // found another floor built in the cell - destroy it
                    Server.World.DestroyObject(occupiedTileStaticObject);
                }
            }
        }

        // no physics for floor
        protected sealed override void SharedCreatePhysics(CreatePhysicsData data)
        {
        }

        /// <summary>
        /// Returns rectangle (region of texture) according to the nearby walls of the same type
        /// </summary>
        private FloorChunkPreset GetAtlasTextureChunkPosition(Tile tile)
        {
            var sameTypeNeighbors = NeighborsPattern.None;

            var eightNeighborTiles = tile.EightNeighborTiles;
            var tileIndex = -1;
            foreach (var neighborTile in eightNeighborTiles)
            {
                tileIndex++;
                foreach (var o in neighborTile.StaticObjects)
                {
                    if (o.ProtoWorldObject != this
                        || o.IsDestroyed)
                    {
                        continue;
                    }

                    // found same floor type in neighbor tile
                    sameTypeNeighbors |= NeighborsPatternDirections.NeighborDirectionSameType[tileIndex];
                    break;
                }
            }

            return FloorTextureChunkSelector.GetRegion(sameTypeNeighbors);
        }

        private void InitProceduralTextureAtlas()
        {
            if (this.proceduralTextureAtlasPrimary == null)
            {
                this.proceduralTextureAtlasPrimary =
                    FloorTextureComposer.CreateProceduralTexture(
                        this.Id + "_Primary",
                        this.texturePrimaryAtlas);
            }
        }
    }

    public abstract class ProtoObjectFloor
        : ProtoObjectFloor
            <StructurePrivateState, StaticObjectPublicState, ObjectFloorClientState>
    {
    }
}
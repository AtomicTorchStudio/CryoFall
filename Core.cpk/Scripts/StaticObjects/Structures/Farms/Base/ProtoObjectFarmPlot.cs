namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Vegetation.Plants;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectFarmPlot
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoObjectFarm
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoObjectFarmPlot
        where TPrivateState : StructurePrivateState, new()
        where TPublicState : StaticObjectPublicState, new()
        where TClientState : StaticObjectClientState, new()
    {
        public const string ErrorSoilNotSuitable = "This soil type is not suitable for agriculture.";

        private static readonly RenderingMaterial MaterialGround
            = IsClient
                  ? RenderingMaterial.Create(new EffectResource("Terrain/GroundTileField"))
                  : null;

        private ClientFarmPlotBlendHelper clientBlendHelper;

        public TextureAtlasResource BlendMaskTextureAtlas { get; private set; }

        public override StaticObjectKind Kind => StaticObjectKind.Floor;

        public abstract ITextureResource Texture { get; }

        protected virtual ITextureResource BlendMaskTexture { get; }
            = new TextureResource("Terrain/Field/MaskField");

        protected abstract TextureResource TextureFieldFertilized { get; }

        protected abstract TextureResource TextureFieldWatered { get; }

        public override void ClientSetupBlueprint(Tile tile, IClientBlueprint blueprint)
        {
            var renderer = blueprint.SpriteRenderer;
            renderer.TextureResource = this.DefaultTexture;

            // setup drawing of the top left chunk of the texture
            renderer.CustomTextureSourceRectangle = new RectangleInt(0,
                                                                     0,
                                                                     ScriptingConstants.TileSizeRealPixels,
                                                                     ScriptingConstants.TileSizeRealPixels);

            // change draw world position and origin to draw the top left chunk of the texture
            renderer.PositionOffset = (0, 1);
            renderer.SpritePivotPoint = (0, 1);
            this.ClientSetupRenderer(renderer);
        }

        protected override void ClientDeinitializeStructure(IStaticWorldObject gameObject)
        {
            base.ClientDeinitializeStructure(gameObject);
            this.clientBlendHelper.Update(gameObject.OccupiedTile);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            this.ClientAddAutoStructurePointsBar(data);
            StructureLandClaimIndicatorManager.ClientInitialize(data.GameObject);

            var worldObject = data.GameObject;
            var tile = worldObject.OccupiedTile;

            var drawOrder = DrawOrder.GroundBlend + 2;
            var renderer = Client.Rendering.CreateSpriteRenderer(
                worldObject,
                this.Texture,
                drawOrder: drawOrder);
            renderer.RenderingMaterial = MaterialGround;
            this.ClientSetupRenderer(renderer);

            data.ClientState.Renderer = renderer;

            this.clientBlendHelper.Update(tile);

            var plant = SharedGetFarmPlantWorldObject(tile);
            if (plant != null)
            {
                // Add extra sprites for watered and fertilized plants.
                // Please note - the farm plot is re-initialized by plant object when its state changed.
                var plantPublicState = plant.GetPublicState<PlantPublicState>();
                if (plantPublicState.IsWatered)
                {
                    Client.Rendering.CreateSpriteRenderer(
                        worldObject,
                        this.TextureFieldWatered,
                        positionOffset: (0.5, 0.4),
                        spritePivotPoint: (0.5, 0.5),
                        drawOrder: drawOrder + 2);
                }

                if (plantPublicState.IsFertilized)
                {
                    Client.Rendering.CreateSpriteRenderer(
                        worldObject,
                        this.TextureFieldFertilized,
                        positionOffset: (0.5, 0.4),
                        spritePivotPoint: (0.5, 0.5),
                        drawOrder: drawOrder + 2);
                }
            }
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            renderer.Size = ScriptingConstants.TileSizeRenderingVirtualSize;
        }

        protected sealed override void PrepareFarmConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair)
        {
            tileRequirements
                .Clear()
                .Add(ConstructionTileRequirements.BasicRequirements)
                .Add(ErrorSoilNotSuitable, c => c.Tile.ProtoTile is IProtoTileFarmAllowed)
                .Add(ConstructionTileRequirements.ValidatorNoFarmPlot)
                .Add(ConstructionTileRequirements.ValidatorNoFloor)
                .Add(ConstructionTileRequirements.ValidatorNoStaticObjectsExceptFloor)
                .Add(ConstructionTileRequirements.ValidatorNoPhysicsBodyStatic)
                .Add(ConstructionTileRequirements.ValidatorNotRestrictedArea)
                .Add(ConstructionTileRequirements.ValidatorNoNpcsAround)
                .Add(ConstructionTileRequirements.ValidatorNoPlayersNearby)
                .Add(LandClaimSystem.ValidatorIsOwnedLandInPvEOnly)
                .Add(LandClaimSystem.ValidatorNoRaid);

            this.PrepareFarmPlotConstructionConfig(tileRequirements, build, repair);
        }

        protected abstract void PrepareFarmPlotConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair);

        protected override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();

            this.BlendMaskTextureAtlas = new TextureAtlasResource(
                this.BlendMaskTexture,
                columns: 4,
                rows: 1);

            this.clientBlendHelper = IsClient
                                         ? new ClientFarmPlotBlendHelper(this)
                                         : null;
        }

        private static IStaticWorldObject SharedGetFarmPlantWorldObject(Tile tile)
        {
            return tile.StaticObjects.FirstOrDefault(
                o => o.ProtoStaticWorldObject is IProtoObjectPlant);
        }
    }

    public abstract class ProtoObjectFarmPlot
        : ProtoObjectFarmPlot
            <StructurePrivateState, StaticObjectPublicState, StaticObjectClientState>
    {
    }
}
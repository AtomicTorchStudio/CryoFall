namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Farms
{
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Tiles;
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

        private static readonly Size2F TileRendererSize
            = Api.IsClient
                  ? ScriptingConstants.TileSizeRenderingVirtualSize
                    * Api.Client.Rendering.SpriteQualitySizeMultiplier
                  : 0;

        private TextureAtlasResource cachedBlendMaskTextureAtlas;

        private ClientFarmPlotBlendHelper clientBlendHelper;

        public TextureAtlasResource BlendMaskTextureAtlas => this.cachedBlendMaskTextureAtlas;

        public override StaticObjectKind Kind => StaticObjectKind.Floor;

        public abstract ITextureResource Texture { get; }

        protected virtual ITextureResource BlendMaskTexture { get; }
            = new TextureResource("Terrain/Field/MaskField");

        public override void ClientSetupBlueprint(Tile tile, IClientBlueprint blueprint)
        {
            var renderer = blueprint.SpriteRenderer;
            renderer.TextureResource = this.DefaultTexture;

            // setup drawing of the top left chunk of the texture
            renderer.CustomTextureSourceRectangle =
                new RectangleInt(0, 0, ScriptingConstants.TileSizeRealPixels, ScriptingConstants.TileSizeRealPixels);

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

            var worldObject = data.GameObject;
            var renderer = Client.Rendering.CreateSpriteRenderer(
                worldObject,
                this.Texture);
            renderer.RenderingMaterial = MaterialGround;
            renderer.DrawOrder = DrawOrder.GroundBlend + 2;
            this.ClientSetupRenderer(renderer);

            data.ClientState.Renderer = renderer;

            this.clientBlendHelper.Update(worldObject.OccupiedTile);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            renderer.Size = TileRendererSize;
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
                .Add(LandClaimSystem.ValidatorIsOwnedOrFreeArea)
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

            this.cachedBlendMaskTextureAtlas = new TextureAtlasResource(
                this.BlendMaskTexture,
                columns: 4,
                rows: 1);

            this.clientBlendHelper = IsClient
                                         ? new ClientFarmPlotBlendHelper(this)
                                         : null;
        }
    }

    public abstract class ProtoObjectFarmPlot
        : ProtoObjectFarmPlot
            <StructurePrivateState, StaticObjectPublicState, StaticObjectClientState>
    {
    }
}
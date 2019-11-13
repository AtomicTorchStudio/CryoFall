namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.PowerStorage
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectPowerStorageLarge : ProtoObjectPowerStorage
    {
        private static readonly SoundResource SoundResourceActive
            = new SoundResource("Objects/Structures/ObjectPowerStorageLarge/Active");

        private readonly TextureAtlasResource textureAtlasActive;

        public ObjectPowerStorageLarge()
        {
            this.textureAtlasActive = new TextureAtlasResource(
                this.GenerateTexturePath() + "Animation",
                columns: 3,
                rows: 2,
                isTransparent: true);
        }

        public override string Description => GetProtoEntity<ObjectPowerStorage>().Description;

        public override double ElectricityCapacity => 30000;

        public override string Name => "Large power storage";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 1;

        public override float StructurePointsMax => 3500;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            var worldObject = data.GameObject;

            // create sound emitter
            data.ClientState.SoundEmitter = Client.Audio.CreateSoundEmitter(
                data.GameObject,
                SoundResourceActive,
                isLooped: true,
                volume: 0.5f,
                radius: 1.5f);
            data.ClientState.SoundEmitter.CustomMaxDistance = 4f;

            // add fan sprite renderers
            var randomizer = (int)(PositionHashHelper.GetHashUInt32(worldObject.TilePosition.X,
                                                                    worldObject.TilePosition.Y)
                                   % 128);

            AddFanRenderer((41 / 256.0, 253 / 256.0),  frameOffset: randomizer,     out var componentAnimator1);
            AddFanRenderer((137 / 256.0, 253 / 256.0), frameOffset: randomizer + 2, out var componentAnimator2);

            // sound and animation are played only if there is a power grid (a land claim area)
            var isActive = LandClaimSystem.SharedIsObjectInsideAnyArea(worldObject);
            componentAnimator1.IsEnabled
                = componentAnimator2.IsEnabled
                      = data.ClientState.SoundEmitter.IsEnabled
                            = isActive;

            void AddFanRenderer(
                Vector2D vector2D,
                int frameOffset,
                out ClientComponentSpriteSheetAnimator componentAnimator)
            {
                var overlaySpriteRenderer = Client.Rendering.CreateSpriteRenderer(
                    worldObject,
                    positionOffset: vector2D,
                    spritePivotPoint: (0, 0),
                    drawOrder: data.ClientState.Renderer.DrawOrder);
                overlaySpriteRenderer.DrawOrderOffsetY = -vector2D.Y
                                                         - 0.01
                                                         + data.ClientState.Renderer.DrawOrderOffsetY;

                // add sprite sheet animation for fan sprite
                var sceneObject = worldObject.ClientSceneObject;
                componentAnimator = sceneObject.AddComponent<ClientComponentSpriteSheetAnimator>();
                componentAnimator.Setup(overlaySpriteRenderer,
                                        ClientComponentSpriteSheetAnimator.CreateAnimationFrames(
                                            this.textureAtlasActive),
                                        isLooped: true,
                                        frameDurationSeconds: 5 / 60.0,
                                        initialFrameOffset: frameOffset);
            }
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.DrawOrderOffsetY = 0.33;
        }

        protected override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            category = GetCategory<StructureCategoryElectricity>();

            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.Short;
            build.AddStageRequiredItem<ItemWire>(count: 10);
            build.AddStageRequiredItem<ItemIngotSteel>(count: 1);
            build.AddStageRequiredItem<ItemIngotLithium>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.Short;
            repair.AddStageRequiredItem<ItemWire>(count: 5);
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 1);
        }

        protected override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();

            if (IsClient)
            {
                ClientLandClaimAreaManager.AreaAdded += ClientLandClaimAreaManagerAreaAddedOrRemovedHandler;
                ClientLandClaimAreaManager.AreaRemoved += ClientLandClaimAreaManagerAreaAddedOrRemovedHandler;
            }
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.9, 0.6),  offset: (0.05, 0))
                .AddShapeRectangle(size: (0.8, 0.4),  offset: (0.1, 0.6),  group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.8, 0.2),  offset: (0.1, 0.75), group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (0.9, 1.17), offset: (0.05, 0),   group: CollisionGroups.ClickArea);
        }

        private static void ClientLandClaimAreaManagerAreaAddedOrRemovedHandler(ILogicObject obj)
        {
            var bounds = LandClaimSystem.SharedGetLandClaimAreaBounds(obj);
            foreach (var staticWorldObject in Client.World.GetStaticWorldObjectsOfProto<ObjectPowerStorageLarge>())
            {
                if (bounds.Contains(staticWorldObject.Bounds))
                {
                    // force re-initialize (required to stop/start sound and animation)
                    staticWorldObject.ClientInitialize();
                }
            }
        }
    }
}
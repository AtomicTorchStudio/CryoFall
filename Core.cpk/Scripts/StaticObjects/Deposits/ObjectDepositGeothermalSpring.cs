namespace AtomicTorch.CBND.CoreMod.StaticObjects.Deposits
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectDepositGeothermalSpring : ProtoObjectDeposit
    {
        private static readonly SoundResource SoundResourceActive
            = new SoundResource("Objects/Deposits/ObjectGeothermalSpring/Active");

        private TextureAtlasResource textureAtlas1;

        private TextureAtlasResource textureAtlas2;

        public override double ClientUpdateIntervalSeconds => 0.5;

        public override double DecaySpeedMultiplierWhenExtractingActive => 1;

        public override double LifetimeTotalDurationSeconds { get; }
            = TimeSpan.FromDays(4).TotalSeconds;

        public override string Name => "Geothermal spring";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.SolidGround;

        public override float StructurePointsMax => 10000;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            // setup animation
            var animationFrameDurationSeconds = 1 / 8.0;
            var clientState = data.ClientState;

            var textureAtlas = SharedIsAlternativeVariant(data.GameObject.TilePosition)
                                   ? this.textureAtlas1
                                   : this.textureAtlas2;

            data.GameObject
                .ClientSceneObject
                .AddComponent<ClientComponentSpriteSheetAnimator>()
                .Setup(
                    clientState.Renderer,
                    ClientComponentSpriteSheetAnimator.CreateAnimationFrames(textureAtlas),
                    isLooped: true,
                    frameDurationSeconds: animationFrameDurationSeconds,
                    randomizeInitialFrame: true);

            if (!data.GameObject.OccupiedTile.StaticObjects.Any(
                    o => o.ProtoStaticWorldObject is IProtoObjectExtractor))
            {
                // create sound emitter as there is no extractor
                clientState.SoundEmitter = Client.Audio.CreateSoundEmitter(
                    data.GameObject,
                    SoundResourceActive,
                    isLooped: true,
                    volume: 0.5f,
                    radius: 1.5f);
                clientState.SoundEmitter.CustomMaxDistance = 5;
            }
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("###",
                         "###",
                         "###");
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            var texturePath = GenerateTexturePath(typeof(ObjectDepositGeothermalSpring));
            this.textureAtlas1 = new TextureAtlasResource(
                texturePath,
                columns: 6,
                rows: 1,
                isTransparent: true);

            this.textureAtlas2 = new TextureAtlasResource(
                texturePath + "2",
                columns: 6,
                rows: 1,
                isTransparent: true);

            return this.textureAtlas1;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            var physicsBody = data.PhysicsBody;

            if (SharedIsAlternativeVariant(data.GameObject.TilePosition))
            {
                physicsBody
                    .AddShapeCircle(radius: 0.4,  center: (1.2, 1.1))
                    .AddShapeCircle(radius: 0.4,  center: (1.5, 1.1))
                    .AddShapeCircle(radius: 0.4,  center: (1.9, 1.1))
                    .AddShapeCircle(radius: 0.25, center: (0.95, 2.0))
                    .AddShapeCircle(radius: 1.25, center: (1.5, 1.5), group: CollisionGroups.ClickArea);
            }
            else
            {
                physicsBody
                    .AddShapeCircle(radius: 0.4,  center: (1.2, 1.1))
                    .AddShapeCircle(radius: 0.4,  center: (1.5, 1.1))
                    .AddShapeCircle(radius: 0.4,  center: (1.9, 1.1))
                    .AddShapeCircle(radius: 1.25, center: (1.5, 1.3), group: CollisionGroups.ClickArea);
            }
        }

        private static bool SharedIsAlternativeVariant(Vector2Ushort tilePosition)
        {
            return PositionalRandom.Get(tilePosition, 0, 2, seed: 9125835) == 0;
        }
    }
}
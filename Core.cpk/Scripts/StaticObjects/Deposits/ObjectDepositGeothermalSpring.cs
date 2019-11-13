namespace AtomicTorch.CBND.CoreMod.StaticObjects.Deposits
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectDepositGeothermalSpring : ProtoObjectDeposit
    {
        private static readonly SoundResource SoundResourceActive
            = new SoundResource("Objects/Deposits/ObjectGeothermalSpring/Active");

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

            data.GameObject
                .ClientSceneObject
                .AddComponent<ClientComponentSpriteSheetAnimator>()
                .Setup(
                    data.ClientState.Renderer,
                    ClientComponentSpriteSheetAnimator.CreateAnimationFrames(
                        (ITextureAtlasResource)this.DefaultTexture),
                    isLooped: true,
                    frameDurationSeconds: animationFrameDurationSeconds,
                    randomizeInitialFrame: true);

            if (!data.GameObject.OccupiedTile.StaticObjects.Any(
                    o => o.ProtoStaticWorldObject is IProtoObjectExtractor))
            {
                // create sound emitter as there is no extractor
                data.ClientState.SoundEmitter = Client.Audio.CreateSoundEmitter(
                    data.GameObject,
                    SoundResourceActive,
                    isLooped: true,
                    volume: 0.5f,
                    radius: 1.5f);
                data.ClientState.SoundEmitter.CustomMaxDistance = 5;
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
            return new TextureAtlasResource(
                GenerateTexturePath(typeof(ObjectDepositGeothermalSpring)),
                columns: 6,
                rows: 1,
                isTransparent: true);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.4,  center: (1.2, 1.1))
                .AddShapeCircle(radius: 0.4,  center: (1.5, 1.1))
                .AddShapeCircle(radius: 0.4,  center: (1.9, 1.1))
                .AddShapeCircle(radius: 0.25, center: (0.95, 2.0))
                .AddShapeCircle(radius: 1.25, center: (1.5, 1.5), group: CollisionGroups.ClickArea);
        }
    }
}
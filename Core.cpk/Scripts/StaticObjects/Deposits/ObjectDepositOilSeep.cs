namespace AtomicTorch.CBND.CoreMod.StaticObjects.Deposits
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectDepositOilSeep : ProtoObjectDeposit
    {
        private static readonly SoundResource SoundResourceActive
            = new SoundResource("Objects/Deposits/ObjectOilSeep/Active");

        public override double ClientUpdateIntervalSeconds => 0.5;

        public override double DecaySpeedMultiplierWhenExtractingActive => 1;

        public override double LifetimeTotalDurationSeconds { get; }
            = TimeSpan.FromDays(4).TotalSeconds;

        public override string Name => "Oil seep";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.SolidGround;

        public override float StructurePointsMax => 10000;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

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
                data.ClientState.SoundEmitter.CustomMaxDistance = 3;
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
            return new TextureResource(
                GenerateTexturePath(typeof(ObjectDepositOilSeep)));
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.48, center: (1.53, 1.55))
                .AddShapeCircle(radius: 0.33, center: (1.9, 1.2))
                .AddShapeCircle(radius: 1.25, center: (1.5, 1.5), group: CollisionGroups.ClickArea);
        }
    }
}
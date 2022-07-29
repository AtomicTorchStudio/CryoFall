namespace AtomicTorch.CBND.CoreMod.StaticObjects.Deposits
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectDepositOilSeep : ProtoObjectDeposit
    {
        private static readonly SoundResource SoundResourceActive
            = new("Objects/Deposits/ObjectOilSeep/Active");

        private TextureResource texture1;

        private TextureResource texture2;

        public override double ClientUpdateIntervalSeconds => 0.5;

        public override double DecaySpeedMultiplierWhenExtractingActive => 1;

        public override double LifetimeTotalDurationSeconds
            => RateResourcesPvPDepositLifetimeDuration.SharedValue;

        public override string Name => "Oil seep";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.SolidGround;

        public override float StructurePointsMax => 10000;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

            var clientState = data.ClientState;

            var texture = SharedIsAlternativeVariant(data.GameObject.TilePosition)
                              ? this.texture1
                              : this.texture2;

            clientState.Renderer.TextureResource = texture;

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
                clientState.SoundEmitter.CustomMaxDistance = 3;
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
            var texturePath = GenerateTexturePath(typeof(ObjectDepositOilSeep));
            this.texture1 = new TextureResource(texturePath,
                                                isTransparent: true);

            this.texture2 = new TextureResource(texturePath + "2",
                                                isTransparent: true);

            return this.texture1;
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            var physicsBody = data.PhysicsBody;

            if (SharedIsAlternativeVariant(data.GameObject.TilePosition))
            {
                physicsBody.AddShapeCircle(radius: 0.48, center: (1.53, 1.55))
                           .AddShapeCircle(radius: 0.33, center: (1.9, 1.2))
                           .AddShapeCircle(radius: 1.25, center: (1.5, 1.5), group: CollisionGroups.ClickArea);
            }
            else
            {
                physicsBody.AddShapeCircle(radius: 1.25, center: (1.3, 1.25), group: CollisionGroups.ClickArea);
            }
        }

        private static bool SharedIsAlternativeVariant(Vector2Ushort tilePosition)
        {
            return PositionalRandom.Get(tilePosition, 0, 2, seed: 9125835) == 0;
        }
    }
}
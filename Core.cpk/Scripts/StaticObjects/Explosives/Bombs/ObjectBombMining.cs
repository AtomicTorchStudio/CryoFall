namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives.Bombs
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Deposits;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectBombMining : ProtoObjectExplosive
    {
        public const double DamageToMinerals = 2000;

        private ITextureAtlasResource atlasTexture;

        public override double DamageRadius => 3.1;

        public override TimeSpan ExplosionDelay { get; } = TimeSpan.FromSeconds(3);

        public override bool IsActivatesRaidModeForLandClaim => false;

        public override string Name => "Mining charge";

        public override double ServerCalculateTotalDamageByExplosive(
            IProtoStaticWorldObject targetStaticWorldObjectProto)
        {
            if (targetStaticWorldObjectProto is IProtoObjectMineral
                || targetStaticWorldObjectProto is IProtoObjectDeposit)
            {
                // the target object is a mineral or deposit, deal special damage
                return DamageToMinerals;
            }

            return base.ServerCalculateTotalDamageByExplosive(targetStaticWorldObjectProto);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            data.ClientState.Renderer.DrawOrderOffsetY = 0.355;

            // add sprite sheet animation
            var sceneObject = data.GameObject.ClientSceneObject;
            sceneObject.AddComponent<ClientComponentSpriteSheetAnimator>()
                       .Setup(data.ClientState.Renderer,
                              ClientComponentSpriteSheetAnimator.CreateAnimationFrames(this.atlasTexture),
                              isLooped: true,
                              frameDurationSeconds: 3 / 60.0);

            // add light source at the firing fuse
            var lightSource = ClientLighting.CreateLightSourceSpot(
                sceneObject,
                LightColors.WoodFiring,
                size: (3, 3),
                logicalSize: 0,
                positionOffset: (0.7, 0.7));

            // add light flickering
            sceneObject.AddComponent<ClientComponentLightSourceEffectFlickering>()
                       .Setup(lightSource,
                              flickeringPercents: 10,
                              flickeringChangePercentsPerSecond: 70);
        }

        protected override void PrepareDamageDescriptionCharacters(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            DamageDistribution damageDistribution)
        {
            damageValue = 50;
            armorPiercingCoef = 0;
            finalDamageMultiplier = 1;
            damageDistribution.Set(DamageType.Kinetic, 1);
        }

        protected override void PrepareDamageDescriptionStructures(
            out double damageValue,
            out double defencePenetrationCoef)
        {
            // Very little explosive damage, since it is not a weapon and should not be used as such.
            // Also, please note the property ActivatesRaidModeForLandClaim.
            damageValue = 50;
            defencePenetrationCoef = 0;
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            this.atlasTexture = new TextureAtlasResource(GenerateTexturePath(thisType) + "Animation",
                                                         columns: 4,
                                                         rows: 1,
                                                         isTransparent: true);

            return this.atlasTexture.Chunk(0, 0);
        }

        protected override void PrepareProtoObjectExplosive(out ExplosionPreset explosionPresets)
        {
            explosionPresets = ExplosionPresets.Large;
        }
    }
}
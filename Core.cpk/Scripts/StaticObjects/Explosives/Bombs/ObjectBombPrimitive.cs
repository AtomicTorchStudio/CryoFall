namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives.Bombs
{
    using System;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Rendering.Lighting;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectBombPrimitive : ProtoObjectExplosive
    {
        private ITextureAtlasResource atlasTexture;

        public override double DamageRadius => 2.1;

        public override bool IsActivatesRaidBlock => true;

        public override string Name => "Primitive bomb";

        public override double RaidBlockDurationMutliplier => 0.5;

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);

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
                positionOffset: (0.31, 0.31));

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
            damageValue = 100;
            armorPiercingCoef = 0.5;
            finalDamageMultiplier = 1;
            damageDistribution.Set(DamageType.Kinetic, 1);
        }

        protected override void PrepareDamageDescriptionStructures(
            out double damageValue,
            out double defencePenetrationCoef)
        {
            damageValue = 6000;
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
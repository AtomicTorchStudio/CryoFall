namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives
{
    using System;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectDepositExplosion : ProtoObjectExplosive
    {
        public override double DamageRadius => 4;

        public override ITextureResource DefaultTexture
            => this.ExplosionPreset.SpriteAtlasResources[0].Chunk(6, 0);

        public override TimeSpan ExplosionDelay => TimeSpan.Zero;

        public override bool IsActivatesRaidModeForLandClaim => false;

        // resource deposit (oil seep, geothermal spring, etc) explosion
        public override string Name => "Deposit explosion";

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            var sceneObject = Client.Scene.GetSceneObject(data.GameObject);
            var componentBombCountdown = sceneObject.FindComponent<ClientComponentBombCountdown>();
            componentBombCountdown.IsRendering = false;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("###",
                         "###",
                         "###");
        }

        protected override void PrepareDamageDescriptionCharacters(
            out double damageValue,
            out double armorPiercingCoef,
            out double finalDamageMultiplier,
            DamageDistribution damageDistribution)
        {
            damageValue = 120;
            armorPiercingCoef = 0;
            finalDamageMultiplier = 1;
            damageDistribution.Set(DamageType.Kinetic, 1);
        }

        protected override void PrepareDamageDescriptionStructures(
            out double damageValue,
            out double defencePenetrationCoef)
        {
            damageValue = 100_000;
            defencePenetrationCoef = 1;
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return TextureResource.NoTexture;
        }

        protected override void PrepareProtoObjectExplosive(out ExplosionPreset explosionPresets)
        {
            explosionPresets = ExplosionPresets.SpecialDepositExplosion;
        }
    }
}
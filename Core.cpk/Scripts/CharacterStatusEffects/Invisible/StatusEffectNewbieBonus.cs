namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Invisible
{
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.CharacterCreation;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    /// <summary>
    /// This status effect is providing a newbie bonus for one hour
    /// (decreased food/water/energy consumption).
    /// Automatically added when the character is created.
    /// </summary>
    public class StatusEffectNewbieBonus : ProtoStatusEffect
    {
        public override string Description => string.Empty;

        public override double IntensityAutoDecreasePerSecondValue => 1.0 / 3600.0; // total of 60 minutes for max time

        public override bool IsPublic => false;

        public override bool IsRemovedOnRespawn => false;

        public override StatusEffectKind Kind => StatusEffectKind.Buff;

        public override string Name => this.ShortId;

        public override double ServerUpdateIntervalSeconds => 60;

        public override double VisibilityIntensityThreshold => double.MaxValue;

        // an icon is not necessary (since this is an invisible effect)
        protected override ITextureResource IconTextureResource => TextureResource.NoTexture;

        protected override void PrepareEffects(Effects effects)
        {
            effects.AddPercent(this, StatName.HungerRate, -30)
                   .AddPercent(this, StatName.ThirstRate,                         -30)
                   .AddPercent(this, StatName.RunningStaminaConsumptionPerSecond, -30);
        }

        protected override void PrepareProtoStatusEffect()
        {
            if (IsServer)
            {
                CharacterCreationSystem.ServerCharacterCreated += this.ServerCharacterCreatedHandler;
            }
        }

        private void ServerCharacterCreatedHandler(ICharacter character)
        {
            if (!Api.IsEditor)
            {
                character.ServerAddStatusEffect(this);
            }
        }
    }
}
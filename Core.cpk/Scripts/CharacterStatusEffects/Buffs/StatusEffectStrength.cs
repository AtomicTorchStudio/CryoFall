namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs
{
    using AtomicTorch.CBND.CoreMod.Stats;

    public class StatusEffectStrength : ProtoStatusEffect
    {
        public override string Description =>
            "You are under the influence of a stimulant, temporarily making you much stronger. You can more easily perform tasks requiring strength.";

        public override double IntensityAutoDecreasePerSecondValue
            => 1.0 / 600.0; // total of 10 minutes for max possible time

        public override StatusEffectKind Kind => StatusEffectKind.Buff;

        public override string Name => "Increased strength";

        protected override void PrepareEffects(Effects effects)
        {
            // increase certain activity effectiveness
            effects.AddPercent(this, StatName.MiningSpeed,      100);
            effects.AddPercent(this, StatName.WoodcuttingSpeed, 100);

            // increase combat effectiveness
            effects.AddPercent(this, StatName.WeaponMeleeDamageBonusMultiplier,         10);
            effects.AddPercent(this, StatName.WeaponMeleeSpecialEffectChanceMultiplier, 100);
        }
    }
}
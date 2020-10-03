namespace AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Debuffs
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Implants;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    /// <summary>
    /// This effect is added automatically when any of the equipped implants are broken.
    /// The effect is automatically removed when the broken implant is removed.
    /// </summary>
    public class StatusEffectBrokenImplant : ProtoStatusEffect
    {
        public override string Description =>
            "A broken implant in your body significantly affects your well-being. Your maximum health (HP) is lowered as a result.";

        public override StatusEffectDisplayMode DisplayMode => StatusEffectDisplayMode.None;

        // does not decrease
        public override double IntensityAutoDecreasePerSecondValue => 0;

        public override bool IsRemovedOnRespawn => false;

        public override StatusEffectKind Kind => StatusEffectKind.Debuff;

        public override string Name => "Broken implant sickness";

        public override double ServerAutoAddRepeatIntervalSeconds => 5;

        protected override void PrepareEffects(Effects effects)
        {
            // reduce max health by 15%
            effects.AddValue(this, StatName.HealthMax, -15);
        }

        protected override IEnumerable<ICharacter> ServerAutoAddGetCharacterCandidates()
        {
            return Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true);
        }

        protected override void ServerOnAutoAdd(ICharacter character)
        {
            if (character.SharedHasStatusEffect<StatusEffectBrokenImplant>())
            {
                return;
            }

            if (ServerHasBrokenImplant(character))
            {
                character.ServerAddStatusEffect(this, 1);
            }
        }

        protected override void ServerUpdate(StatusEffectData data)
        {
            var hasBrokenImplant = ServerHasBrokenImplant(data.Character);
            data.Intensity = hasBrokenImplant ? 1 : 0;
        }

        private static bool ServerHasBrokenImplant(ICharacter character)
        {
            var containerEquipment = character.SharedGetPlayerContainerEquipment();
            foreach (var item in containerEquipment.Items)
            {
                if (item.ProtoItem is ItemImplantBroken)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
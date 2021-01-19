namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects.Buffs;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemTyrantHeart : ProtoItemConsumable
    {
        public override string Description =>
            "Strange spongy organ from an unknown lifeform. Emits exotic particles as it decays.";

        public override string Name => "Sand Tyrant heart";

        protected override void PrepareEffects(EffectActionsList effects)
        {
            effects
                .WillAddEffect<StatusEffectQuantumAcceleration>(intensity: 1.00);
        }

        protected override void PrepareHints(List<string> hints)
        {
            base.PrepareHints(hints);
            hints.Add(GetProtoEntity<StatusEffectQuantumAcceleration>().Description);
        }

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
        }
    }
}
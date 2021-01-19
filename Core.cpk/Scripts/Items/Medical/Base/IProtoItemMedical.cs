namespace AtomicTorch.CBND.CoreMod.Items.Medical
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public interface IProtoItemMedical : IProtoItemUsableFromContainer
    {
        IReadOnlyList<EffectAction> Effects { get; }
    }
}
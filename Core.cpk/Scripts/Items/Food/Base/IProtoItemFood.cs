namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public interface IProtoItemFood
        : IProtoItemOrganic,
          IProtoItemUsableFromContainer,
          IProtoItemWithFreshness
    {
        IReadOnlyList<EffectAction> Effects { get; }

        float FoodRestore { get; }

        bool IsAvailableInCompletionist { get; }

        float StaminaRestore { get; }

        float WaterRestore { get; }
    }
}
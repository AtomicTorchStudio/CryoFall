namespace AtomicTorch.CBND.CoreMod.Items.Food
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;

    public interface IProtoItemFood
        : IProtoItemOrganic,
          IProtoItemUsableFromContainer,
          IProtoItemWithFreshness
    {
        float FoodRestore { get; }

        float HealthRestore { get; }

        bool IsAvailableInCompletionist { get; }

        float StaminaRestore { get; }

        float WaterRestore { get; }
    }
}
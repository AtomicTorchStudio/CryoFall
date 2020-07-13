namespace AtomicTorch.CBND.CoreMod.Items.Fishing.Base
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;

    public interface IProtoItemFish : IProtoItemUsableFromContainer
    {
        FishingBaitWeightReadOnlyList BaitWeightList { get; }

        IReadOnlyDropItemsList DropItemsList { get; }

        bool IsSaltwaterFish { get; }

        float MaxLength { get; }

        float MaxWeight { get; }

        byte RequiredFishingKnowledgeLevel { get; }
    }
}
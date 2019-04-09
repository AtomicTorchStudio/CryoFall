namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public interface IProtoCharacterMob : IProtoCharacter, IProtoSpawnableObject
    {
        IReadOnlyDropItemsList LootDroplist { get; }
    }
}
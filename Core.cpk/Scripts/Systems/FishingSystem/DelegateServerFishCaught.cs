namespace AtomicTorch.CBND.CoreMod.Systems.FishingSystem
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public delegate void DelegateServerFishCaught(ICharacter character, IItem itemFish, float sizeValue);
}
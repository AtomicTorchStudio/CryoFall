namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.GameApi.Data.Items;

    public interface ICharacterPublicStateWithEquipment : ICharacterPublicState
    {
        IItemsContainer ContainerEquipment { get; }
    }
}
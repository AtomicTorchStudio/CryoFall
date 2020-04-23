namespace AtomicTorch.CBND.CoreMod.Items.Tools
{
    using AtomicTorch.CBND.GameApi.Data.Items;

    public interface IProtoItemToolLight : IProtoItemTool, IProtoItemWithFuel, IProtoItemWithCharacterAppearance
    {
        IReadOnlyItemLightConfig ItemLightConfig { get; }

        void ClientTrySetActiveState(IItem item, bool setIsActive);
    }
}
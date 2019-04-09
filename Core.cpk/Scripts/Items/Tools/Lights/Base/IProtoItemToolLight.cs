namespace AtomicTorch.CBND.CoreMod.Items.Tools.Lights
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public interface IProtoItemToolLight : IProtoItemWithFuel, IProtoItemWithCharacterAppearance
    {
        IReadOnlyItemLightConfig ItemLightConfig { get; }

        void ClientTrySetActiveState(IItem item, bool setIsActive);
    }
}
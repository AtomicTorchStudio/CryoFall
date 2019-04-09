namespace AtomicTorch.CBND.CoreMod.Items.Tools.Toolboxes
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;

    public interface IProtoItemToolToolbox
        : IProtoItemWithCharacterAppearance,
          IProtoItemWithDurablity
    {
        double ConstructionSpeedMultiplier { get; }
    }
}
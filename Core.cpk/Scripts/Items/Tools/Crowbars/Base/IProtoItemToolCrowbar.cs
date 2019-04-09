namespace AtomicTorch.CBND.CoreMod.Items.Tools.Crowbars
{
    using AtomicTorch.CBND.CoreMod.Items.Weapons;

    public interface IProtoItemToolCrowbar
        : IProtoItemWithCharacterAppearance,
          IProtoItemWithDurablity
    {
        double DeconstructionSpeedMultiplier { get; }
    }
}
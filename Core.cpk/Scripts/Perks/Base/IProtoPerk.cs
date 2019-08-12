namespace AtomicTorch.CBND.CoreMod.Perks.Base
{
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data;

    public interface IProtoPerk : IProtoEntity
    {
        IReadOnlyStatsDictionary ProtoEffects { get; }
    }
}
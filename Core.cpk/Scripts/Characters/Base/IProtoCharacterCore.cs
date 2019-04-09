namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public interface IProtoCharacterCore : IProtoCharacter
    {
        IReadOnlyStatsDictionary ProtoCharacterDefaultEffects { get; }

        double StatDefaultHealthMax { get; }
    }
}
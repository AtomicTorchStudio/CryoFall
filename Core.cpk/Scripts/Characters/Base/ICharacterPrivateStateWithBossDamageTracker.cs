namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public interface ICharacterPrivateStateWithBossDamageTracker : IPrivateState
    {
        ServerBossDamageTracker DamageTracker { get; set; }
    }
}
namespace AtomicTorch.CBND.CoreMod.Characters
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class CharacterMobPrivateState : BaseCharacterPrivateState
    {
        [TempOnly]
        public double AttackRange { get; set; }

        [TempOnly]
        public ICharacter CurrentAgroCharacter { get; set; }
    }
}
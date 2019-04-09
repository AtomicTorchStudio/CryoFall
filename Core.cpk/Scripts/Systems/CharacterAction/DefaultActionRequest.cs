namespace AtomicTorch.CBND.CoreMod.Systems
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class DefaultActionRequest : IActionRequest
    {
        public DefaultActionRequest(ICharacter character)
        {
            this.Character = character;
        }

        [TempOnly]
        public ICharacter Character { get; set; }

        public virtual bool Equals(IActionRequest other)
        {
            return this.Character == other.Character
                   && this.GetType() == other.GetType();
        }
    }
}
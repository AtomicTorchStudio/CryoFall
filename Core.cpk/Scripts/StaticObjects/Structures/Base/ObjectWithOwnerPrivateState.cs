namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures
{
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ObjectWithOwnerPrivateState : StructurePrivateState
    {
        public ICharacter Owner { get; set; }
    }
}
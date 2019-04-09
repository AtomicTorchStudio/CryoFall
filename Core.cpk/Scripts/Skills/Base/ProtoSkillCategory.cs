namespace AtomicTorch.CBND.CoreMod.Skills
{
    using AtomicTorch.CBND.GameApi.Data;

    public abstract class ProtoSkillCategory : ProtoEntity
    {
        public abstract ushort Order { get; }
    }
}
namespace AtomicTorch.CBND.CoreMod.Skills
{
    public interface ISkillEffect
    {
        string Description { get; }

        /// <summary>
        /// Required level for effect to take action.
        /// </summary>
        byte Level { get; }
    }
}
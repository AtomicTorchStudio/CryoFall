namespace AtomicTorch.CBND.CoreMod.Systems.Resources
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class GatheringActionState
        : ActionSystemState<
            GatheringSystem,
            WorldActionRequest,
            GatheringActionState,
            PublicActionStateWithTargetObjectSounds>
    {
        public GatheringActionState(
            ICharacter character,
            IWorldObject targetWorldObject,
            double durationSeconds)
            : base(character, targetWorldObject, durationSeconds)
        {
        }
    }
}
namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events
{
    using AtomicTorch.CBND.CoreMod.ClientComponents.StaticObjects;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public interface IProtoObjectHackableContainer
        : IProtoStaticWorldObject,
          IProtoWorldObjectCustomInteractionCursor
    {
        double HackingStageDuration { get; }

        double HackingStagesNumber { get; }

        IReadOnlyDropItemsList LootDroplist { get; }

        bool ServerOnHackingStage(IStaticWorldObject worldObject, ICharacter character);
    }
}
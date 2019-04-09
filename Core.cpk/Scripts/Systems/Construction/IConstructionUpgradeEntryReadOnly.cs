namespace AtomicTorch.CBND.CoreMod.Systems.Construction
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public interface IConstructionUpgradeEntryReadOnly
    {
        IProtoObjectStructure ProtoStructure { get; }

        IReadOnlyList<ProtoItemWithCount> RequiredItems { get; }

        bool CheckRequirementsSatisfied(ICharacter character);

        void ServerDestroyRequiredItems(ICharacter character);
    }
}
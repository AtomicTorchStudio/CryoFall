namespace AtomicTorch.CBND.CoreMod.Systems.Construction
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public interface IConstructionStageConfigReadOnly
    {
        bool IsAllowed { get; }

        double StageDurationSeconds { get; }

        IReadOnlyList<ProtoItemWithCount> StageRequiredItems { get; }

        byte StagesCount { get; }

        bool CheckStageCanBeBuilt(ICharacter character);

        void ServerDestroyRequiredItems(ICharacter character);

        void ServerReturnRequiredItems(ICharacter character, byte stagesCount = 1);
    }
}
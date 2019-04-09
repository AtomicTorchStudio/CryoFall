namespace AtomicTorch.CBND.CoreMod.Quests
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Resources;

    public interface IProtoQuest : IProtoLogicObject
    {
        string Description { get; }

        string Hints { get; }

        ITextureResource Icon { get; }

        IReadOnlyList<IProtoQuest> Prerequisites { get; }

        IReadOnlyList<IQuestRequirement> Requirements { get; }

        ushort RewardLearningPoints { get; }
    }
}
namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.PlayerTasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.PowerStorage;

    public class QuestEstablishPowerGrid : ProtoQuest
    {
        public const string HintGenerators =
            "Different types of [b]generators[/b], such as steam, solar and engine, can be used to produce electrical power together.";

        public const string HintGridInformation =
            "You can see [b]detailed grid information[/b] when interacting with any power storage building or in your land claim.";

        public const string HintGridZone =
            "Electrical devices only work when they are [b]within[/b] the land claim zone. Building any electrical devices outside of the land claim zone is pointless.";

        public const string HintPowerStorage =
            "Electrical energy is stored in [b]power storage[/b] buildings for convenient use when needed. It is a good idea to increase your overall energy capacity on the base.";

        public const string HintThresholds =
            "You can configure custom [u]startup and shutdown[/u] power [b]thresholds[/b] for every power consumer and generator.";

        public const string HintUnitedGrid =
            "Several connected land claim zones will behave as a [b]single power grid[/b].";

        public override string Description =>
            "Establishing a power grid for your base is an important step toward increased efficiency. There are many advanced structures that require electricity to operate.";

        public override string Name => "Establish power grid";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage3;

        protected override void PrepareQuest(QuestsList prerequisites, TasksList tasks, HintsList hints)
        {
            tasks
                .Add(TaskBuildStructure.Require<ObjectGeneratorSteam>())
                .Add(TaskBuildStructure.Require<ObjectPowerStorage>())
                .Add(TaskBuildStructure.Require<ObjectLightFloorLampSmall>());

            prerequisites
                .Add<QuestBuildChemicalLab>();

            hints
                .Add(HintGridZone)
                .Add(HintPowerStorage)
                .Add(HintGenerators)
                .Add(HintGridInformation)
                .Add(HintUnitedGrid)
                .Add(HintThresholds);
        }
    }
}
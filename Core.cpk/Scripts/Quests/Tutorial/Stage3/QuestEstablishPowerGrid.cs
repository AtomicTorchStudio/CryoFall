namespace AtomicTorch.CBND.CoreMod.Quests.Tutorial
{
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Generators;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Lights;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.PowerStorage;

    public class QuestEstablishPowerGrid : ProtoQuest
    {
        public override string Description =>
            "Establishing a power grid for your base is an important step toward increased efficiency. There are many advanced structures that require electricity to operate.";

        public override string Hints =>
            @"[*] Electrical devices only work when they are [b]within[/b] the land claim zone. Building any electrical devices outside of the land claim zone is pointless.
              [*] Electrical energy is stored in [b]power storage[/b] buildings for convenient use when needed. It is a good idea to increase your overall energy capacity on the base.
              [*] Different types of [b]generators[/b], such as steam, solar and engine, can be used to produce electrical power together.
              [*] If you [u]don't have enough[/u] stored electrical power, your base will go into [b]blackout[/b] mode and you'll need to manually restore power.
              [*] You can see [b]detailed grid information[/b] when interacting with power storage or in your land claim.
              [*] Several connected land claim zones will behave as a [b]single power grid[/b].";

        public override string Name => "Establish power grid";

        public override ushort RewardLearningPoints => QuestConstants.TutorialRewardStage3;

        protected override void PrepareQuest(QuestsList prerequisites, RequirementsList requirements)
        {
            requirements
                .Add(RequirementBuildStructure.Require<ObjectGeneratorSteam>())
                .Add(RequirementBuildStructure.Require<ObjectPowerStorage>())
                .Add(RequirementBuildStructure.Require<ObjectLightFloorLampSmall>());

            prerequisites
                .Add<QuestBuildChemicalLab>();
        }
    }
}
namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectLaunchpadStage1 : ProtoObjectLaunchpad
    {
        public const string TaskBuildRocketFrame = "Build rocket frame";

        public override string Description =>
            "The culmination of your struggles on this planet. You are finally ready to start thinking about the ultimate escape!";

        public override string Name => "Launchpad";

        protected override void PrepareLaunchpadConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade)
        {
            build.StagesCount = 10;
            build.StageDurationSeconds = BuildDuration.VeryLong;
            build.AddStageRequiredItem<ItemIngotSteel>(count: 5);
            build.AddStageRequiredItem<ItemComponentsElectronic>(count: 5);
            build.AddStageRequiredItem<ItemComponentsHighTech>(count: 1);

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.VeryLong;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 4);
            repair.AddStageRequiredItem<ItemComponentsElectronic>(count: 2);

            upgrade.AddUpgrade<ObjectLaunchpadStage2>();
            // please note: the upgrade is free but requires finishing the tasks defined below
        }

        protected override void PrepareLaunchpadTasks(LaunchpadTasksList tasksList)
        {
            tasksList.AddTask(TaskBuildRocketFrame,
                              new TextureResource("Misc/LaunchpadTasks/RocketFrame.png"),
                              new InputItems()
                                  .Add<ItemStructuralPlating>(25)
                                  .Add<ItemAramidFiber>(50)
                                  .Add<ItemComponentsMechanical>(50));
        }
    }
}
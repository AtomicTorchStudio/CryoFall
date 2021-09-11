namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Implants;
    using AtomicTorch.CBND.CoreMod.Items.Special;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Special;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectLaunchpadStage3 : ProtoObjectLaunchpad
    {
        public const string TaskBuildAnomalyAvoidanceSystem = "Build anomaly avoidance system";

        public const string TaskBuildLifeSupportSystem = "Build life support system";

        public const string TaskBuildNavigationSystem = "Build navigation system";

        public override string Description => GetProtoEntity<ObjectLaunchpadStage1>().Description;

        public override string Name => "Launchpad — Stage 3";

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            this.ClientAddRenderer(data.GameObject, TextureStage3,     TextureStage3Offset);
            this.ClientAddRenderer(data.GameObject, TextureTower,      TextureTowerOffset);
            this.ClientAddRenderer(data.GameObject, TextureTowerMast2, TextureTowerMast2Offset);
        }

        protected override void PrepareLaunchpadConstructionConfig(
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade)
        {
            // build is not allowed - it's an upgrade from previous level
            build.IsAllowed = false;

            repair.StagesCount = 10;
            repair.StageDurationSeconds = BuildDuration.VeryLong;
            repair.AddStageRequiredItem<ItemIngotSteel>(count: 4);
            repair.AddStageRequiredItem<ItemComponentsElectronic>(count: 2);

            upgrade.AddUpgrade<ObjectLaunchpadStage4>();
            // please note: the upgrade is free but requires finishing the tasks defined below
        }

        protected override void PrepareLaunchpadTasks(LaunchpadTasksList tasksList)
        {
            tasksList.AddTask(TaskBuildLifeSupportSystem,
                              new TextureResource("Misc/LaunchpadTasks/LifeSupportSystem.png"),
                              new InputItems()
                                  .Add<ItemKeinite>(100)
                                  .Add<ItemVialBiomaterial>(100)
                                  .Add<ItemComponentsIndustrialChemicals>(100))
                     .AddTask(TaskBuildNavigationSystem,
                              new TextureResource("Misc/LaunchpadTasks/NavigationSystem.png"),
                              new InputItems()
                                  .Add<ItemComponentsOptical>(50)
                                  .Add<ItemImplantArtificialRetina>(1)
                                  .Add<ItemSolarPanel>(10))
                     .AddTask(TaskBuildAnomalyAvoidanceSystem,
                              new TextureResource("Misc/LaunchpadTasks/AnomalyAvoidanceSystem.png"),
                              new InputItems()
                                  .Add<ItemPragmiumSensor>(1)
                                  .Add<ItemTeleportLocationData>(1)
                                  .Add<ItemOrePragmium>(100));
        }
    }
}
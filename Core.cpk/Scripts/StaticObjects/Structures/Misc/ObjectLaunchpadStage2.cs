namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Reactor;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectLaunchpadStage2 : ProtoObjectLaunchpad
    {
        public const string TaskBuildControlElectronics = "Build control electronics";

        public const string TaskBuildFuelTank = "Build fuel tank";

        public const string TaskBuildPropulsionSystem = "Build propulsion system";

        public override string Description => GetProtoEntity<ObjectLaunchpadStage1>().Description;

        public override string Name => "Launchpad—Stage 2";

        protected override ITextureResource ClientCreateIcon()
        {
            return new TextureResource("StaticObjects/Structures/Misc/ObjectLaunchpad/Icon_Stage2.png");
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            this.ClientAddRenderer(data.GameObject, TextureStage2, TextureStage2Offset);
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

            upgrade.AddUpgrade<ObjectLaunchpadStage3>();
            // please note: the upgrade is free but requires finishing the tasks defined below
        }

        protected override void PrepareLaunchpadTasks(LaunchpadTasksList tasksList)
        {
            tasksList.AddTask(TaskBuildFuelTank,
                              new TextureResource("Misc/LaunchpadTasks/FuelTank.png"),
                              new InputItems()
                                  .Add<ItemIngotSteel>(100)
                                  .Add<ItemPlastic>(100)
                                  .Add<ItemReactorDeflector>(5))
                     .AddTask(TaskBuildPropulsionSystem,
                              new TextureResource("Misc/LaunchpadTasks/PropulsionSystem.png"),
                              new InputItems()
                                  .Add<ItemImpulseEngine>(25)
                                  .Add<ItemPragmiumHeart>(5)
                                  .Add<ItemTyrantHeart>(5))
                     .AddTask(TaskBuildControlElectronics,
                              new TextureResource("Misc/LaunchpadTasks/ControlElectronics.png"),
                              new InputItems()
                                  .Add<ItemComponentsElectronic>(25)
                                  .Add<ItemComponentsHighTech>(25)
                                  .Add<ItemWire>(100));
        }
    }
}
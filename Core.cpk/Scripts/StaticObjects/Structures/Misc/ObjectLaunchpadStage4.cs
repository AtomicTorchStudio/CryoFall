namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Misc
{
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectLaunchpadStage4 : ProtoObjectLaunchpad
    {
        public const string TaskFuelTheRocket = "Fuel the rocket";

        public const string TaskPrepareSupplies = "Prepare supplies";

        public override string Description => GetProtoEntity<ObjectLaunchpadStage1>().Description;

        public override string Name => "Launchpad—Stage 4";

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            this.ClientAddRenderer(data.GameObject, TextureStage4,     TextureStage4Offset);
            this.ClientAddRenderer(data.GameObject, TextureTower,      TextureTowerOffset);
            this.ClientAddRenderer(data.GameObject, TextureTowerMast1, TextureTowerMast1Offset);
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

            upgrade.AddUpgrade<ObjectLaunchpadStage5>();
            // please note: the upgrade is free but requires finishing the tasks defined below
        }

        protected override void PrepareLaunchpadTasks(LaunchpadTasksList tasksList)
        {
            tasksList.AddTask(TaskFuelTheRocket,
                              new TextureResource("Misc/LaunchpadTasks/FuelTheRocket.png"),
                              new InputItems()
                                  .Add<ItemFuelCellPragmium>(5)
                                  .Add<ItemFuelSack>(50)
                                  .Add<ItemSolvent>(100))
                     .AddTask(TaskPrepareSupplies,
                              new TextureResource("Misc/LaunchpadTasks/PrepareSupplies.png"),
                              new InputItems()
                                  .Add<ItemComponentsPharmaceutical>(100)
                                  .Add<ItemBottleWater>(100)
                                  .Add<ItemWheatGrains>(100));
        }
    }
}
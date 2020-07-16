namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Devices;
    using AtomicTorch.CBND.CoreMod.Items.Drones;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Items.Equipment.ApartSuit;
    using AtomicTorch.CBND.CoreMod.Items.Equipment.Assault;
    using AtomicTorch.CBND.CoreMod.Items.Equipment.SuperHeavyArmor;
    using AtomicTorch.CBND.CoreMod.Items.Explosives.Bombs;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Implants;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Special;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectSpaceDebris : ProtoObjectHackableContainer
    {
        public override double HackingStageDuration
            => PveSystem.SharedIsPve(false)
                   ? 2  // 2 seconds per stage in PvE
                   : 4; // 4 seconds per stage in PvP

        public override double HackingStagesNumber
            => PveSystem.SharedIsPve(false)
                   ? 4   // 4 stages in PvE (total 4*2=8 seconds to hack)
                   : 60; // 60 stages in PvP (total 60*4=240 seconds (4 minutes) to hack)

        public override string Name => "Space debris";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Metal;

        public override double ObstacleBlockDamageCoef => 0.5;

        public override float StructurePointsMax => 1000;

        protected override bool CanFlipSprite => true;

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return (this.Layout.Center.X, 0.8);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset = (211 / 256.0, 130 / 256.0);
            renderer.DrawOrderOffsetY = 0.5;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("###",
                         "###");
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureResource("StaticObjects/Misc/Events/ObjectSpaceDebris",
                                       isTransparent: true);
        }

        protected override void PrepareLootDroplist(DropItemsList droplist)
        {
            DropItemConditionDelegate T3Specialized = ServerTechTimeGateHelper.IsAvailableT3Specialized;
            DropItemConditionDelegate T4Specialized = ServerTechTimeGateHelper.IsAvailableT4Specialized;
            DropItemConditionDelegate T5Specialized = ServerTechTimeGateHelper.IsAvailableT5Specialized;

            droplist.Outputs = 3;

            // loot groups are sorted in the order of rarity (more common first)
            // ammo
            droplist.Add(
                    weight: 1,
                    nestedList:
                    new DropItemsList(outputs: 2)
                        .Add<ItemAmmo10mmStandard>(count: 50,         weight: 1)
                        .Add<ItemAmmo10mmHollowPoint>(count: 50,      weight: 1)
                        .Add<ItemAmmo10mmArmorPiercing>(count: 50,    weight: 1)
                        .Add<ItemAmmo12gaPellets>(count: 40,          weight: 1)
                        .Add<ItemAmmo12gaSlugs>(count: 40,            weight: 1)
                        .Add<ItemAmmo300ArmorPiercing>(count: 30,     weight: 1)
                        .Add<ItemAmmo300Incendiary>(count: 30,        weight: 1)
                        .Add<ItemAmmoGrenadeHE>(count: 10,            weight: 1)
                        .Add<ItemAmmoGrenadeIncendiary>(count: 10,    weight: 1)
                        .Add<ItemAmmoGrenadeFreeze>(count: 10,        weight: 1)
                        .Add<ItemAmmoGrenadeFragmentation>(count: 10, weight: 1)
                        .Add<ItemAmmo50SH>(count: 40,                 weight: 1)
                );

            // components and high value items
            droplist.Add(
                    weight: 1,
                    nestedList:
                    new DropItemsList(outputs: 1)
                        // components
                        .Add<ItemComponentsElectronic>(count: 10, weight: 1)
                        .Add<ItemComponentsHighTech>(count: 10,   weight: 1)
                        .Add<ItemComponentsMechanical>(count: 10, weight: 1)
                        .Add<ItemComponentsOptical>(count: 10,    weight: 1)
                        .Add<ItemComponentsWeapon>(count: 10,     weight: 1)
                        // high value items
                        .Add<ItemFuelCellGasoline>(count: 1, weight: 1)
                        .Add<ItemFuelCellPragmium>(count: 1, weight: 1 / 2.0)
                        .Add<ItemSolarPanel>(count: 1,       weight: 1 / 2.0)
                );

            // misc
            droplist.Add(
                    weight: 1,
                    nestedList:
                    new DropItemsList(outputs: 2)
                        // resources / misc
                        .Add<ItemCanisterGasoline>(count: 10,   weight: 1)
                        .Add<ItemCanisterMineralOil>(count: 10, weight: 1)
                        .Add<ItemFirelog>(count: 10,            weight: 1)
                        .Add<ItemBatteryHeavyDuty>(count: 5,    weight: 1)
                        .Add<ItemFertilizer>(count: 5,          weight: 1)
                        .Add<ItemPlastic>(count: 10,            weight: 1)
                        .Add<ItemRubberVulcanized>(count: 10,   weight: 1)
                        // food
                        .Add<ItemMRE>(count: 10, weight: 1)
                        // explosives
                        .Add<ItemBombMining>(count: 20, weight: 1 / 2.0)
                        // medical
                        .Add<ItemHeatPreExposure>(count: 3,  weight: 1)
                        .Add<ItemStrengthBoostBig>(count: 5, weight: 1)
                        .Add<ItemMedkit>(count: 3,           weight: 1)                           // 1 tier lower
                        .Add<ItemStimpack>(count: 3,         weight: 1, condition: T3Specialized) // 1 tier lower
                        .Add<ItemPeredozin>(count: 2,        weight: 1, condition: T4Specialized)
                        .Add<ItemNeuralEnhancer>(count: 1,   weight: 1, condition: T4Specialized)
                );

            // ranged weapons
            droplist.Add(
                    weight: 1 / 2.0,
                    nestedList:
                    new DropItemsList(outputs: 1)
                        // require reaching particular tier before the weapon could be acquired there
                        .Add<ItemHandgun10mm>(count: 1,          weight: 1,       condition: T3Specialized)
                        .Add<ItemSubmachinegun10mm>(count: 1,    weight: 1,       condition: T3Specialized)
                        .Add<ItemRifle10mm>(count: 1,            weight: 1,       condition: T3Specialized)
                        .Add<ItemShotgunMilitary>(count: 1,      weight: 1,       condition: T3Specialized)
                        .Add<ItemGrenadeLauncher>(count: 1,      weight: 1,       condition: T3Specialized)
                        .Add<ItemSteppenHawk>(count: 1,          weight: 1,       condition: T4Specialized)
                        .Add<ItemMachinegun300>(count: 1,        weight: 1 / 2.0, condition: T4Specialized)
                        .Add<ItemRifle300>(count: 1,             weight: 1 / 2.0, condition: T5Specialized)
                        .Add<ItemGrenadeLauncherMulti>(count: 1, weight: 1 / 2.0, condition: T5Specialized)
                );

            // equipment
            droplist.Add(
                    weight: 1 / 2.0,
                    nestedList:
                    new DropItemsList(outputs: 1)
                        .Add<ItemHelmetRespirator>(count: 1,  weight: 1)
                        .Add<ItemHelmetNightVision>(count: 1, weight: 1) // 1 tier lower
                        .Add<ItemMilitaryHelmet>(count: 1,    weight: 1) // 1 tier lower
                        .Add<ItemMilitaryArmor>(count: 1,     weight: 1) // 1 tier lower
                        // advanced stuff
                        .Add<ItemHelmetNightVisionAdvanced>(count: 1,
                                                            weight: 1 / 2.0,
                                                            condition: T3Specialized)                 // 1 tier lower
                        .Add<ItemAssaultHelmet>(count: 1,  weight: 1 / 2.0, condition: T3Specialized) // 1 tier lower
                        .Add<ItemAssaultArmor>(count: 1,   weight: 1 / 2.0, condition: T3Specialized) // 1 tier lower
                        .Add<ItemApartSuit>(count: 1,      weight: 1 / 3.0, condition: T4Specialized)
                        .Add<ItemSuperHeavySuit>(count: 1, weight: 1 / 3.0, condition: T5Specialized)
                );

            // devices & drones
            droplist.Add(
                    weight: 1 / 10.0,
                    nestedList:
                    new DropItemsList(outputs: 1)
                        // devices
                        .Add<ItemPowerBankStandard>(count: 1, weight: 1)
                        .Add<ItemPowerBankLarge>(count: 1,    weight: 1)
                        .Add<ItemPragmiumSensor>(count: 1,    weight: 1 / 5.0, condition: T4Specialized)
                        // drones
                        .Add<ItemDroneIndustrialStandard>(count: 1, weight: 1,       condition: T3Specialized)
                        .Add<ItemDroneControlStandard>(count: 1,    weight: 1 / 2.0, condition: T3Specialized)
                        .Add<ItemDroneIndustrialAdvanced>(count: 1, weight: 1 / 2.0, condition: T4Specialized)
                        .Add<ItemDroneControlAdvanced>(count: 1,    weight: 1 / 4.0, condition: T4Specialized)
                );

            // implants
            droplist.Add(
                    weight: 1 / 20.0,
                    nestedList:
                    new DropItemsList(outputs: 1)
                        .Add<ItemImplantArtificialLiver>(count: 1,     weight: 1)
                        .Add<ItemImplantArtificialRetina>(count: 1,    weight: 1)
                        .Add<ItemImplantArtificialStomach>(count: 1,   weight: 1)
                        .Add<ItemImplantATPEnergyExtractor>(count: 1,  weight: 1)
                        .Add<ItemImplantHealingGland>(count: 1,        weight: 1)
                        .Add<ItemImplantMetabolismModulator>(count: 1, weight: 1)
                        .Add<ItemImplantNanofiberSkin>(count: 1,       weight: 1)
                        .Add<ItemImplantReinforcedBones>(count: 1,     weight: 1)
                );
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            if (data.IsFirstTimeInit)
            {
                Server.World.CreateStaticWorldObject<ObjectCrater>(data.GameObject.TilePosition);
            }
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.8, 0.35), offset: (1 + 0.1, 0.65))
                .AddShapeRectangle(size: (0.8, 0.4),  offset: (1 + 0.1, 0.65), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.6, 0.15), offset: (1 + 0.2, 1.3),  group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (0.8, 0.9),  offset: (1 + 0.1, 0.6),  group: CollisionGroups.ClickArea);
        }
    }
}
namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Items.Equipment.Assault;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Melee;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectLootCrateMilitary : ProtoObjectLootContainer
    {
        public override string Name => "Military crate";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Wood;

        public override double ObstacleBlockDamageCoef => 0.5;

        public override float StructurePointsMax => 1000;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset += (0, 0.5);
            renderer.DrawOrderOffsetY = 0.25;
        }

        protected override void PrepareLootDroplist(DropItemsList droplist)
        {
            DropItemConditionDelegate T3Specialized = ServerTechTimeGateHelper.IsAvailableT3Specialized;
            DropItemConditionDelegate T4Specialized = ServerTechTimeGateHelper.IsAvailableT4Specialized;
            DropItemConditionDelegate T5Specialized = ServerTechTimeGateHelper.IsAvailableT5Specialized;

            // common loot
            droplist.Add(nestedList:
                         new DropItemsList(outputs: 2, outputsRandom: 1)
                             // resources, components
                             .Add<ItemBlackpowder>(count: 20,          countRandom: 10, weight: 1 / 1.0)
                             .Add<ItemNitrocellulosePowder>(count: 20, countRandom: 10, weight: 1 / 1.0)
                             .Add<ItemComponentsWeapon>(count: 2,      countRandom: 2,  weight: 1 / 10.0)
                             // ammo
                             .Add<ItemAmmo8mmStandard>(count: 5,       countRandom: 10, weight: 1 / 1.0)
                             .Add<ItemAmmo8mmToxic>(count: 5,          countRandom: 10, weight: 1 / 5.0)
                             .Add<ItemAmmo10mmStandard>(count: 5,      countRandom: 10, weight: 1 / 1.0)
                             .Add<ItemAmmo10mmHollowPoint>(count: 5,   countRandom: 10, weight: 1 / 4.0)
                             .Add<ItemAmmo10mmArmorPiercing>(count: 5, countRandom: 10, weight: 1 / 4.0)
                             .Add<ItemAmmo12gaBuckshot>(count: 5,      countRandom: 10, weight: 1 / 2.0)
                             .Add<ItemAmmo12gaSlugs>(count: 5,         countRandom: 10, weight: 1 / 2.0)
                             .Add<ItemAmmo12gaPellets>(count: 5,       countRandom: 10, weight: 1 / 2.0)
                             .Add<ItemAmmo300ArmorPiercing>(count: 5,  countRandom: 10, weight: 1 / 10.0)
                             .Add<ItemAmmo300Incendiary>(count: 5,     countRandom: 10, weight: 1 / 10.0)
                             .Add<ItemAmmoGrenadeHE>(count: 2,         countRandom: 3,  weight: 1 / 15.0)
                             .Add<ItemAmmoGrenadeIncendiary>(count: 2, countRandom: 3,  weight: 1 / 15.0)
                             .Add<ItemAmmo50SH>(count: 5,              countRandom: 10, weight: 1 / 5.0)
                             // trash ammo
                             .Add<ItemAmmo10mmBlank>(count: 5,         countRandom: 10, weight: 1 / 15.0)
                             .Add<ItemAmmo12gaSaltCharge>(count: 5,    countRandom: 10, weight: 1 / 15.0));

            // rare loot
            droplist.Add(probability: 1 / 10.0,
                         nestedList:
                         new DropItemsList(outputs: 1)
                             // weapons
                             .Add<ItemKnifeIron>(weight: 1 / 1.0)
                             .Add<ItemRevolver8mm>(weight: 1 / 1.0)
                             .Add<ItemLuger>(weight: 1 / 1.0)
                             .Add<ItemMachinePistol>(weight: 1 / 1.0)
                             .Add<ItemShotgunDoublebarreled>(weight: 1 / 1.0)
                             .Add<ItemRifleBoltAction>(weight: 1 / 1.0)
                             .Add<ItemHandgun10mm>(weight: 1 / 2.0,       condition: T3Specialized)
                             .Add<ItemSubmachinegun10mm>(weight: 1 / 4.0, condition: T3Specialized)
                             .Add<ItemRifle10mm>(weight: 1 / 4.0,         condition: T3Specialized)
                             .Add<ItemShotgunMilitary>(weight: 1 / 4.0,   condition: T3Specialized)
                             .Add<ItemGrenadeLauncher>(weight: 1 / 4.0,   condition: T3Specialized)
                             .Add<ItemSteppenHawk>(weight: 1 / 10.0,      condition: T4Specialized)
                             // equipment
                             .Add<ItemMilitaryHelmet>(weight: 1 / 4.0,    condition: T3Specialized)
                             .Add<ItemMilitaryArmor>(weight: 1 / 4.0,     condition: T3Specialized)
                             .Add<ItemAssaultHelmet>(weight: 1 / 10.0,    condition: T4Specialized)
                             .Add<ItemAssaultArmor>(weight: 1 / 10.0,     condition: T4Specialized)
                             // misc stuff for soldiers use :)
                             .Add<ItemCigarNormal>(count: 3,        countRandom: 2, weight: 1 / 5.0)
                             .Add<ItemCigarPremium>(count: 3,       countRandom: 2, weight: 1 / 5.0)
                             .Add<ItemStrengthBoostSmall>(count: 3, countRandom: 2, weight: 1 / 5.0)
                             .Add<ItemStrengthBoostBig>(count: 3,   countRandom: 2, weight: 1 / 5.0)
                             .Add<ItemMRE>(count: 1,                countRandom: 2, weight: 1 / 5.0)
                );

            // extra loot from skill
            droplist.Add(condition: SkillSearching.ServerRollExtraLoot,
                         nestedList:
                         new DropItemsList(outputs: 1)
                             .Add<ItemAmmo8mmStandard>(count: 2,       countRandom: 2)
                             .Add<ItemAmmo10mmStandard>(count: 2,      countRandom: 2)
                             .Add<ItemAmmo10mmHollowPoint>(count: 2,   countRandom: 2)
                             .Add<ItemAmmo10mmArmorPiercing>(count: 2, countRandom: 2)
                             .Add<ItemAmmo12gaSlugs>(count: 2,         countRandom: 2)
                             .Add<ItemAmmo12gaPellets>(count: 2,       countRandom: 2));
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(size: (0.8, 0.45), offset: (0.1, 0.55))
                .AddShapeRectangle(size: (0.8, 0.5),  offset: (0.1, 0.55), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.6, 0.15), offset: (0.2, 1.3),  group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (0.8, 0.8),  offset: (0.1, 0.55), group: CollisionGroups.ClickArea);
        }
    }
}
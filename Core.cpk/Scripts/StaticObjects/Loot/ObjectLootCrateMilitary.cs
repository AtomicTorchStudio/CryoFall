namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Items.Equipment.Assault;
    using AtomicTorch.CBND.CoreMod.Items.Explosives.Bombs;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Melee;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged;
    using AtomicTorch.CBND.CoreMod.Skills;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectLootCrateMilitary : ProtoObjectLootContainer
    {
        public override string Name => "Military crate";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Wood;

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
                             // trash ammo
                             .Add<ItemAmmo10mmBlank>(count: 5,      countRandom: 10, weight: 1 / 15.0)
                             .Add<ItemAmmo12gaSaltCharge>(count: 5, countRandom: 10, weight: 1 / 15.0));

            // rare loot
            droplist.Add(probability: 1 / 10.0,
                         nestedList:
                         new DropItemsList(outputs: 1)
                             // resources
                             .Add<ItemExplosives>(count: 25, countRandom: 25, weight: 1 / 4.0)
                             .Add<ItemBombPrimitive>(weight: 1 / 5.0)
                             .Add<ItemBombModern>(weight: 1 / 10.0)
                             // weapons
                             .Add<ItemKnifeIron>(weight: 1 / 1.0)
                             .Add<ItemRevolver8mm>(weight: 1 / 1.0)
                             .Add<ItemLuger>(weight: 1 / 1.0)
                             .Add<ItemMachinePistol>(weight: 1 / 1.0)
                             .Add<ItemHandgun10mm>(weight: 1 / 2.0)
                             .Add<ItemShotgunDoublebarreled>(weight: 1 / 1.0)
                             .Add<ItemSubmachinegun10mm>(weight: 1 / 4.0)
                             .Add<ItemShotgunMilitary>(weight: 1 / 4.0)
                             // equipment
                             .Add<ItemMilitaryHelmet>(weight: 1 / 4.0)
                             .Add<ItemMilitaryJacket>(weight: 1 / 4.0)
                             .Add<ItemMilitaryPants>(weight: 1 / 4.0)
                             .Add<ItemAssaultHelmet>(weight: 1 / 10.0)
                             .Add<ItemAssaultJacket>(weight: 1 / 10.0)
                             .Add<ItemAssaultPants>(weight: 1 / 10.0)
                             // misc stuff for soldiers use :)
                             .Add<ItemCigarNormal>(count: 3,        countRandom: 2, weight: 1 / 5.0)
                             .Add<ItemCigarPremium>(count: 3,       countRandom: 2, weight: 1 / 5.0)
                             .Add<ItemStrengthBoostSmall>(count: 3, countRandom: 2, weight: 1 / 5.0)
                             .Add<ItemStrengthBoostBig>(count: 3,   countRandom: 2, weight: 1 / 5.0)
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
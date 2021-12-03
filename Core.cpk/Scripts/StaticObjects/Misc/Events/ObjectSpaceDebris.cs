namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Events;
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
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Melee;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;
    using static Technologies.ServerTechTimeGateHelper;

    public class ObjectSpaceDebris : ProtoObjectHackableContainer
    {
        private const int WorldOffsetY = 2;

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
            return (this.Layout.Center.X, WorldOffsetY + 0.8);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.PositionOffset = (211 / 256.0, WorldOffsetY + 130 / 256.0);
            renderer.DrawOrderOffsetY += 0.5;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            // extra space below is used to ensure that the object will be not obscured by trees
            layout.Setup("###",
                         "###",
                         "###",
                         "###");
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureResource("StaticObjects/Misc/Events/ObjectSpaceDebris",
                                       isTransparent: true);
        }

        protected override void PrepareLootDroplist(DropItemsList droplist)
        {
            if (IsClient)
            {
                return;
            }

            if (PveSystem.ServerIsPvE)
            {
                PrepareDroplistPvE(droplist);
            }
            else
            {
                PrepareDroplistPvP(droplist);
            }
        }

        protected override double ServerGetDropListProbabilityMultiplier(IStaticWorldObject staticWorldObject)
        {
            return RateResourcesGatherSpaceDebris.SharedValue;
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            if (data.IsFirstTimeInit
                && !data.GameObject.OccupiedTile.StaticObjects
                        .Any(o => o.ProtoGameObject is ObjectCrater))
            {
                Server.World.CreateStaticWorldObject<ObjectCrater>(
                    (data.GameObject.TilePosition + (0, WorldOffsetY)).ToVector2Ushort());
            }
        }

        protected override void ServerOnHacked(ICharacter character, IStaticWorldObject worldObject)
        {
            PlayerCharacter.GetPrivateState(character)
                           .CompletionistData
                           .ServerOnParticipatedInEvent(Api.GetProtoEntity<EventSpaceDrop>());
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            var y = WorldOffsetY;
            data.PhysicsBody
                .AddShapeRectangle(size: (0.8, 0.35), offset: (1 + 0.1, y + 0.65))
                .AddShapeRectangle(size: (0.8, 0.4),  offset: (1 + 0.1, y + 0.65), group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size: (0.6, 0.15), offset: (1 + 0.2, y + 1.3),  group: CollisionGroups.HitboxRanged)
                .AddShapeRectangle(size: (0.8, 0.9),  offset: (1 + 0.1, y + 0.6),  group: CollisionGroups.ClickArea);
        }

        private static void PrepareDroplistPvE(DropItemsList droplist)
        {
            droplist.Outputs = 3;

            // loot groups are sorted in the order of rarity (more common first)
            // ammo
            droplist.Add(
                    weight: 1,
                    nestedList:
                    new DropItemsList(outputs: 2)
                        .Add<ItemAmmo8mmStandard>(count: 50,          weight: 1)
                        .Add<ItemAmmo8mmToxic>(count: 50,             weight: 1)
                        .Add<ItemAmmo12gaBuckshot>(count: 50,         weight: 1)
                        .Add<ItemAmmo10mmStandard>(count: 50,         weight: 1)
                        .Add<ItemAmmo10mmHollowPoint>(count: 50,      weight: 1)
                        .Add<ItemAmmo10mmArmorPiercing>(count: 50,    weight: 1)
                        .Add<ItemAmmo12gaPellets>(count: 40,          weight: 1)
                        .Add<ItemAmmo12gaSlugs>(count: 40,            weight: 1)
                        .Add<ItemAmmo300ArmorPiercing>(count: 30,     weight: 1)
                        .Add<ItemAmmo300Incendiary>(count: 30,        weight: 1)
                        .Add<ItemAmmo50SH>(count: 40,                 weight: 1)
                        .Add<ItemAmmoGrenadeHE>(count: 10,            weight: 1 / 2.0)
                        .Add<ItemAmmoGrenadeIncendiary>(count: 10,    weight: 1 / 2.0)
                        .Add<ItemAmmoGrenadeFragmentation>(count: 10, weight: 1 / 2.0)
                        .Add<ItemAmmoGrenadeFreeze>(count: 10,        weight: 1 / 3.0)
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
                        .Add<ItemFirelog>(count: 20,            countRandom: 20, weight: 1)
                        .Add<ItemBatteryHeavyDuty>(count: 5,    weight: 1)
                        .Add<ItemPlastic>(count: 10,            weight: 1)
                        .Add<ItemRubberVulcanized>(count: 10,   weight: 1)
                        // food
                        .Add<ItemMRE>(count: 10, weight: 1)
                        // medical
                        .Add<ItemHeatPreExposure>(count: 3,  weight: 1)
                        .Add<ItemStrengthBoostBig>(count: 5, weight: 1)
                        .Add<ItemMedkit>(count: 3,           weight: 1)
                        .Add<ItemStimpack>(count: 3,         weight: 1)
                        .Add<ItemPeredozin>(count: 2,        weight: 1)
                        .Add<ItemNeuralEnhancer>(count: 1,   weight: 1)
                        // misc
                        .Add<ItemCigarettes>(count: 5,  weight: 5)
                        .Add<ItemBombMining>(count: 20, weight: 1 / 2.0)
                );

            // weapons
            droplist.Add(
                    weight: 1 / 2.0,
                    nestedList:
                    new DropItemsList(outputs: 1)
                        // melee
                        .Add<ItemStunBaton>(count: 1, weight: 1 / 2.0)
                        // ranged
                        .Add<ItemSubmachinegun10mm>(count: 1,    weight: 1)
                        .Add<ItemRifle10mm>(count: 1,            weight: 1)
                        .Add<ItemShotgunMilitary>(count: 1,      weight: 1)
                        .Add<ItemSteppenHawk>(count: 1,          weight: 1)
                        .Add<ItemGrenadeLauncher>(count: 1,      weight: 1 / 2.0)
                        .Add<ItemMachinegun300>(count: 1,        weight: 1 / 2.0)
                        .Add<ItemRifle300>(count: 1,             weight: 1 / 2.0)
                        .Add<ItemGrenadeLauncherMulti>(count: 1, weight: 1 / 4.0)
                );

            // equipment
            droplist.Add(
                    weight: 1 / 2.0,
                    nestedList:
                    new DropItemsList(outputs: 1)
                        .Add<ItemHelmetMiner>(count: 1,       weight: 1)
                        .Add<ItemHelmetRespirator>(count: 1,  weight: 1)
                        .Add<ItemHelmetNightVision>(count: 1, weight: 1)
                        .Add<ItemMilitaryHelmet>(count: 1,    weight: 1)
                        .Add<ItemMilitaryArmor>(count: 1,     weight: 1)
                        // advanced stuff
                        .Add<ItemHelmetNightVisionAdvanced>(count: 1, weight: 1 / 2.0)
                        .Add<ItemAssaultHelmet>(count: 1,             weight: 1 / 2.0)
                        .Add<ItemAssaultArmor>(count: 1,              weight: 1 / 2.0)
                        .Add<ItemApartSuit>(count: 1,                 weight: 1 / 3.0)
                        .Add<ItemSuperHeavySuit>(count: 1,            weight: 1 / 3.0)
                );

            // devices & drones
            droplist.Add(
                    weight: 1 / 10.0,
                    nestedList:
                    new DropItemsList(outputs: 1)
                        // devices
                        .Add<ItemPowerBankStandard>(count: 1, weight: 1)
                        .Add<ItemPowerBankLarge>(count: 1,    weight: 1)
                        .Add<ItemPragmiumSensor>(count: 1,    weight: 1 / 5.0)
                        // drones
                        .Add<ItemDroneIndustrialAdvanced>(count: 1, weight: 1)
                        .Add<ItemDroneControlAdvanced>(count: 1,    weight: 1 / 2.0)
                );

            // implants
            droplist.Add(
                    weight: 1 / 20.0,
                    nestedList:
                    new DropItemsList(outputs: 1)
                        .Add<ItemImplantArtificialLiver>()
                        .Add<ItemImplantArtificialRetina>()
                        .Add<ItemImplantArtificialStomach>()
                        .Add<ItemImplantATPEnergyExtractor>()
                        .Add<ItemImplantHealingGland>()
                        .Add<ItemImplantMetabolismModulator>()
                        .Add<ItemImplantNanofiberSkin>()
                        .Add<ItemImplantReinforcedBones>()
                        .Add<ItemImplantToxinFiltration>()
                );
        }

        private static void PrepareDroplistPvP(DropItemsList droplist)
        {
            // tech tier filters
            DropItemConditionDelegate beforeT3
                = context => !IsAvailableT3Specialized(context);

            DropItemConditionDelegate T3Only
                = context => IsAvailableT3Specialized(context)
                             && !IsAvailableT4Specialized(context);

            DropItemConditionDelegate T3Plus = IsAvailableT3Specialized;

            DropItemConditionDelegate T3BeforeT5
                = context => IsAvailableT3Specialized(context)
                             && !IsAvailableT5Specialized(context);

            DropItemConditionDelegate beforeT4
                = context => !IsAvailableT4Specialized(context);

            DropItemConditionDelegate T4Basic = IsAvailableT4Basic;

            DropItemConditionDelegate T4Only
                = context => IsAvailableT4Specialized(context)
                             && !IsAvailableT5Specialized(context);

            DropItemConditionDelegate T4Plus = IsAvailableT4Specialized;

            DropItemConditionDelegate T5 = IsAvailableT5Specialized;

            droplist.Outputs = 3;

            // loot groups are sorted in the order of rarity (more common first)
            // ammo
            droplist.Add(
                    weight: 1,
                    nestedList:
                    new DropItemsList(outputs: 2)
                        .Add<ItemAmmo8mmStandard>(count: 50,          weight: 1,       condition: beforeT3)
                        .Add<ItemAmmo8mmToxic>(count: 50,             weight: 1,       condition: beforeT3)
                        .Add<ItemAmmo12gaBuckshot>(count: 50,         weight: 1,       condition: beforeT3)
                        .Add<ItemAmmo10mmStandard>(count: 100,        weight: 1,       condition: T3Only)
                        .Add<ItemAmmo10mmHollowPoint>(count: 100,     weight: 1,       condition: T3Only)
                        .Add<ItemAmmo10mmArmorPiercing>(count: 100,   weight: 1,       condition: T3Only)
                        .Add<ItemAmmo12gaPellets>(count: 50,          weight: 1,       condition: T3Only)
                        .Add<ItemAmmo12gaSlugs>(count: 50,            weight: 1,       condition: T3Only)
                        .Add<ItemAmmo300ArmorPiercing>(count: 100,    weight: 1,       condition: T4Plus)
                        .Add<ItemAmmo300Incendiary>(count: 100,       weight: 1,       condition: T4Plus)
                        .Add<ItemAmmoGrenadeHE>(count: 10,            weight: 1 / 2.0, condition: T3Plus)
                        .Add<ItemAmmoGrenadeIncendiary>(count: 10,    weight: 1 / 2.0, condition: T4Plus)
                        .Add<ItemAmmoGrenadeFragmentation>(count: 10, weight: 1 / 2.0, condition: T4Plus)
                        .Add<ItemAmmoGrenadeFreeze>(count: 10,        weight: 1 / 2.0, condition: T5)
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
                        .Add<ItemFuelCellGasoline>(count: 1, weight: 1,       condition: T3Plus)
                        .Add<ItemSolarPanel>(count: 1,       weight: 1 / 2.0, condition: T4Plus)
                );

            // misc
            droplist.Add(
                    weight: 1,
                    nestedList:
                    new DropItemsList(outputs: 2)
                        // resources / misc
                        .Add<ItemCanisterGasoline>(count: 10,   weight: 1,       condition: T3Plus)
                        .Add<ItemCanisterMineralOil>(count: 10, weight: 1,       condition: T3Plus)
                        .Add<ItemFirelog>(count: 20,            countRandom: 20, weight: 1)
                        .Add<ItemBatteryHeavyDuty>(count: 5,    weight: 1,       condition: T3Plus)
                        .Add<ItemPlastic>(count: 10,            weight: 1)
                        .Add<ItemRubberVulcanized>(count: 20,   weight: 1)
                        // food
                        .Add<ItemMRE>(count: 10, weight: 1)
                        // medical
                        .Add<ItemHeatPreExposure>(count: 3,  weight: 1, condition: T3Plus)
                        .Add<ItemStrengthBoostBig>(count: 5, weight: 1)
                        .Add<ItemMedkit>(count: 3,           weight: 1, condition: T3Plus)
                        .Add<ItemStimpack>(count: 3,         weight: 1, condition: T4Plus)
                        .Add<ItemPeredozin>(count: 2,        weight: 1, condition: T4Plus)
                        .Add<ItemNeuralEnhancer>(count: 1,   weight: 1, condition: T4Plus)
                        // misc
                        .Add<ItemCigarettes>(count: 5,  weight: 5,       condition: T3Plus)
                        .Add<ItemBombMining>(count: 20, weight: 1 / 2.0, condition: T3Plus)
                );

            // weapons
            droplist.Add(
                    weight: 1 / 2.0,
                    // require reaching particular tier before the weapon could be acquired there
                    condition: T3Plus,
                    nestedList:
                    new DropItemsList(outputs: 1)
                        // melee
                        .Add<ItemStunBaton>(count: 1, weight: 1 / 2.0, condition: T3BeforeT5)
                        // ranged
                        .Add<ItemSubmachinegun10mm>(count: 1,    weight: 1,       condition: T3Only)
                        .Add<ItemRifle10mm>(count: 1,            weight: 1,       condition: T3Only)
                        .Add<ItemShotgunMilitary>(count: 1,      weight: 1,       condition: T3Only)
                        .Add<ItemMachinegun300>(count: 1,        weight: 1,       condition: T4Plus)
                        .Add<ItemLaserRifle>(count: 1,           weight: 1 / 1.5, condition: T4Plus)
                        .Add<ItemGrenadeLauncher>(count: 1,      weight: 1,       condition: T3BeforeT5)
                        .Add<ItemRifle300>(count: 1,             weight: 1 / 2.0, condition: T5)
                        .Add<ItemGrenadeLauncherMulti>(count: 1, weight: 1 / 2.0, condition: T5)
                );

            // equipment
            droplist.Add(
                    weight: 1 / 2.0,
                    nestedList:
                    new DropItemsList(outputs: 1)
                        .Add<ItemLeatherArmor>(count: 1,              condition: beforeT3)
                        .Add<ItemFurArmor>(count: 1,                  condition: beforeT3)
                        .Add<ItemHelmetMiner>(count: 1,               condition: beforeT3)
                        .Add<ItemHelmetRespirator>(count: 1,          condition: beforeT4)
                        .Add<ItemHelmetNightVision>(count: 1,         condition: T3Only)
                        .Add<ItemMetalHelmetClosed>(count: 1,         condition: T3Only, weight: 1 / 2.0)
                        .Add<ItemMetalHelmetSkull>(count: 1,          condition: T3Only, weight: 1 / 2.0)
                        .Add<ItemMetalArmor>(count: 1,                condition: T3Only)
                        .Add<ItemMilitaryHelmet>(count: 1,            condition: T3Only)
                        .Add<ItemMilitaryArmor>(count: 1,             condition: T3Only)
                        .Add<ItemHelmetNightVisionAdvanced>(count: 1, condition: T4Plus)
                        .Add<ItemAssaultHelmet>(count: 1,             condition: T4Plus)
                        .Add<ItemAssaultArmor>(count: 1,              condition: T4Plus)
                        .Add<ItemApartSuit>(count: 1,                 condition: T4Plus)
                        .Add<ItemSuperHeavySuit>(count: 1,            condition: T5, weight: 1 / 2.0)
                );

            // devices, drones, mech parts
            droplist.Add(
                    weight: 1 / 10.0,
                    condition: T3Plus,
                    nestedList:
                    new DropItemsList(outputs: 1)
                        // devices
                        .Add<ItemPowerBankStandard>(count: 1, weight: 1)
                        .Add<ItemPowerBankLarge>(count: 1,    weight: 1)
                        .Add<ItemPragmiumSensor>(count: 1,    weight: 1 / 5.0, condition: T4Basic)
                        // drones
                        .Add<ItemDroneIndustrialStandard>(count: 1, weight: 1,       condition: T3Only)
                        .Add<ItemDroneControlStandard>(count: 1,    weight: 1 / 2.0, condition: T3Only)
                        .Add<ItemDroneIndustrialAdvanced>(count: 1, weight: 1,       condition: T4Basic)
                        .Add<ItemDroneControlAdvanced>(count: 1,    weight: 1 / 2.0, condition: T4Basic)
                        // mech parts
                        .Add<ItemStructuralPlating>(count: 2, countRandom: 1, weight: 2, condition: T5)
                        .Add<ItemUniversalActuator>(count: 1, weight: 2,      condition: T5)
                        .Add<ItemImpulseEngine>(count: 1,     weight: 2,      condition: T5)
                );

            // bombs starting from T3 (please note: this is a droplist for PvP)
            droplist.Add(
                    weight: 1 / 25.0,
                    condition: T3Plus,
                    nestedList:
                    new DropItemsList(outputs: 1)
                        .Add<ItemBombPrimitive>(count: 1, condition: T3Only)
                        .Add<ItemBombModern>(count: 1,    condition: T4Plus)
                );

            // implants
            droplist.Add(
                    weight: 1 / 20.0,
                    // drop it a tier earlier (useful as a bonus)
                    condition: T3Plus,
                    nestedList:
                    new DropItemsList(outputs: 1)
                        .Add<ItemImplantArtificialLiver>()
                        .Add<ItemImplantArtificialRetina>()
                        .Add<ItemImplantArtificialStomach>()
                        .Add<ItemImplantATPEnergyExtractor>()
                        .Add<ItemImplantHealingGland>()
                        .Add<ItemImplantMetabolismModulator>()
                        .Add<ItemImplantNanofiberSkin>()
                        .Add<ItemImplantReinforcedBones>()
                        .Add<ItemImplantToxinFiltration>()
                );
        }
    }
}
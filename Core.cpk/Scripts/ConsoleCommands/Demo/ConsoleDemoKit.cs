// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Demo
{
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Devices;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Implants;
    using AtomicTorch.CBND.CoreMod.Items.Medical;
    using AtomicTorch.CBND.CoreMod.Items.Seeds;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Axes;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Crowbars;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Pickaxes;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Toolboxes;
    using AtomicTorch.CBND.CoreMod.Items.Tools.WateringCans;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Melee;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ConsoleDemoKit : BaseConsoleCommand
    {
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum KitCategory
        {
            // resources / prefix "r"
            rBasic,

            // weapons & ammo / prefix "w"
            w8mm,

            w10mm,

            w12ga,

            w300,

            wEnergy,

            wMelee,

            // tools, prefix "t"
            tToolsAll,

            // items, prefix i
            iMedicine,

            iImplants,

            iFarming
        }

        public override string Alias => "kit";

        public override string Description =>
            "Adds sets of items according to specified category to the player character.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "demo.kit";

        public string Execute(
            KitCategory category = KitCategory.rBasic,
            [CurrentCharacterIfNull] ICharacter player = null)
        {
            switch (category)
            {
                case KitCategory.rBasic:
                    CreateItem<ItemLogs>();
                    CreateItem<ItemStone>();
                    CreateItem<ItemPlanks>();
                    CreateItem<ItemTwigs>();
                    CreateItem<ItemIngotCopper>();
                    CreateItem<ItemIngotIron>();
                    CreateItem<ItemIngotSteel>();
                    CreateItem<ItemFibers>();
                    CreateItem<ItemRope>();
                    return "Added basic resources.";

                case KitCategory.w8mm:
                    CreateItem<ItemLuger>();
                    CreateItem<ItemRevolver8mm>();
                    CreateItem<ItemMachinePistol>();
                    CreateItem<ItemAmmo8mmStandard>();
                    CreateItem<ItemAmmo8mmToxic>();
                    return "Added 8mm arms and ammo.";

                case KitCategory.w10mm:
                    CreateItem<ItemHandgun10mm>();
                    CreateItem<ItemSubmachinegun10mm>();
                    CreateItem<ItemAmmo10mmStandard>();
                    CreateItem<ItemAmmo10mmArmorPiercing>();
                    CreateItem<ItemAmmo10mmHollowPoint>();
                    CreateItem<ItemAmmo10mmBlank>();
                    return "Added 10mm arms and ammo.";

                case KitCategory.w12ga:
                    CreateItem<ItemShotgunDoublebarreled>();
                    CreateItem<ItemShotgunMilitary>();
                    CreateItem<ItemAmmo12gaPellets>();
                    CreateItem<ItemAmmo12gaSlugs>();
                    CreateItem<ItemAmmo12gaBuckshot>();
                    CreateItem<ItemAmmo12gaSaltCharge>();
                    return "Added 12ga arms and ammo.";

                case KitCategory.w300:
                    CreateItem<ItemMachinegun300>();
                    CreateItem<ItemRifle300>();
                    CreateItem<ItemAmmo300ArmorPiercing>();
                    CreateItem<ItemAmmo300Incendiary>();
                    return "Added .300 arms and ammo.";

                case KitCategory.wEnergy:
                    CreateItem<ItemLaserPistol>();
                    CreateItem<ItemLaserRifle>();
                    CreateItem<ItemPlasmaPistol>();
                    CreateItem<ItemPlasmaRifle>();
                    CreateItem<ItemPowerBankLarge>();
                    CreateItem<ItemBatteryDisposable>(amount: 100);
                    return "Added energy weapons and related items.";

                case KitCategory.wMelee:
                    CreateItem<ItemKnifeStone>();
                    CreateItem<ItemKnifeIron>();
                    CreateItem<ItemMaceCopper>();
                    CreateItem<ItemMaceIron>();
                    CreateItem<ItemRapierLaserBlue>();
                    CreateItem<ItemRapierLaserGreen>();
                    CreateItem<ItemRapierLaserPurple>();
                    CreateItem<ItemRapierLaserRed>();
                    CreateItem<ItemRapierLaserWhite>();
                    CreateItem<ItemRapierLaserYellow>();
                    CreateItem<ItemPowerBankLarge>();
                    CreateItem<ItemBatteryDisposable>();
                    return "Added 12ga arms and ammo.";

                case KitCategory.iMedicine:
                    CreateItem<ItemAntiNausea>();
                    CreateItem<ItemAntiRadiation>();
                    CreateItem<ItemAntiRadiationPreExposure>();
                    CreateItem<ItemAntiToxin>();
                    CreateItem<ItemAntiToxinPreExposure>();
                    CreateItem<ItemBandage>();
                    CreateItem<ItemHeatPreExposure>();
                    CreateItem<ItemHemostatic>();
                    CreateItem<ItemHerbGreen>();
                    CreateItem<ItemHerbPurple>();
                    CreateItem<ItemHerbRed>();
                    CreateItem<ItemMedkit>();
                    CreateItem<ItemNeuralEnhancer>();
                    CreateItem<ItemPainkiller>();
                    CreateItem<ItemPeredozin>();
                    CreateItem<ItemPsiPreExposure>();
                    CreateItem<ItemRemedyHerbal>();
                    CreateItem<ItemSplint>();
                    CreateItem<ItemStimpack>();
                    CreateItem<ItemStrengthBoostBig>();
                    CreateItem<ItemStrengthBoostSmall>();
                    return "Added medicine.";

                case KitCategory.iImplants:
                    CreateItem<ItemImplantArtificialRetina>();
                    CreateItem<ItemImplantArtificialStomach>();
                    CreateItem<ItemImplantATPEnergyExtractor>();
                    CreateItem<ItemImplantHealingGland>();
                    CreateItem<ItemImplantMetabolismModulator>();
                    CreateItem<ItemImplantNanofiberSkin>();
                    CreateItem<ItemVialBiomaterial>();
                    CreateItem<ItemVialEmpty>();
                    return "Added implants and related items.";

                case KitCategory.iFarming:
                    CreateItem<ItemWateringCanWood>();
                    CreateItem<ItemWateringCanCopper>();
                    CreateItem<ItemWateringCanSteel>();
                    CreateItem<ItemWateringCanPlastic>();
                    CreateItem<ItemBottleWater>(100);
                    CreateItem<ItemSeedsBellPepper>();
                    CreateItem<ItemSeedsCarrot>();
                    CreateItem<ItemSeedsChiliPepper>();
                    CreateItem<ItemSeedsCorn>();
                    CreateItem<ItemSeedsCucumber>();
                    CreateItem<ItemSeedsFlowerBlueSage>();
                    CreateItem<ItemSeedsFlowerOni>();
                    CreateItem<ItemSeedsMilkmelon>();
                    CreateItem<ItemSeedsTobacco>();
                    CreateItem<ItemSeedsTomato>();
                    CreateItem<ItemSeedsWheat>();
                    return "Added implants and related items.";

                case KitCategory.tToolsAll:
                    CreateItem<ItemAxeStone>();
                    CreateItem<ItemAxeIron>();
                    CreateItem<ItemAxeSteel>();
                    CreateItem<ItemPickaxeStone>();
                    CreateItem<ItemPickaxeIron>();
                    CreateItem<ItemPickaxeSteel>();
                    CreateItem<ItemToolboxT1>();
                    CreateItem<ItemToolboxT2>();
                    CreateItem<ItemToolboxT3>();
                    CreateItem<ItemCrowbar>();
                    return "Added all basic tools.";

                default:
                    return "Incorrect argument given.";
            }

            void CreateItem<TProtoItem>(ushort amount = 0)
                where TProtoItem : class, IProtoItem, new()
            {
                var item = GetProtoEntity<TProtoItem>();
                amount = amount == 0
                             ? item.MaxItemsPerStack
                             : amount;

                Server.Items.CreateItem(player, item, amount);
            }
        }
    }
}
// ReSharper disable CanExtractXamlLocalizableStringCSharp

namespace AtomicTorch.CBND.CoreMod.ConsoleCommands.Admin
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Devices;
    using AtomicTorch.CBND.CoreMod.Items.Explosives.Bombs;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Axes;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Crowbars;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Lights;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Pickaxes;
    using AtomicTorch.CBND.CoreMod.Items.Tools.Toolboxes;
    using AtomicTorch.CBND.CoreMod.Items.Tools.WateringCans;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Melee;
    using AtomicTorch.CBND.CoreMod.Items.Weapons.Ranged;
    using AtomicTorch.CBND.CoreMod.Systems.Console;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ConsoleAdminPwn : BaseConsoleCommand
    {
        public override string Alias => "pwn";

        public override string Description => "Special debug command to add useful items to the player character.";

        public override ConsoleCommandKinds Kind => ConsoleCommandKinds.ServerOperator;

        public override string Name => "admin.pwn";

        public string Execute([CurrentCharacterIfNull] ICharacter player = null)
        {
            var characterPrivateState = PlayerCharacter.GetPrivateState(player);
            var characterPublicState = PlayerCharacter.GetPublicState(player);

            var containerHotbar = characterPrivateState.ContainerHotbar;
            var containerEquipment = characterPublicState.ContainerEquipment;

            // Hotbar items
            Server.Items.CreateItem<ItemKnifeIron>(container: containerHotbar, count: 1);
            Server.Items.CreateItem<ItemSubMachinegun10mm>(container: containerHotbar, count: 1);
            Server.Items.CreateItem<ItemPickaxeSteel>(container: containerHotbar, count: 1);
            Server.Items.CreateItem<ItemAxeSteel>(container: containerHotbar, count: 1);
            Server.Items.CreateItem<ItemToolboxT3>(container: containerHotbar, count: 1);
            Server.Items.CreateItem<ItemCrowbar>(container: containerHotbar, count: 1);
            Server.Items.CreateItem<ItemWateringCanWood>(container: containerHotbar, count: 1);
            Server.Items.CreateItem<ItemFlashlight>(container: containerHotbar, count: 1);

            // Inventory items
            CreateItem(player, GetProtoEntity<ItemAmmo10mmStandard>());
            CreateItem(player, GetProtoEntity<ItemAmmo10mmArmorPiercing>());
            CreateItem(player, GetProtoEntity<ItemAmmo10mmHollowPoint>());
            CreateItem(player, GetProtoEntity<ItemBatteryDisposable>(), 10);
            CreateItem(player, GetProtoEntity<ItemBombModern>(),       10);
            CreateItem(player, GetProtoEntity<ItemPlanks>(),           200);
            CreateItem(player, GetProtoEntity<ItemCanisterGasoline>(), 100);
            CreateItem(player, GetProtoEntity<ItemFuelCellPragmium>(), 2);

            // Equipment items
            //Server.Items.CreateItem<ItemClothShirt>(container: containerEquipment, slotId: (byte?)EquipmentType.Chest);
            //Server.Items.CreateItem<ItemClothHat>(container: containerEquipment, slotId: (byte?)EquipmentType.Head);
            //Server.Items.CreateItem<ItemClothPants>(container: containerEquipment, slotId: (byte?)EquipmentType.Legs);
            Server.Items.CreateItem<ItemPowerBankLarge>(container: containerEquipment);

            return "Added some test items to " + player;
        }

        private static void CreateItem(
            ICharacter player,
            IProtoItem item,
            uint count = 0,
            bool atHotbar = false)
        {
            if (atHotbar)
            {
                // try to add at hotbar
                var result = Server.Items.CreateItem(protoItem: item,
                                                     container: player.SharedGetPlayerContainerHotbar(),
                                                     count: count == 0 ? (uint)item.MaxItemsPerStack : count);
                if (result.IsEverythingCreated)
                {
                    return;
                }

                result.Rollback();
            }

            Server.Items.CreateItem(item,
                                    player,
                                    count: count == 0
                                               ? item.MaxItemsPerStack
                                               : count);
        }
    }
}
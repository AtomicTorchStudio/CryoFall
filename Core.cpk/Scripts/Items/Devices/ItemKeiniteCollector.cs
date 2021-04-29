namespace AtomicTorch.CBND.CoreMod.Items.Devices
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ItemKeiniteCollector : ProtoItemEquipmentDevice
    {
        private const int DurabilityDecreasePerUse = 1;

        private static IProtoItem protoItemKeiniteCollector;

        public override string Description =>
            "Special device that is able to extract low-grade keinite from certain native lifeforms and mutated animals.";

        public override uint DurabilityMax => 300;

        public override string Name => "Keinite extractor";

        public override bool OnlySingleDeviceOfThisProtoAppliesEffect => true;

        public static bool ConditionHasDeviceEquipped(DropItemContext context)
        {
            // Please note: checking this condition will also automatically deduct the device's durability.
            if (!context.HasCharacter)
            {
                return false;
            }

            // find the device
            var itemDevice = context.Character.SharedGetPlayerContainerEquipment()
                                    .GetItemsOfProto(protoItemKeiniteCollector)
                                    .FirstOrDefault();
            if (itemDevice is null)
            {
                // don't have an equipped device
                return false;
            }

            ItemDurabilitySystem.ServerModifyDurability(itemDevice, -DurabilityDecreasePerUse);
            return true;
        }

        protected override void PrepareProtoItemEquipment()
        {
            protoItemKeiniteCollector = this;
        }
    }
}
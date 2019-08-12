namespace AtomicTorch.CBND.CoreMod.Items.Devices
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.Systems.Resources;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;

    public class ItemHuntersTools : ProtoItemEquipmentDevice
    {
        private const int DurabilityDecreasePerUse = 1;

        public override string Description =>
            "Bundle of useful hunter's tools makes looting any creature much quicker.";

        public override uint DurabilityMax => 100;

        public override string Name => "Hunter's tools";

        public override bool OnlySingleDeviceOfThisProtoAppliesEffect => true;

        protected override void PrepareEffects(Effects effects)
        {
            effects.AddPercent(this, StatName.HuntingLootingSpeed, 30);
        }

        protected override void PrepareProtoItemEquipment()
        {
            base.PrepareProtoItemEquipment();

            if (IsServer)
            {
                GatheringSystem.ServerOnGather += this.ServerGatheringSystemGatherHandler;
            }
        }

        private void ServerGatheringSystemGatherHandler(ICharacter character, IStaticWorldObject worldObject)
        {
            if (!(worldObject.ProtoStaticWorldObject is ObjectCorpse))
            {
                return;
            }

            // corpse looted
            var itemDevice = character.SharedGetPlayerContainerEquipment()
                                      .GetItemsOfProto(this)
                                      .FirstOrDefault();
            if (itemDevice == null)
            {
                // don't have an equipped device
                return;
            }

            ItemDurabilitySystem.ServerModifyDurability(itemDevice, -DurabilityDecreasePerUse);
        }
    }
}
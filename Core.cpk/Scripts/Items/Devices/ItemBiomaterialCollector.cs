namespace AtomicTorch.CBND.CoreMod.Items.Devices
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Loot;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Systems.Resources;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ItemBiomaterialCollector : ProtoItemEquipmentDevice
    {
        public const string NotificationNoEmptyVials = "No empty vials to collect biomaterial";

        private const int DurabilityDecreasePerUse = 1;

        public override string Description =>
            "Special device for extracting biomaterial from animals and other creatures. Requires empty biomaterial vials to operate.";

        public override ushort DurabilityMax => 300;

        public override string Name => "Biomaterial collector";

        public override bool OnlySingleDeviceOfThisProtoAppliesEffect => true;

        protected override void PrepareProtoItemEquipment()
        {
            base.PrepareProtoItemEquipment();

            if (IsServer)
            {
                GatheringSystem.ServerOnGather += this.ServerGatheringSystemGatherHandler;
            }
        }

        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_NotEnoughEmptyVials()
        {
            NotificationSystem.ClientShowNotification(
                NotificationNoEmptyVials,
                color: NotificationColor.Bad,
                icon: this.Icon);
        }

        private void ServerGatheringSystemGatherHandler(ICharacter character, IStaticWorldObject worldObject)
        {
            if (!(worldObject.ProtoStaticWorldObject is ObjectCorpse))
            {
                return;
            }

            // corpse looted
            // find the device and vial
            var itemDevice = character.SharedGetPlayerContainerEquipment()
                                      .GetItemsOfProto(this)
                                      .FirstOrDefault();
            if (itemDevice == null)
            {
                // don't have an equipped device
                return;
            }

            var protoItemVialEmpty = GetProtoEntity<ItemVialEmpty>();
            // require at least one vial
            if (!character.ContainsItemsOfType(protoItemVialEmpty, requiredCount: 1))
            {
                // don't have an empty vial
                this.CallClient(character, _ => _.ClientRemote_NotEnoughEmptyVials());
                return;
            }

            var protoMob = (IProtoCharacterCore)worldObject.GetPublicState<ObjectCorpse.PublicState>()
                                                           .ProtoCharacterMob;
            var healthMax = protoMob.StatDefaultHealthMax;
            var maxVialsToUse = (ushort)MathHelper.Clamp(Math.Floor(healthMax / 100.0),
                                                         min: 1,
                                                         max: 10);

            // destroy empty vial
            Server.Items.DestroyItemsOfType(
                character,
                protoItemVialEmpty,
                maxVialsToUse,
                out var destroyedEmptyVialsCount);

            if (destroyedEmptyVialsCount == 0)
            {
                // cannot destroy any empty vial (should be impossible)
                return;
            }

            // spawn biomaterial vials
            var createItemResult = Server.Items.CreateItem<ItemVialBiomaterial>(character,
                                                                                count: destroyedEmptyVialsCount);

            var itemChangedCount = NotificationSystem.SharedGetItemsChangedCount(createItemResult);
            itemChangedCount.Add(protoItemVialEmpty, -(int)destroyedEmptyVialsCount);
            NotificationSystem.ServerSendItemsNotification(character, itemChangedCount);

            ItemDurabilitySystem.ServerModifyDurability(itemDevice, -DurabilityDecreasePerUse);
            Logger.Info("Biomaterial collected successfully with Biomaterial collector", character);
        }
    }
}
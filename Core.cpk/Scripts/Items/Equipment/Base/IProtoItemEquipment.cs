namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public interface IProtoItemEquipment : IProtoItem, IProtoItemWithDurablity
    {
        byte[] CompatibleContainerSlotsIds { get; }

        EquipmentType EquipmentType { get; }

        IReadOnlyStatsDictionary ProtoEffects { get; }

        IReadOnlyList<SkeletonSlotAttachment> SlotAttachmentsFemale { get; }

        IReadOnlyList<SkeletonSlotAttachment> SlotAttachmentsMale { get; }

        void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents);
    }
}
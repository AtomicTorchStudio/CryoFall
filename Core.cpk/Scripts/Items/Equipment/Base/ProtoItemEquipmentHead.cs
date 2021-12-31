namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using JetBrains.Annotations;

    /// <summary>
    /// Item prototype for head equipment.
    /// </summary>
    public abstract class ProtoItemEquipmentHead
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemEquipment
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemEquipmentHead
        where TPrivateState : ItemPrivateState, IItemWithDurabilityPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        public sealed override EquipmentType EquipmentType => EquipmentType.Head;

        public abstract bool IsHairVisible { get; }

        public virtual bool IsHeadVisible => true;

        public override bool IsSkinnable => true;

        public override bool RequireEquipmentTexturesFemale => false;

        public override bool RequireEquipmentTexturesMale => true;

        /// <inheritdoc />
        public ReadOnlySoundPreset<CharacterSound> SoundPresetCharacterOverride { get; private set; }

        protected override double DefenseMultiplier => DefaultDefenseMultipliers.Head;

        public virtual void ClientGetHeadSlotSprites(
            [CanBeNull] IItem item,
            bool isMale,
            SkeletonResource skeletonResource,
            bool isFrontFace,
            bool isPreview,
            out string spriteFront,
            out string spriteBehind)
        {
            IReadOnlyList<SkeletonSlotAttachment> slotAttachments;
            if (this.ClientIsMustUseDefaultAppearance(item?.Container.OwnerAsCharacter,
                                                      isPreview))
            {
                slotAttachments = isMale
                                      ? ((IProtoItemEquipment)this.BaseProtoItem).SlotAttachmentsMale
                                      : ((IProtoItemEquipment)this.BaseProtoItem).SlotAttachmentsFemale;
            }
            else
            {
                slotAttachments = isMale
                                      ? this.SlotAttachmentsMale
                                      : this.SlotAttachmentsFemale;
            }

            ProtoItemEquipmentHeadHelper.ClientFindDefaultHeadSprites(
                slotAttachments,
                skeletonResource,
                isFrontFace,
                out spriteFront,
                out spriteBehind);
        }

        public override void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents,
            bool isPreview)
        {
            // we don't call basic implementation here
            //base.ClientSetupSkeleton(item, character, skeletonRenderer);
        }

        protected sealed override void PrepareProtoItemEquipment()
        {
            base.PrepareProtoItemEquipment();
            this.SoundPresetCharacterOverride = this.PrepareSoundPresetCharacterOverride();
            this.PrepareProtoItemEquipmentHead();
        }

        protected virtual void PrepareProtoItemEquipmentHead()
        {
        }

        protected virtual ReadOnlySoundPreset<CharacterSound> PrepareSoundPresetCharacterOverride()
        {
            return null;
        }
    }

    /// <summary>
    /// Item prototype for head equipment.
    /// </summary>
    public abstract class ProtoItemEquipmentHead
        : ProtoItemEquipmentHead
            <ItemWithDurabilityPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}
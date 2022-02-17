namespace AtomicTorch.CBND.CoreMod.Items.Equipment
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.SlotOverlays;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Other.StatModificationDisplay;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.DataStructures;

    /// <summary>
    /// Item prototype for equipment.
    /// </summary>
    public abstract class ProtoItemEquipment
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItem
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemEquipment
        where TPrivateState : ItemPrivateState, IItemWithDurabilityPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        public byte[] CompatibleContainerSlotsIds { get; private set; }

        public abstract uint DurabilityMax { get; }

        public abstract EquipmentType EquipmentType { get; }

        public override double GroundIconScale => 1.5;

        public virtual bool IsRepairable => true;

        /// <summary>
        /// Equipment items cannot be stacked.
        /// </summary>
        public sealed override ushort MaxItemsPerStack => 1;

        public IReadOnlyStatsDictionary ProtoEffects { get; private set; }

        public abstract bool RequireEquipmentTexturesFemale { get; }

        public abstract bool RequireEquipmentTexturesMale { get; }

        public IReadOnlyList<SkeletonSlotAttachment> SlotAttachmentsFemale { get; private set; }

        public IReadOnlyList<SkeletonSlotAttachment> SlotAttachmentsMale { get; private set; }

        protected abstract double DefenseMultiplier { get; }

        public virtual void ClientCreateItemSlotOverlayControls(IItem item, List<Control> controls)
        {
            if (this.DurabilityMax > 0)
            {
                controls.Add(ItemSlotDurabilityOverlayControl.Create(item));
            }
        }

        public virtual void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents,
            bool isPreview)
        {
            if (this.ClientIsMustUseDefaultAppearance(character, isPreview))
            {
                // the skin is not owned, apply base proto item's appearance instead
                ((IProtoItemEquipment)this.BaseProtoItem)
                    .ClientSetupSkeleton(item,
                                         character,
                                         skeletonRenderer,
                                         skeletonComponents,
                                         isPreview: false);
                return;
            }

            var isMale = PlayerCharacter.GetPublicState(character).IsMale;
            var slotAttachments = isMale
                                      ? this.SlotAttachmentsMale
                                      : this.SlotAttachmentsFemale;

            ClientSkeletonAttachmentsLoader.SetAttachments(skeletonRenderer, slotAttachments);
        }

        public virtual void ServerOnItemBrokeAndDestroyed(IItem item, IItemsContainer container, byte slotId)
        {
        }

        public virtual void ServerOnItemDamaged(IItem item, double damageApplied)
        {
            ItemDurabilitySystem.ServerModifyDurability(item, delta: -(int)damageApplied);
        }

        public virtual bool SharedCanApplyEffects(IItem item, IItemsContainer containerEquipment)
        {
            return true;
        }

        protected virtual void ClientFillSlotAttachmentSources(ITempList<string> folders)
        {
            var baseProtoItem = this.BaseProtoItem;
            if (baseProtoItem is not null)
            {
                folders.Add(
                    $"Characters/Equipment/{baseProtoItem.ShortId}/{this.ShortId.Substring(baseProtoItem.ShortId.Length)}");
                return;
            }

            var path = $"Characters/Equipment/{this.ShortId}/Default";
            if (Api.Shared.IsFolderExists(ContentPaths.Textures + path))
            {
                folders.Add(path);
                return;
            }

            folders.Add($"Characters/Equipment/{this.ShortId}");
        }
    
        protected override void ClientTooltipCreateControlsInternal(IItem item, List<UIElement> controls)
        {
            controls.Add(
                new StatModificationDisplay(this.ProtoEffects, hideDefenseStats: true)
                {
                    Opacity = 0.8,
                    Margin = new Thickness(0, 0, 0, 8)
                });

            base.ClientTooltipCreateControlsInternal(item, controls);

            controls.Add(
                ItemTooltipArmorStats.Create(this));
        }

        protected override string GenerateIconPath()
        {
            var baseProtoItem = this.BaseProtoItem;
            if (baseProtoItem is not null)
            {
                return
                    $"Items/Equipment/Item{baseProtoItem.ShortId}/{this.ShortId.Substring(baseProtoItem.ShortId.Length)}";
            }

            var path = $"Items/Equipment/Item{this.ShortId}/Default";
            if (Api.Shared.IsFolderExists(ContentPaths.Textures + path))
            {
                return path;
            }

            return $"Items/Equipment/{this.GetType().Name}";
        }

        protected abstract void PrepareDefense(DefenseDescription defense);

        protected virtual void PrepareEffects(Effects effects)
        {
        }

        protected sealed override void PrepareProtoItem()
        {
            base.PrepareProtoItem();
            this.CompatibleContainerSlotsIds = this.SharedGetCompatibleContainerSlotsIds();

            if (IsClient)
            {
                using var tempSourcePaths = Api.Shared.GetTempList<string>();
                this.ClientFillSlotAttachmentSources(tempSourcePaths);
                using var tempSpritePaths = ClientEquipmentSpriteHelper.CollectSpriteFilePaths(
                    tempSourcePaths.AsList());

                ClientEquipmentSpriteHelper.CollectSlotAttachments(
                    tempSpritePaths.AsList(),
                    this.Id,
                    requireEquipmentTexturesMale: this.RequireEquipmentTexturesMale,
                    requireEquipmentTexturesFemale: this.RequireEquipmentTexturesFemale,
                    out var slotAttachmentsMale,
                    out var slotAttachmentsFemale);

                this.SlotAttachmentsMale = slotAttachmentsMale;
                this.SlotAttachmentsFemale = slotAttachmentsFemale;

                foreach (var file in tempSpritePaths.AsList())
                {
                    file.Dispose();
                }
            }

            var defenseDescription = new DefenseDescription();
            if (this.DefenseMultiplier < 0
                || this.DefenseMultiplier > 1)
            {
                throw new Exception(
                    $"{nameof(this.DefenseMultiplier)} must be in range from 0 to 1 (inclusive) for {this}. Current value is: {this.DefenseMultiplier:F2}");
            }

            defenseDescription.SetMultiplier(this.DefenseMultiplier);
            this.PrepareDefense(defenseDescription);
            var defense = defenseDescription.ToReadOnly();

            var effects = new Effects();
            defense.FillEffects(this, effects);
            this.PrepareEffects(effects);
            this.ProtoEffects = effects.ToReadOnly();

            this.PrepareProtoItemEquipment();
        }

        protected virtual void PrepareProtoItemEquipment()
        {
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);
            ItemDurabilitySystem.ServerInitializeItem(data.PrivateState, data.IsFirstTimeInit);
        }

        /// <summary>
        /// Gets the array of allowed slots to allow the item to be equipped into. Cached during <see cref="PrepareProtoItem" />.
        /// </summary>
        protected virtual byte[] SharedGetCompatibleContainerSlotsIds()
        {
            return new[]
            {
                (byte)this.EquipmentType
            };
        }
    }

    /// <summary>
    /// Item prototype for equipment.
    /// </summary>
    public abstract class ProtoItemEquipment
        : ProtoItemEquipment
            <ItemWithDurabilityPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}
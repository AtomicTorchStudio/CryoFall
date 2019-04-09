namespace AtomicTorch.CBND.CoreMod.Items
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;

    /// <summary>
    /// Base class for item types with specific generics parameters for states.
    /// </summary>
    /// <typeparam name="TPrivateState">Type of server private state.</typeparam>
    /// <typeparam name="TPublicState">Type of server public state.</typeparam>
    /// <typeparam name="TClientState">Type of client state.</typeparam>
    public abstract class ProtoItem
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoGameObject
          <IItem,
              TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItem,
          IProtoItemWithSoundPreset
        where TPrivateState : BasePrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        /// <summary>
        /// This flag will be true only in case the method <see cref="ClientItemUseStart" /> or <see cref="ClientItemUseFinish" />
        /// has override.
        /// </summary>
        private bool isUsableItem;

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        protected ProtoItem()
        {
            var name = this.GetType().Name;
            if (!name.StartsWith("Item", StringComparison.Ordinal))
            {
                throw new Exception("Item class name must start with \"Item\": " + this.GetType().Name);
            }

            this.ShortId = name.Substring("Item".Length);
        }

        /// <summary>
        /// Every frame.
        /// </summary>
        public override double ClientUpdateIntervalSeconds => 0;

        /// <summary>
        /// Text description.
        /// </summary>
        public abstract string Description { get; }

        public virtual ITextureResource GroundIcon => this.Icon;

        public virtual double GroundIconScale => 1.0;

        /// <summary>
        /// Gets the item icon.
        /// </summary>
        public abstract ITextureResource Icon { get; }

        /// <summary>
        /// Returns true if this item could be stacked (has count more than 1 per item slot).
        /// </summary>
        public bool IsStackable => this.MaxItemsPerStack > 1;

        /// <summary>
        /// Gets how many items could be stored in one stack (item slot). For non-stackable items this will be 1.
        /// </summary>
        public abstract ushort MaxItemsPerStack { get; }

        public override double ServerUpdateIntervalSeconds => 1;

        public override string ShortId { get; }

        public ReadOnlySoundPreset<ItemSound> SoundPresetItem { get; private set; }

        public void ClientItemHotbarSelectionChanged(IItem item, bool isSelected, bool isByPlayer)
        {
            Logger.Info($"{item} is {(isSelected ? "selected" : "unselected")} in hotbar now");
            this.ClientItemHotbarSelectionChanged(new ClientHotbarItemData(item, isSelected, isByPlayer));
            //this.SoundPresetItem.PlaySound(isSelected ? ItemSound.HotbarSelect : ItemSound.HotbarDeselect);
        }

        /// <summary>
        /// Client use item finish method. Called for current selected item in hotbar when Client release the left mouse button.
        /// </summary>
        /// <param name="item">Current selected item.</param>
        public void ClientItemUseFinish(IItem item)
        {
            if (!this.isUsableItem)
            {
                return;
            }

            ValidateIsClient();
            if (!ClientUseItemHelper.ClientIsUsingItem)
            {
                // already finished using item
                return;
            }

            ClientUseItemHelper.ClientIsUsingItem = false;
            if (this.ClientItemUseFinish(new ClientItemData(item)))
            {
                this.SoundPresetItem.PlaySound(ItemSound.Use,
                                               pitch: RandomHelper.Range(0.95f, 1.05f));
            }
        }

        /// <summary>
        /// Client use item start method. Called for current selected item in hotbar when Client press the left mouse button.
        /// </summary>
        /// <param name="item">Current selected item.</param>
        public void ClientItemUseStart(IItem item)
        {
            if (!this.isUsableItem)
            {
                return;
            }

            ValidateIsClient();
            if (ClientUseItemHelper.ClientIsUsingItem)
            {
                // already started using item
                return;
            }

            ClientUseItemHelper.ClientIsUsingItem = true;
            this.ClientItemUseStart(new ClientItemData(item));
        }

        public void ClientOnItemDrop(IItem item, IItemsContainer itemsContainer = null)
        {
            if (itemsContainer == null)
            {
                itemsContainer = item.Container;
                if (itemsContainer == null)
                {
                    Logger.Error($"Item container is null for {item} - perhaps item was deleted");
                    return;
                }
            }

            if (itemsContainer.ProtoItemsContainer is ItemsContainerCharacterEquipment)
            {
                // item equipped
                this.SoundPresetItem.PlaySound(ItemSound.Equip);
                return;
            }

            this.SoundPresetItem.PlaySound(ItemSound.Drop);
        }

        public void ClientOnItemPick(IItem item, IItemsContainer fromContainer)
        {
            if (fromContainer?.ProtoItemsContainer is ItemsContainerCharacterEquipment)
            {
                // item unequipped
                this.SoundPresetItem.PlaySound(ItemSound.Unequip);
                return;
            }

            this.SoundPresetItem.PlaySound(ItemSound.Pick);
        }

        public virtual void ClientTooltipCreateControls(IItem item, List<Control> controls)
        {
            if (this is IProtoItemWithFuel protoItemWithFuel
                && protoItemWithFuel.ItemFuelConfig.FuelCapacity > 0)
            {
                controls.Add(ItemTooltipFuelControl.Create(item));
            }

            if (this is IProtoItemWithDurablity protoItemWithDurablity
                && protoItemWithDurablity.DurabilityMax > 0)
            {
                controls.Add(ItemTooltipInfoDurabilityControl.Create(item));
            }
        }

        public virtual void ServerOnCharacterDeath(IItem item, bool isEquipped, out bool shouldDrop)
        {
            shouldDrop = true;
            if (!isEquipped)
            {
                return;
            }

            if (item.ProtoItem is IProtoItemWithDurablity protoItemWithDurablity)
            {
                // -10% of durability reduced on death
                const double durabilityFractionReduceOnDeath = 0.1;
                var durabilityDelta = (ushort)(protoItemWithDurablity.DurabilityMax * durabilityFractionReduceOnDeath);
                ItemDurabilitySystem.ServerModifyDurability(item, delta: -durabilityDelta);
            }
        }

        public void ServerOnSplitItem(IItem itemFrom, IItem newItem, int countSplit)
        {
            ValidateIsServer();
            try
            {
                this.ServerOnSplitItem(new ServerOnSplitItemData(itemFrom, newItem, countSplit));
            }
            catch (Exception ex)
            {
                this.ReportException(ex);
            }
        }

        /// <summary>
        /// Server callback method when stacking items (of this item type) from one slot into another slot.
        /// This method is called only if the item type is stackable.
        /// Use it to merge items states (if any).
        /// </summary>
        /// <param name="itemFrom">Item of this ProtoItem</param>
        /// <param name="itemTo">Item of this ProtoItem</param>
        /// <param name="countStacked">Count subtracted from itemFrom and added to itemTo</param>
        public void ServerOnStackItems(IItem itemFrom, IItem itemTo, int countStacked)
        {
            ValidateIsServer();
            try
            {
                this.ServerOnStackItems(new ServerOnStackItemData(itemFrom, itemTo, countStacked));
            }
            catch (Exception ex)
            {
                this.ReportException(ex);
            }
        }

        public virtual bool SharedCanSelect(IItem item, ICharacter character)
        {
            return true;
        }

        protected virtual void ClientItemHotbarSelectionChanged(ClientHotbarItemData data)
        {
        }

        /// <summary>
        /// Client use item finish method. Called for current selected item in hotbar when Client release the left mouse button.
        /// </summary>
        protected virtual bool ClientItemUseFinish(ClientItemData data)
        {
            return false;
        }

        /// <summary>
        /// Client use item start method. Called for current selected item in hotbar when Client press the left mouse button.
        /// </summary>
        protected virtual void ClientItemUseStart(ClientItemData data)
        {
        }

        /// <summary>
        /// Invoked when other character had used the item of this prototype.
        /// By default it plays the "Use" sound from the sound preset. But also it might be useful for playing animations, etc.
        /// </summary>
        protected virtual void ClientOnOtherCharacterUsedItem(ICharacter character)
        {
            this.SoundPresetItem.PlaySound(ItemSound.Use,
                                           character,
                                           pitch: RandomHelper.Range(0.95f, 1.05f));
        }

        /// <summary>
        /// Prepares prototype - invoked after all scripts are loaded, so you can access other scripting
        /// entities by using <see cref="ProtoEntity.GetProtoEntity{TProtoEntity}" /> and
        /// <see cref="ProtoEntity.FindProtoEntities{TProtoEntity}" /> methods.
        /// </summary>
        protected sealed override void PrepareProto()
        {
            base.PrepareProto();

            var type = this.GetType();
            this.isUsableItem = type.HasOverride(nameof(ClientItemUseStart),     isPublic: false)
                                || type.HasOverride(nameof(ClientItemUseFinish), isPublic: false);

            this.SoundPresetItem = this.PrepareSoundPresetItem();

            this.PrepareProtoItem();
        }

        /// <summary>
        /// Prepares prototype - invoked after all scripts are loaded, so you can access other scripting
        /// entities by using <see cref="ProtoEntity.GetProtoEntity{TProtoEntity}" /> and
        /// <see cref="ProtoEntity.FindProtoEntities{TProtoEntity}" /> methods.
        /// </summary>
        protected virtual void PrepareProtoItem()
        {
        }

        protected virtual ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemGeneric;
        }

        /// <summary>
        /// Invokes for other characters method <see cref="ClientOnOtherCharacterUsedItem" />.
        /// </summary>
        /// <param name="character"></param>
        protected void ServerNotifyItemUsed(ICharacter character, IItem item)
        {
            using (var scopedBy = Api.Shared.GetTempList<ICharacter>())
            {
                Server.World.GetScopedByPlayers(character, scopedBy);
                this.CallClient(scopedBy, _ => _.ClientRemote_CharacterUsedItem(character));
            }

            ServerItemUseObserver.NotifyItemUsed(character, item);
        }

        /// <summary>
        /// Server callback method when splitting items (of this item type) from one slot into another slot.
        /// This method is called only if the item type is stackable.
        /// Use it to split items states (if any).
        /// Please note that the new item already has a copy of the source item state.
        /// </summary>
        protected virtual void ServerOnSplitItem(ServerOnSplitItemData data)
        {
        }

        /// <summary>
        /// Server callback method when stacking items (of this item type) from one slot into another slot.
        /// This method is called only if the item type is stackable.
        /// Use it to merge items states (if any).
        /// </summary>
        protected virtual void ServerOnStackItems(ServerOnStackItemData data)
        {
        }

        protected void ServerValidateItemForRemoteCall(IItem item, ICharacter character)
        {
            if (item == null)
            {
                throw new Exception("Item is not found.");
            }

            if (item.ProtoGameObject != this)
            {
                throw new Exception("Item type is different!");
            }

            if (!Server.Core.IsInPrivateScope(character, item))
            {
                throw new Exception(
                    $"{character} cannot access {item} because it's container is not in the private scope");
            }

            if (item.IsDestroyed
                || item.Count < 1)
            {
                throw new Exception($"{item} is destroyed");
            }
        }

        [RemoteCallSettings(DeliveryMode.ReliableOrdered, keyArgIndex: 0)]
        private void ClientRemote_CharacterUsedItem(ICharacter character)
        {
            Logger.Important($"Other character used {this}: {character}");
            this.ClientOnOtherCharacterUsedItem(character);
        }

        /// <summary>
        /// Data for ClientItemHotbarSelectionChanged method.
        /// </summary>
        protected struct ClientHotbarItemData
        {
            public readonly bool IsByPlayer;

            public readonly bool IsSelected;

            /// <summary>
            /// Item.
            /// </summary>
            public readonly IItem Item;

            private TClientState clientState;

            private TPrivateState syncPrivateState;

            private TPublicState syncPublicState;

            internal ClientHotbarItemData(IItem item, bool isSelected, bool isByPlayer)
                : this()
            {
                this.Item = item;
                this.IsSelected = isSelected;
                this.IsByPlayer = isByPlayer;
            }

            /// <summary>
            /// Client state of item.
            /// </summary>
            public TClientState ClientState => this.clientState ?? (this.clientState = GetClientState(this.Item));

            /// <summary>
            /// Synchronized server private state for this item.<br />
            /// It will throw exception if you don't have this game object in your private state.
            /// </summary>
            public TPrivateState SyncPrivateState
                => this.syncPrivateState
                   ?? (this.syncPrivateState = GetPrivateState(this.Item));

            /// <summary>
            /// Synchronized server public state for this item.
            /// </summary>
            public TPublicState SyncPublicState
                => this.syncPublicState
                   ?? (this.syncPublicState = GetPublicState(this.Item));
        }

        /// <summary>
        /// Data for ClientItemUseStart/ClientItemUseFinish methods.
        /// </summary>
        protected struct ClientItemData
        {
            /// <summary>
            /// Item.
            /// </summary>
            public readonly IItem Item;

            private TClientState clientState;

            private TPrivateState syncPrivateState;

            private TPublicState syncPublicState;

            internal ClientItemData(IItem item) : this()
            {
                this.Item = item;
            }

            /// <summary>
            /// Client state of item.
            /// </summary>
            public TClientState ClientState => this.clientState ?? (this.clientState = GetClientState(this.Item));

            /// <summary>
            /// Synchronized server private state for this item.<br />
            /// It will throw exception if you don't have this game object in your private state.
            /// </summary>
            public TPrivateState SyncPrivateState
                => this.syncPrivateState
                   ?? (this.syncPrivateState = GetPrivateState(this.Item));

            /// <summary>
            /// Synchronized server public state for this item.
            /// </summary>
            public TPublicState SyncPublicState
                => this.syncPublicState
                   ?? (this.syncPublicState = GetPublicState(this.Item));
        }

        /// <summary>
        /// Data for ServerOnSplitItem() method.
        /// </summary>
        protected struct ServerOnSplitItemData
        {
            /// <summary>
            /// Count subtracted from itemFrom and added to newItem.
            /// </summary>
            public readonly int CountSplit;

            /// <summary>
            /// Item of this ProtoItem, from which count is subtracted.
            /// </summary>
            public readonly IItem ItemFrom;

            /// <summary>
            /// Item of this ProtoItem, to which count is added.
            /// </summary>
            public readonly IItem NewItem;

            internal ServerOnSplitItemData(IItem itemFrom, IItem newItem, int countSplit)
            {
                this.ItemFrom = itemFrom;
                this.NewItem = newItem;
                this.CountSplit = countSplit;
            }
        }

        /// <summary>
        /// Data for ServerOnStackItem() method.
        /// </summary>
        protected struct ServerOnStackItemData
        {
            /// <summary>
            /// Count subtracted from itemFrom and added to itemTo.
            /// </summary>
            public readonly int CountStacked;

            /// <summary>
            /// Item of this ProtoItem, from which count is subtracted.
            /// </summary>
            public readonly IItem ItemFrom;

            /// <summary>
            /// Item of this ProtoItem, to which count is added.
            /// </summary>
            public readonly IItem ItemTo;

            internal ServerOnStackItemData(IItem itemFrom, IItem itemTo, int countStacked)
            {
                this.ItemFrom = itemFrom;
                this.ItemTo = itemTo;
                this.CountStacked = countStacked;
            }
        }
    }
}
namespace AtomicTorch.CBND.CoreMod.Items
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.ItemContainers;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Crafting;
    using AtomicTorch.CBND.CoreMod.Systems.ItemDurability;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.UI;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips;
    using AtomicTorch.CBND.CoreMod.Vehicles;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using JetBrains.Annotations;

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
          IProtoItemWithSoundPreset,
          IProtoItemWithRecipeData,
          IProtoItemWithSkinData
        where TPrivateState : ItemPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        public const string NotificationItemCannotBeUsedInVehicle =
            "This item cannot be used while you're riding a vehicle.";

        private const double DurabilityFractionReduceOnDeath = 0.15;

        private readonly Lazy<IProtoItem> lazyBaseProtoItem;

        /// <summary>
        /// This flag will be true only in case the method <see cref="ClientItemUseStart" /> or <see cref="ClientItemUseFinish" />
        /// has override.
        /// </summary>
        private bool isUsableItem;

        private List<Recipe> listedInRecipes;

        private List<IProtoItem> skins;

        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        protected ProtoItem()
        {
            var name = this.GetType().Name;
            if (name.StartsWith("Item", StringComparison.Ordinal))
            {
                this.ShortId = name.Substring("Item".Length);
            }
            else if (name.StartsWith("Skin", StringComparison.Ordinal))
            {
                this.ShortId = name.Substring("Skin".Length);
            }
            else
            {
                throw new Exception("Item class name must start with \"Item\" or \"Skin\": " + this.GetType().Name);
            }

            this.lazyBaseProtoItem = new Lazy<IProtoItem>(
                () =>
                {
                    var baseType = this.GetType().BaseType;
                    if (baseType is null
                        || baseType.IsAbstract)
                    {
                        return null;
                    }

                    return (IProtoItem)Api.Shared.GetProtoEntityAbstract(baseType);
                });
        }

        public virtual IProtoItem BaseProtoItem => this.lazyBaseProtoItem.Value;

        public virtual bool CanBeSelectedInVehicle => false;

        /// <summary>
        /// Every frame.
        /// </summary>
        public override double ClientUpdateIntervalSeconds => 0;

        /// <summary>
        /// Text description.
        /// </summary>
        public abstract string Description { get; }

        public IReadOnlyList<string> DescriptionHints
        {
            get
            {
                if (IsServer)
                {
                    // the game cannot provide hints on the server and there is no need for it
                    // (some hints may contain button binding key names, etc)
                    return Array.Empty<string>();
                }

                var hints = new List<string>();
                this.PrepareHints(hints);
                return hints;
            }
        }

        public virtual ITextureResource GroundIcon => this.Icon;

        public virtual double GroundIconScale => 1.0;

        public virtual bool HasSkinCustomEffects => false;

        /// <summary>
        /// Gets the item icon.
        /// </summary>
        public ITextureResource Icon { get; private set; }

        public virtual bool IsModSkin => false;

        public bool IsSkin => this.BaseProtoItem is not null;

        public virtual bool IsSkinnable => false;

        /// <summary>
        /// Returns true if this item could be stacked (has count more than 1 per item slot).
        /// </summary>
        public bool IsStackable => this.MaxItemsPerStack > 1;

        public IReadOnlyList<Recipe> ListedInRecipes
            => this.listedInRecipes
               ?? (IReadOnlyList<Recipe>)Array.Empty<Recipe>();

        /// <summary>
        /// Gets how many items could be stored in one stack (item slot). For non-stackable items this will be 1.
        /// </summary>
        public abstract ushort MaxItemsPerStack { get; }

        public override double ServerUpdateIntervalSeconds => double.MaxValue;

        public override string ShortId { get; }

        public SkinId SkinId { get; private set; }

        public IReadOnlyList<IProtoItem> Skins
            => this.skins
               ?? (IReadOnlyList<IProtoItem>)Array.Empty<IProtoItem>();

        public ReadOnlySoundPreset<ItemSound> SoundPresetItem { get; private set; }

        public virtual ITextureResource ClientGetIcon(IItem item)
        {
            return this.Icon;
        }

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

            this.ClientItemUseStart(new ClientItemData(item));
        }

        public virtual void ClientOnItemContainerSlotChanged(IItem item)
        {
        }

        public virtual void ClientOnItemDrop(IItem item, IItemsContainer itemsContainer = null)
        {
            if (itemsContainer is null)
            {
                itemsContainer = item.Container;
                if (itemsContainer is null)
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

        public virtual void ClientOnItemPick(IItem item, IItemsContainer fromContainer)
        {
            if (fromContainer?.ProtoItemsContainer is ItemsContainerCharacterEquipment)
            {
                // item unequipped
                this.SoundPresetItem.PlaySound(ItemSound.Unequip);
                return;
            }

            this.SoundPresetItem.PlaySound(ItemSound.Pick);
        }

        public void ClientTooltipCreateControls([CanBeNull] IItem item, List<UIElement> controls)
        {
            this.ClientTooltipCreateControlsInternal(item, controls);

            if (this.IsSkin
                && Client.Microtransactions.GetSkinData((ushort)this.SkinId).Pool == SkinsPool.SupporterPack
                && !Api.Client.MasterServer.IsSupporterPackOwner)
            {
                controls.Add(new ItemTooltipAlertControl() { Text = CoreStrings.Skin_RequiresSupporterPack });
            }

            if (item is null
                || this.DescriptionHints.Count == 0)
            {
                return;
            }

            controls.Add(new Control() { Height = 5 }); // padding before the hints
            foreach (var hint in this.DescriptionHints)
            {
                controls.Add(new ItemTooltipHintControl() { Text = hint });
            }
        }

        public bool IsSkinOrVariant(IProtoItem protoItem)
        {
            if (this == protoItem)
            {
                // same prototype - assume it's a skin
                return true;
            }

            if (this.IsSkin)
            {
                // a skin itself - ask its base item (skins holder) whether the provided item is a skin
                return ((IProtoItemWithSkinData)this.BaseProtoItem)
                    .IsSkinOrVariant(protoItem);
            }

            // not a skin - determine whether the provided item is its inheritor
            return ((IProtoItemWithSkinData)protoItem).BaseProtoItem == this;
        }

        public void PrepareProtoItemLinkRecipe(Recipe recipe)
        {
            this.listedInRecipes ??= new List<Recipe>();
            this.listedInRecipes.Add(recipe);
        }

        public void PrepareProtoItemLinkSkin(IProtoItem protoItem)
        {
            if (((IProtoItemWithSkinData)protoItem).BaseProtoItem != this)
            {
                throw new InvalidOperationException();
            }

            if (!this.IsSkinnable)
            {
                return;
            }

            if (this.IsStackable)
            {
                throw new Exception($"The item is not skinnable: {this.Id}. Cannot register skin: {protoItem.Id}");
            }

            this.skins ??= new List<IProtoItem>();
            this.skins.Add(protoItem);
        }

        public virtual void ServerItemHotbarSelectionChanged(
            IItem item,
            ICharacter character,
            bool isSelected)
        {
        }

        public virtual void ServerOnCharacterDeath(IItem item, bool isEquipped, out bool shouldDrop)
        {
            // only unequipped items will drop unless full loot system enabled
            shouldDrop = RatePvPIsFullLootEnabled.SharedValue
                         || !isEquipped;

            if (!isEquipped)
            {
                return;
            }

            if (item.ProtoItem is IProtoItemWithDurability protoItemWithDurability)
            {
                // reduce equipped item's durability on death
                var durabilityDelta = protoItemWithDurability.DurabilityMax * DurabilityFractionReduceOnDeath;
                ItemDurabilitySystem.ServerModifyDurability(item,
                                                            delta: -durabilityDelta,
                                                            roundUp: false);
            }
        }

        public virtual void ServerOnItemContainerSlotChanged(IItem item, ICharacter byCharacter)
        {
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

        public virtual bool SharedCanSelect(IItem item, ICharacter character, bool isAlreadySelected, bool isByPlayer)
        {
            var vehicle = PlayerCharacter.GetPublicState(character).CurrentVehicle;
            if (vehicle is null)
            {
                return true;
            }

            var protoVehicle = (IProtoVehicle)vehicle.ProtoGameObject;
            if (this.CanBeSelectedInVehicle
                && protoVehicle.IsPlayersHotbarAndEquipmentItemsAllowed)
            {
                return true;
            }

            if (IsClient
                && !isAlreadySelected
                && protoVehicle.IsPlayersHotbarAndEquipmentItemsAllowed
                && isByPlayer)
            {
                NotificationSystem.ClientShowNotification(this.Name,
                                                          NotificationItemCannotBeUsedInVehicle,
                                                          NotificationColor.Neutral,
                                                          item.ProtoItem.Icon);
            }

            return false;
        }

        protected static string SharedGetRelativeFolderPath(Type type, Type baseType)
        {
            var ns = type.Namespace;
            var defaultNamespace = baseType.Namespace;

            if (string.Equals(ns, defaultNamespace))
            {
                return string.Empty;
            }

            // ReSharper disable once PossibleNullReferenceException
            if (!ns.StartsWith(defaultNamespace))
            {
                throw new Exception(
                    "Namespace for " + type.FullName + " must start with base class namespace: " + defaultNamespace);
            }

            var folderPath = ns.Substring(defaultNamespace.Length + 1).Replace('.', '/');
            return folderPath;
        }

        protected bool ClientIsMustUseDefaultAppearance(ICharacter character, bool isPreview)
        {
            // currently the game will not apply Supporter Pack skins for players that don't own Supporter Pack
            return this.IsSkin
                   && (!isPreview || !CraftingSkinPreviewControl.IsDisplayed)
                   && character is not null
                   && !character.IsNpc
                   && Client.Microtransactions.GetSkinData((ushort)this.SkinId).Pool == SkinsPool.SupporterPack
                   && !PlayerCharacter.GetPublicState(character).IsSupporterPackOwner;
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

        protected virtual void ClientTooltipCreateControlsInternal(IItem item, List<UIElement> controls)
        {
            if (item is not null
                && this is IProtoItemWithFuel protoItemWithFuel
                && protoItemWithFuel.ItemFuelConfig.FuelCapacity > 0)
            {
                controls.Add(ItemTooltipFuelControl.Create(item));
            }

            if (item is not null
                && this is IProtoItemWithDurability protoItemWithDurability
                && protoItemWithDurability.DurabilityMax > 0)
            {
                controls.Add(ItemTooltipInfoDurabilityControl.Create(item));
            }
        }

        protected virtual string GenerateIconPath()
        {
            var type = this.GetType();
            var folderPath = SharedGetRelativeFolderPath(type, typeof(ProtoItem<,,>));
            var baseProtoItem = this.BaseProtoItem;
            if (baseProtoItem is not null)
            {
                var baseName = baseProtoItem.GetType().Name;
                return $"Items/{folderPath}/{baseName}/{this.ShortId.Substring(baseName.Length - 4)}.png";
            }

            var path = $"Items/{folderPath}/{type.Name}/Default.png";
            if (Api.Shared.IsFileExists(ContentPaths.Textures + path))
            {
                return path;
            }

            return $"Items/{folderPath}/{type.Name}.png";
        }

        protected virtual void PrepareHints(List<string> hints)
        {
        }

        protected virtual ITextureResource PrepareIcon()
        {
            return new TextureResource(this.GenerateIconPath(), qualityOffset: -1);
        }

        /// <summary>
        /// Prepares prototype - invoked after all scripts are loaded, so you can access other scripting
        /// entities by using <see cref="ProtoEntity.GetProtoEntity{TProtoEntity}" /> and
        /// <see cref="ProtoEntity.FindProtoEntities{TProtoEntity}" /> methods.
        /// </summary>
        protected sealed override void PrepareProto()
        {
            if (this.IsSkin)
            {
                if (!this.GetType().Name.StartsWith("Skin", StringComparison.Ordinal))
                {
                    throw new Exception(this.Id + " is a skin - its class name must start with \"Skin\"");
                }

                this.SkinId = SkinBinding.BindSkin(this);
            }

            // Please note: the icon is required on the server side as well e.g. to use for console commands
            // autocomplete (as it should not provide items that don't have icons).
            var icon = this.PrepareIcon();
            if (icon is ITextureAtlasResource iconAtlas
                and not ITextureAtlasChunkResource)
            {
                // use the first chunk of the atlas texture
                icon = iconAtlas.Chunk(0, 0);
            }

            this.Icon = icon;

            var type = this.GetType();
            this.isUsableItem = type.HasOverride(nameof(ClientItemUseStart),     isPublic: false)
                                || type.HasOverride(nameof(ClientItemUseFinish), isPublic: false);

            this.SoundPresetItem = this.PrepareSoundPresetItem();

            ((IProtoItemWithSkinData)this.BaseProtoItem)?.PrepareProtoItemLinkSkin(this);

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
            using var scopedBy = Api.Shared.GetTempList<ICharacter>();
            Server.World.GetScopedByPlayers(character, scopedBy);
            this.CallClient(scopedBy.AsList(), _ => _.ClientRemote_CharacterUsedItem(character));
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
            if (item is null)
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

        [RemoteCallSettings(DeliveryMode.ReliableUnordered, keyArgIndex: 0)]
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

            public readonly IItem Item;

            private TClientState clientState;

            private TPrivateState privateState;

            private TPublicState publicState;

            internal ClientHotbarItemData(IItem item, bool isSelected, bool isByPlayer)
                : this()
            {
                this.Item = item;
                this.IsSelected = isSelected;
                this.IsByPlayer = isByPlayer;
            }

            /// <summary>
            /// Client state of the item.
            /// </summary>
            public TClientState ClientState => this.clientState ??= GetClientState(this.Item);

            /// <summary>
            /// Synchronized server private state for the item.
            /// It will throw exception if you don't have this game object in your private state.
            /// </summary>
            public TPrivateState PrivateState => this.privateState ??= GetPrivateState(this.Item);

            /// <summary>
            /// Synchronized server public state for the item.
            /// </summary>
            public TPublicState PublicState => this.publicState ??= GetPublicState(this.Item);
        }

        /// <summary>
        /// Data for ClientItemUseStart/ClientItemUseFinish methods.
        /// </summary>
        protected struct ClientItemData
        {
            public readonly IItem Item;

            private TClientState clientState;

            private TPrivateState privateState;

            private TPublicState publicState;

            internal ClientItemData(IItem item) : this()
            {
                this.Item = item;
            }

            /// <summary>
            /// Client state of item.
            /// </summary>
            public TClientState ClientState => this.clientState ??= GetClientState(this.Item);

            /// <summary>
            /// Synchronized server private state for the item.
            /// It will throw exception if you don't have this game object in your private state.
            /// </summary>
            public TPrivateState PrivateState => this.privateState ??= GetPrivateState(this.Item);

            /// <summary>
            /// Synchronized server public state for the item.
            /// </summary>
            public TPublicState PublicState => this.publicState ??= GetPublicState(this.Item);
        }

        /// <summary>
        /// Data for ServerOnSplitItem() method.
        /// </summary>
        protected readonly struct ServerOnSplitItemData
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
        protected readonly struct ServerOnStackItemData
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
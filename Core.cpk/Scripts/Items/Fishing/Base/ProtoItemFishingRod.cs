namespace AtomicTorch.CBND.CoreMod.Items.Fishing.Base
{
    using System.Collections.Generic;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.FishingBaitReloadingSystem;
    using AtomicTorch.CBND.CoreMod.Systems.FishingSystem;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// Base tool item prototype for fishing rods.
    /// </summary>
    public abstract class ProtoItemFishingRod
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemTool
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemToolFishing
        where TPrivateState : ItemPrivateState, IItemWithDurabilityPrivateState, new()
        where TPublicState : ItemFishingRodPublicState, new()
        where TClientState : BaseClientState, new()
    {
        private ClientInputContext helperInputListener;

        protected ProtoItemFishingRod()
        {
            var name = this.GetType().Name;
            this.CharacterTextureResource = new TextureResource(
                "Characters/Tools/" + name,
                isProvidesMagentaPixelPosition: true);
        }

        public virtual TextureResource CharacterTextureResource { get; }

        public abstract Vector2F FishingLineStartScreenOffset { get; }

        public abstract double FishingSpeedMultiplier { get; }

        public Control ClientCreateHotbarOverlayControl(IItem item)
        {
            return new HotbarItemFishingRodOverlayControl(item);
        }

        public virtual void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            ProtoCharacterSkeleton protoCharacterSkeleton,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents)
        {
            protoCharacterSkeleton.ClientSetupItemInHand(
                skeletonRenderer,
                "WeaponMelee",
                this.CharacterTextureResource);
        }

        protected override void ClientItemHotbarSelectionChanged(ClientHotbarItemData data)
        {
            if (data.IsSelected)
            {
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                this.helperInputListener = ClientInputContext
                                           .Start("Current fishing rod item reloading")
                                           .HandleButtonDown(
                                               GameButton.ItemReload,
                                               FishingBaitReloadingSystem.ClientTrySwitchBaitType)
                                           .HandleButtonDown(
                                               GameButton.ItemSwitchMode,
                                               FishingBaitReloadingSystem.ClientTrySwitchBaitType);
            }
            else
            {
                this.helperInputListener?.Stop();
                this.helperInputListener = null;
            }
        }

        protected override void ClientItemUseStart(ClientItemData data)
        {
            if (ClientCurrentCharacterHelper.PrivateState.CurrentActionState
                    is FishingActionState fishingActionState)
            {
                fishingActionState.ClientOnItemUse();
                return;
            }

            FishingSystem.Instance.ClientTryStartAction();
        }

        protected override string GenerateIconPath()
        {
            return "Items/Fishing/" + this.GetType().Name;
        }

        protected sealed override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemGeneric;
        }
    }

    /// <summary>
    /// Base tool item prototype for fishing rods.
    /// </summary>
    public abstract class ProtoItemFishingRod
        : ProtoItemFishingRod
            <ItemWithDurabilityPrivateState,
                ItemFishingRodPublicState,
                EmptyClientState>
    {
    }
}
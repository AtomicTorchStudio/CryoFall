namespace AtomicTorch.CBND.CoreMod.Items.Tools.WateringCans
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Controls;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Input;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Manufacturers;
    using AtomicTorch.CBND.CoreMod.Systems.Watering;
    using AtomicTorch.CBND.CoreMod.Systems.WateringCanRefill;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.HotbarOverlays;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    /// <summary>
    /// Base tool itemWateringCan prototype for watering farm plants.
    /// </summary>
    public abstract class ProtoItemToolWateringCan
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemTool
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemToolWateringCan
        where TPrivateState : ItemWateringCanPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        private ClientInputContext helperInputListener;

        protected ProtoItemToolWateringCan()
        {
            var name = this.GetType().Name;
            this.CharacterTextureResource = new TextureResource(
                "Characters/Tools/" + name,
                isProvidesMagentaPixelPosition: true);
        }

        public virtual double ActionDurationWateringSeconds => 1;

        public virtual TextureResource CharacterTextureResource { get; }

        public abstract byte WaterCapacity { get; }

        public virtual TimeSpan WateringDuration { get; }
            = TimeSpan.FromHours(1);

        public Control ClientCreateHotbarOverlayControl(IItem item)
        {
            return new HotbarItemWateringCanOverlayControl(item);
        }

        public virtual void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            ProtoCharacterSkeleton protoCharacterSkeleton,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents,
            bool isPreview = false)
        {
            protoCharacterSkeleton.ClientSetupItemInHand(
                skeletonRenderer,
                "WeaponMelee",
                this.CharacterTextureResource);
        }

        public bool SharedCanWater(IItem itemWateringCan)
        {
            return this.SharedGetWaterAmount(itemWateringCan) >= 1;
        }

        public byte SharedGetWaterAmount(IItem itemWateringCan)
        {
            var privateState = GetPrivateState(itemWateringCan);
            return privateState.WaterAmount;
        }

        public void SharedOnRefilled(IItem itemWateringCan, byte currentWaterAmount)
        {
            var privateState = GetPrivateState(itemWateringCan);
            privateState.WaterAmount = Math.Min(currentWaterAmount, this.WaterCapacity);
        }

        public void SharedOnWatered(IItem itemWateringCan, IStaticWorldObject staticWorldObject)
        {
            var privateState = GetPrivateState(itemWateringCan);
            var newWaterAmount = privateState.WaterAmount - 1;
            if (newWaterAmount < 0)
            {
                Logger.Error("Water amount cannot be reduced: " + itemWateringCan);
                return;
            }

            privateState.WaterAmount = (byte)newWaterAmount;
        }

        protected override void ClientItemHotbarSelectionChanged(ClientHotbarItemData data)
        {
            if (data.IsSelected)
            {
                // ReSharper disable once CanExtractXamlLocalizableStringCSharp
                this.helperInputListener = ClientInputContext
                                           .Start("Current watering can item")
                                           .HandleButtonDown(
                                               GameButton.ItemReload,
                                               WateringCanRefillSystem.Instance.ClientTryStartAction);
            }
            else
            {
                this.helperInputListener?.Stop();
                this.helperInputListener = null;
            }
        }

        protected override bool ClientItemUseFinish(ClientItemData data)
        {
            WateringSystem.Instance.ClientTryAbortAction();
            // never play watering can "use" sound
            return false;
        }

        protected override void ClientItemUseStart(ClientItemData data)
        {
            // try to find well object
            var objectWell =
                Client.World.TileAtCurrentMousePosition.StaticObjects.FirstOrDefault(
                    _ => _.ProtoStaticWorldObject is ProtoObjectWell);

            if (objectWell is not null
                || data.PrivateState.WaterAmount == 0)
            {
                // try to refill
                WateringCanRefillSystem.Instance.ClientTryStartAction();
                return;
            }

            WateringSystem.Instance.ClientTryStartAction();
        }

        protected override void PrepareHints(List<string> hints)
        {
            base.PrepareHints(hints);
            hints.Add(ItemHints.ClickToRefilWateringCan);
            hints.Add(ItemHints.ClickToWaterPlants);
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemGeneric.Clone()
                                    .Replace(ItemSound.Use,    "Items/Tools/WateringCan/Use")
                                    .Replace(ItemSound.Refill, "Items/Tools/WateringCan/Refill");
        }
    }

    /// <summary>
    /// Base tool itemWateringCan prototype for watering farm plants.
    /// </summary>
    public abstract class ProtoItemToolWateringCan
        : ProtoItemToolWateringCan
            <ItemWateringCanPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}
namespace AtomicTorch.CBND.CoreMod.Items.Tools.Special
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.VehicleRepairKitSystem;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    /// <summary>
    /// Base tool item prototype for repairing vehicles.
    /// </summary>
    public abstract class ProtoItemVehicleRepairKit
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemTool
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemVehicleRepairKit
        where TPrivateState : ItemPrivateState, IItemWithDurabilityPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        protected ProtoItemVehicleRepairKit()
        {
            var name = this.GetType().Name;
            this.CharacterTextureResource = new TextureResource(
                "Characters/Tools/" + name,
                isProvidesMagentaPixelPosition: true);
        }

        public virtual TextureResource CharacterTextureResource { get; }

        public ReadOnlySoundPreset<ObjectSound> ObjectInteractionSoundsPreset { get; private set; }

        public abstract double RepairSpeedMultiplier { get; }

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
            if (!data.IsSelected)
            {
                ConstructionPlacementSystem.ClientDisableConstructionPlacement();
            }
        }

        protected override bool ClientItemUseFinish(ClientItemData data)
        {
            VehicleRepairKitSystem.ClientTryAbortAction();
            // never play "use" sound
            return false;
        }

        protected override void ClientItemUseStart(ClientItemData data)
        {
            VehicleRepairKitSystem.ClientTryStartAction();
        }

        protected override void PrepareHints(List<string> hints)
        {
            base.PrepareHints(hints);
            hints.Add(ItemHints.ClickToRepairVehicle);
        }

        protected sealed override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            this.ObjectInteractionSoundsPreset = this.PrepareSoundPresetToolbox();
            return ItemsSoundPresets.ItemGeneric;
        }

        protected abstract ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetToolbox();
    }

    /// <summary>
    /// Base tool item prototype for repairing vehicles.
    /// </summary>
    public abstract class ProtoItemVehicleRepairKit
        : ProtoItemVehicleRepairKit
            <ItemWithDurabilityPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}
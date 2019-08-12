namespace AtomicTorch.CBND.CoreMod.Items.Tools.Toolboxes
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    /// <summary>
    /// Base tool item prototype for creating constructions.
    /// </summary>
    public abstract class ProtoItemToolToolbox
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemTool
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemToolToolbox
        where TPrivateState : BasePrivateState, IItemWithDurabilityPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        protected ProtoItemToolToolbox()
        {
            var name = this.GetType().Name;
            this.Icon = new TextureResource("Items/Tools/Toolboxes/" + name);
            this.CharacterTextureResource = new TextureResource(
                "Characters/Tools/Toolboxes/" + name,
                isProvidesMagentaPixelPosition: true);
        }

        public virtual TextureResource CharacterTextureResource { get; }

        public abstract double ConstructionSpeedMultiplier { get; }

        public override ITextureResource Icon { get; }

        public ReadOnlySoundPreset<ObjectSound> ObjectInteractionSoundsPreset { get; private set; }

        public virtual void ClientSetupSkeleton(
            IItem item,
            ICharacter character,
            IComponentSkeleton skeletonRenderer,
            List<IClientComponent> skeletonComponents)
        {
            ClientSkeletonItemInHandHelper.Setup(
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
            ConstructionSystem.ClientTryAbortAction();
            // never play "use" sound
            return false;
        }

        protected override void ClientItemUseStart(ClientItemData data)
        {
            ConstructionSystem.ClientTryStartAction();
        }

        protected sealed override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            this.ObjectInteractionSoundsPreset = this.PrepareSoundPresetToolbox();
            return ItemsSoundPresets.ItemGeneric;
        }

        protected abstract ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetToolbox();
    }

    /// <summary>
    /// Base tool item prototype for creating constructions.
    /// </summary>
    public abstract class ProtoItemToolToolbox
        : ProtoItemToolToolbox
            <ItemWithDurabilityPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}
namespace AtomicTorch.CBND.CoreMod.Items.Tools.Crowbars
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.CharacterSkeletons;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Deconstruction;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    /// <summary>
    /// Base tool item prototype for deconstruction of structures.
    /// </summary>
    public abstract class ProtoItemToolCrowbar
        <TPrivateState,
         TPublicState,
         TClientState>
        : ProtoItemTool
          <TPrivateState,
              TPublicState,
              TClientState>,
          IProtoItemToolCrowbar
        where TPrivateState : BasePrivateState, IItemWithDurabilityPrivateState, new()
        where TPublicState : BasePublicState, new()
        where TClientState : BaseClientState, new()
    {
        protected ProtoItemToolCrowbar()
        {
            var name = this.GetType().Name;
            this.CharacterTextureResource = new TextureResource(
                "Characters/Tools/" + name,
                isProvidesMagentaPixelPosition: true);
        }

        public virtual TextureResource CharacterTextureResource { get; }

        public abstract double DeconstructionSpeedMultiplier { get; }

        public ReadOnlySoundPreset<ObjectSound> ObjectInteractionSoundsPreset { get; private set; }

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

        protected override bool ClientItemUseFinish(ClientItemData data)
        {
            DeconstructionSystem.ClientTryAbortAction();
            // never play crowbar "use" sound
            return false;
        }

        protected override void ClientItemUseStart(ClientItemData data)
        {
            DeconstructionSystem.ClientTryStartAction();
        }

        protected override void PrepareHints(List<string> hints)
        {
            base.PrepareHints(hints);
            hints.Add(ItemHints.ClickToDisasseble);
        }

        protected abstract ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetCrowbar();

        protected sealed override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            this.ObjectInteractionSoundsPreset = this.PrepareSoundPresetCrowbar();
            return ItemsSoundPresets.ItemGeneric;
        }
    }

    /// <summary>
    /// Base tool item prototype for deconstruction of structures.
    /// </summary>
    public abstract class ProtoItemToolCrowbar
        : ProtoItemToolCrowbar
            <ItemWithDurabilityPrivateState,
                EmptyPublicState,
                EmptyClientState>
    {
    }
}
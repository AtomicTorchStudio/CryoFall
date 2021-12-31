namespace AtomicTorch.CBND.CoreMod.Systems.ItemFuelRefill
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class ItemFuelRefillActionState
        : BaseSystemActionState<
            ItemFuelRefillSystem,
            ItemFuelRefillRequest,
            ItemFuelRefillActionState,
            ItemFuelRefillActionState.PublicState>
    {
        public readonly IItem Item;

        public readonly IReadOnlyList<IItem> ItemsToConsumeForRefill;

        public ItemFuelRefillActionState(
            ICharacter character,
            double durationSeconds,
            IItem item,
            IReadOnlyList<IItem> itemsToConsumeForRefill)
            : base(character, null, durationSeconds)
        {
            this.Item = item;
            this.ItemsToConsumeForRefill = itemsToConsumeForRefill;
        }

        public override bool IsDisplayingProgress => true;

        protected override void OnCompletedOrCancelled()
        {
            base.OnCompletedOrCancelled();

            if (Api.IsClient)
            {
                (this.ItemsToConsumeForRefill[0].ProtoItem as IProtoItemWithSoundPreset)
                    ?.SoundPresetItem?.PlaySound(ItemSound.Use);
            }
        }

        public class PublicState : BasePublicActionState
        {
            protected override void ClientOnCompleted()
            {
            }

            protected override void ClientOnStart()
            {
            }
        }
    }
}
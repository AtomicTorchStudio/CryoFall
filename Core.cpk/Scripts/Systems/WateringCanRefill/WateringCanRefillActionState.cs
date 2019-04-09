namespace AtomicTorch.CBND.CoreMod.Systems.WateringCanRefill
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Tools.WateringCans;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class WateringCanRefillActionState
        : ActionSystemState<
            WateringCanRefillSystem,
            WateringCanRefillRequest,
            WateringCanRefillActionState,
            WateringCanRefillActionState.PublicState>
    {
        public readonly IReadOnlyList<IItem> ItemsToConsumeForRefill;

        public readonly IItem ItemWateringCan;

        public WateringCanRefillActionState(
            ICharacter character,
            IWorldObject targetWorldObject,
            double durationSeconds,
            IItem itemWateringCan,
            IReadOnlyList<IItem> itemsToConsumeForRefill)
            : base(character, targetWorldObject, durationSeconds)
        {
            this.ItemWateringCan = itemWateringCan;
            this.ItemsToConsumeForRefill = itemsToConsumeForRefill;
        }

        public class PublicState : BasePublicActionState
        {
            protected override void ClientOnCompleted()
            {
                if (this.IsCancelled)
                {
                    return;
                }

                if (this.Character.SharedGetPlayerSelectedHotbarItemProto()
                        is IProtoItemToolWateringCan protoWateringCan)
                {
                    protoWateringCan.SharedGetItemSoundPreset()
                                    .PlaySound(ItemSound.Refill,
                                               this.Character,
                                               pitch: RandomHelper.Range(0.95f, 1.05f));
                }
            }

            protected override void ClientOnStart()
            {
                // TODO: play animation
            }
        }
    }
}
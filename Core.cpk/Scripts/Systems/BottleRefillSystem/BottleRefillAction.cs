namespace AtomicTorch.CBND.CoreMod.Systems.BottleRefillSystem
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Items.Tools.WateringCans;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class BottleRefillAction
        : BaseSystemActionState<
            BottleRefillSystem,
            BottleRefillRequest,
            BottleRefillAction,
            BottleRefillAction.PublicState>
    {
        public readonly IItem ItemEmptyBottle;

        public readonly IProtoTileWater WaterProtoTileToRefill;

        public BottleRefillAction(
            ICharacter character,
            double durationSeconds,
            IItem itemEmptyBottle,
            IProtoTileWater waterProtoTileToRefill)
            : base(character, targetWorldObject: null, durationSeconds)
        {
            this.ItemEmptyBottle = itemEmptyBottle;
            this.WaterProtoTileToRefill = waterProtoTileToRefill;
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
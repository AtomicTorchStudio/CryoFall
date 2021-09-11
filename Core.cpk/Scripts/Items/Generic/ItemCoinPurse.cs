namespace AtomicTorch.CBND.CoreMod.Items.Generic
{
    using System;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.GameApi.Data.Characters;

    public class ItemCoinPurse : ProtoItemConsumable
    {
        private IReadOnlyDropItemsList droplist;

        public override string Description => "Old coin purse. Maybe it still contains something?";

        public override string ItemUseCaption => ItemUseCaptions.Open;

        public override string Name => "Coin purse";

        protected override void PrepareProtoItemConsumable()
        {
            base.PrepareProtoItemConsumable();

            this.droplist = new DropItemsList()
                            // penny coins 20-50
                            .Add<ItemCoinPenny>(count: 20, countRandom: 30)
                            // shiny coins 1-5 with 20% chance
                            .Add<ItemCoinShiny>(count: 1, countRandom: 4, probability: 0.2);
        }

        protected override ReadOnlySoundPreset<ItemSound> PrepareSoundPresetItem()
        {
            return ItemsSoundPresets.ItemGeneric;
        }

        protected override void ServerOnUse(ICharacter character, PlayerCharacterCurrentStats currentStats)
        {
            var createItemResult = this.droplist.TryDropToCharacterOrGround(
                character,
                character.TilePosition,
                new DropItemContext(character),
                out _,
                probabilityMultiplier: RateResourcesGatherBasic.SharedValue);

            if (!createItemResult.IsEverythingCreated)
            {
                createItemResult.Rollback();
                throw new Exception("Not enough space");
            }

            NotificationSystem.ServerSendItemsNotification(character, createItemResult);
        }
    }
}
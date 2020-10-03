namespace AtomicTorch.CBND.CoreMod.Systems.BottleRefillSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.Items.Food;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Notifications;
    using AtomicTorch.CBND.CoreMod.Tiles;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class BottleRefillSystem
        : ProtoActionSystem<
            BottleRefillSystem,
            BottleRefillRequest,
            BottleRefillAction,
            BottleRefillAction.PublicState>
    {
        public const string NotificationStandNearWaterToRefill =
            "Stand near water to fill the bottle or put it in a well.";

        private const double ActionDuration = 1;

        private static readonly Lazy<IProtoItem> ProtoItemBottleEmpty
            = new Lazy<IProtoItem>(GetProtoEntity<ItemBottleEmpty>);

        public override string Name => "Bottle refill system";

        protected override BottleRefillRequest ClientTryCreateRequest(ICharacter character)
        {
            var item = character.SharedGetPlayerSelectedHotbarItem();
            if (item is null
                || item.ProtoItem != ProtoItemBottleEmpty.Value)
            {
                // no bottle selected
                return null;
            }

            var tile = character.Tile;
            if (!(tile.ProtoTile is IProtoTileWater protoTileWater)
                || !protoTileWater.CanCollect)
            {
                // pointed tile is not a water - find a neighbor tile with water
                protoTileWater = tile.EightNeighborTiles
                                     .FirstOrDefault(t => t.ProtoTile is IProtoTileWater p
                                                          && p.CanCollect)
                                     .ProtoTile as IProtoTileWater;
            }

            if (protoTileWater is null
                || SharedGetFilledBottlePrototype(protoTileWater) is null)
            {
                // not a compatible water tile
                ClientShowNeedWaterTileNotification(item);
                return null;
            }

            return new BottleRefillRequest(character, item, protoTileWater);
        }

        protected override void SharedOnActionCompletedInternal(BottleRefillAction state, ICharacter character)
        {
            var itemBottle = state.ItemEmptyBottle;

            var requiredWaterProtoTile = state.WaterProtoTileToRefill;
            if (requiredWaterProtoTile is null)
            {
                throw new Exception("Impossible");
            }

            if (IsClient)
            {
                return;
            }

            // destroy the empty bottle item stack
            var itemsContainer = itemBottle.Container;
            var countToSpawn = itemBottle.Count;
            var containerSlotId = itemBottle.ContainerSlotId;
            Server.Items.SetCount(itemBottle,
                                  count: 0,
                                  byCharacter: character);

            // spawn a filled bottle item stack in place of the destroyed empty bottle item stack
            var protoItemFilledBottle = SharedGetFilledBottlePrototype(requiredWaterProtoTile);
            if (protoItemFilledBottle is null)
            {
                throw new Exception("Should be impossible");
            }

            var result = Server.Items.CreateItem(protoItemFilledBottle, itemsContainer, countToSpawn, containerSlotId);
            var itemsChangedCount = NotificationSystem.SharedGetItemsChangedCount(result);
            this.CallClient(character,
                            _ => _.ClientRemote_ActionCompleted(itemsChangedCount));
        }

        protected override BottleRefillAction SharedTryCreateState(BottleRefillRequest request)
        {
            return new BottleRefillAction(
                request.Character,
                durationSeconds: ActionDuration,
                itemEmptyBottle: request.Item,
                request.WaterProtoTileToRefill);
        }

        protected override void SharedValidateRequest(BottleRefillRequest request)
        {
            var character = request.Character;
            var itemBottle = request.Item;
            if (itemBottle != character.SharedGetPlayerSelectedHotbarItem())
            {
                throw new Exception("The item is not selected");
            }

            if (itemBottle.ProtoItem != ProtoItemBottleEmpty.Value)
            {
                throw new Exception("The item must be an empty bottle");
            }

            if (IsServer
                && !Server.Core.IsInPrivateScope(character, itemBottle))
            {
                throw new Exception(
                    $"{character} cannot access {itemBottle} because it's container is not in the private scope");
            }

            // ensure that the required proto tile is around the player
            var requiredWaterProtoTile = request.WaterProtoTileToRefill;
            var tile = character.Tile;
            if (tile.ProtoTile != requiredWaterProtoTile
                && tile.EightNeighborTiles.All(t => t.ProtoTile != requiredWaterProtoTile))
            {
                if (IsClient)
                {
                    ClientShowNeedWaterTileNotification(itemBottle);
                }

                throw new Exception("No tiles of the required type found around");
            }
        }

        private static void ClientShowNeedWaterTileNotification(IItem item)
        {
            NotificationSystem.ClientShowNotification(
                NotificationStandNearWaterToRefill,
                color: NotificationColor.Bad,
                icon: item.ProtoItem.Icon);
        }

        private static IProtoItem SharedGetFilledBottlePrototype(IProtoTileWater protoTile)
        {
            if (protoTile is null
                || !protoTile.CanCollect)
            {
                return null;
            }

            return protoTile switch
            {
                TileWaterLake => GetProtoEntity<ItemBottleWaterStale>(),
                TileWaterSea  => GetProtoEntity<ItemBottleWaterSalty>(),
                _             => null
            };
        }

        [RemoteCallSettings(DeliveryMode.ReliableUnordered)]
        private void ClientRemote_ActionCompleted(Dictionary<IProtoItem, int> itemsChangedCount)
        {
            // play refill sound
            Client.Audio.PlayOneShot(new SoundResource("Items/Tools/WateringCan/Refill"));
            // display the removed empty bottle notification
            NotificationSystem.ClientShowItemsNotification(new Dictionary<IProtoItem, int>()
                                                               { { ProtoItemBottleEmpty.Value, -1 } });
            // display the added water bottle notification
            NotificationSystem.ClientShowItemsNotification(itemsChangedCount);
        }
    }
}
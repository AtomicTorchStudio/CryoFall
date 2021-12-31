namespace AtomicTorch.CBND.CoreMod.Systems.TradingStations
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class TradingStationsMapMarksSystem : ProtoSystem<TradingStationsMapMarksSystem>
    {
        public static readonly SuperObservableCollection<TradingStationMark> ClientTradingStationMarksList
            = IsClient
                  ? new SuperObservableCollection<TradingStationMark>()
                  : null;

        private static readonly HashSet<IStaticWorldObject> ServerActiveTradingStations
            = IsServer
                  ? new HashSet<IStaticWorldObject>()
                  : null;

        public static Task<TradingStationInfo> ClientRequestTradingStationInfo(uint tradingStationId)
        {
            return Instance.CallServer(_ => _.ServerRemote_GetTradingStationInfo(tradingStationId));
        }

        public static void ServerRefreshMark(IStaticWorldObject tradingStation)
        {
            if (ServerIsTradingStationHasActiveLots(tradingStation))
            {
                ServerTryAddMark(tradingStation);
                return;
            }

            ServerTryRemoveMark(tradingStation);
        }

        public static void ServerTryAddMark(IStaticWorldObject tradingStation)
        {
            var owners = WorldObjectOwnersSystem.SharedGetDirectOwners(tradingStation);
            if (owners.Count == 0)
            {
                // no owners - incomplete trading station
                // (first owner set when the trading station instance spawned and ServerOnBuilt method called in ProtoTradingStation)
                return;
            }

            if (!ServerIsTradingStationHasActiveLots(tradingStation))
            {
                return;
            }

            if (!ServerActiveTradingStations.Add(tradingStation))
            {
                // already added
                return;
            }

            var allOnlinePlayers = Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true);
            foreach (var onlinePlayer in allOnlinePlayers)
            {
                var isOwner = owners.Contains(onlinePlayer.Name);
                var mark = new TradingStationMark(tradingStation.Id,
                                                  tradingStation.TilePosition,
                                                  isOwner);
                Instance.CallClient(onlinePlayer,
                                    _ => _.ClientRemote_MarkAdded(mark));
            }
        }

        public static void ServerTryRemoveMark(IStaticWorldObject tradingStation)
        {
            if (!ServerActiveTradingStations.Remove(tradingStation))
            {
                // don't have a mark for this trading station
                return;
            }

            var allOnlinePlayers = Server.Characters.EnumerateAllPlayerCharacters(onlyOnline: true);
            Instance.CallClient(allOnlinePlayers,
                                _ => _.ClientRemote_MarkRemoved(tradingStation.Id));
        }

        protected override void PrepareSystem()
        {
            base.PrepareSystem();
            WorldObjectOwnersSystem.ServerOwnersChanged += this.WorldObjectOwnersSystemOwnersChangedHandler;
        }

        private static bool ServerIsTradingStationHasActiveLots(IStaticWorldObject tradingStation)
        {
            var publicState = tradingStation.GetPublicState<ObjectTradingStationPublicState>();
            foreach (var lot in publicState.Lots)
            {
                if (lot.State == TradingStationLotState.Available)
                {
                    return true;
                }
            }

            return false;
        }

        [RemoteCallSettings(DeliveryMode.ReliableOrdered, groupName: nameof(TradingStationsMapMarksSystem))]
        private void ClientRemote_MarkAdded(TradingStationMark mark)
        {
            ClientTradingStationMarksList.Add(mark);
        }

        [RemoteCallSettings(DeliveryMode.ReliableOrdered, groupName: nameof(TradingStationsMapMarksSystem))]
        private void ClientRemote_MarkRemoved(uint tradingStationId)
        {
            var mark = ClientTradingStationMarksList.First(m => m.TradingStationId == tradingStationId);
            ClientTradingStationMarksList.Remove(mark);
        }

        [RemoteCallSettings(DeliveryMode.ReliableOrdered, groupName: nameof(TradingStationsMapMarksSystem))]
        private void ClientRemote_MarksRequestResult(List<TradingStationMark> marks)
        {
            foreach (var mark in marks)
            {
                ClientTradingStationMarksList.Add(mark);
            }
        }

        [RemoteCallSettings(DeliveryMode.ReliableUnordered,
                            timeInterval: 0.05,
                            clientMaxSendQueueSize: 100)]
        private TradingStationInfo ServerRemote_GetTradingStationInfo(uint tradingStationId)
        {
            foreach (var tradingStation in ServerActiveTradingStations)
            {
                if (tradingStation.Id != tradingStationId)
                {
                    continue;
                }

                var publicState = tradingStation.GetPublicState<ObjectTradingStationPublicState>();
                var activeLots = new List<TradingStationLotInfo>(publicState.Lots.Count);
                foreach (var lot in publicState.Lots)
                {
                    if (lot.State == TradingStationLotState.Available)
                    {
                        activeLots.Add(new TradingStationLotInfo(lot));
                    }
                }

                return new TradingStationInfo(isBuying: publicState.Mode == TradingStationMode.StationBuying,
                                              activeLots);
            }

            return default;
        }

        [RemoteCallSettings(DeliveryMode.ReliableOrdered, groupName: nameof(TradingStationsMapMarksSystem))]
        private void ServerRemote_RequestMarks()
        {
            var character = ServerRemoteContext.Character;

            var result = new List<TradingStationMark>(capacity: ServerActiveTradingStations.Count);

            // add to the character private scope all owned areas
            foreach (var tradingStation in ServerActiveTradingStations)
            {
                var isOwner = WorldObjectOwnersSystem.SharedIsOwner(character, tradingStation);
                result.Add(new TradingStationMark(tradingStation.Id,
                                                  tradingStation.TilePosition,
                                                  isOwner));
            }

            this.CallClient(character, _ => _.ClientRemote_MarksRequestResult(result));
        }

        private void WorldObjectOwnersSystemOwnersChangedHandler(IWorldObject worldObject)
        {
            if (worldObject.ProtoGameObject is not IProtoObjectTradingStation)
            {
                // not a trading station
                return;
            }

            // refresh the mark for all players - remove and add the mark
            ServerTryRemoveMark((IStaticWorldObject)worldObject);
            ServerTryAddMark((IStaticWorldObject)worldObject);
        }

        [NotPersistent]
        public readonly struct TradingStationInfo : IRemoteCallParameter
        {
            public readonly IReadOnlyList<TradingStationLotInfo> ActiveLots;

            public readonly bool IsBuying;

            public TradingStationInfo(bool isBuying, IReadOnlyList<TradingStationLotInfo> activeLots)
            {
                this.IsBuying = isBuying;
                this.ActiveLots = activeLots;
            }
        }

        [NotPersistent]
        public readonly struct TradingStationLotInfo : IRemoteCallParameter
        {
            public TradingStationLotInfo(TradingStationLot lot)
            {
                this.ProtoItem = lot.ItemOnSale?.ProtoItem ?? lot.ProtoItem;
                this.State = lot.State;
                this.LotQuantity = lot.LotQuantity;
                this.CountAvailable = lot.CountAvailable;
                this.PriceCoinShiny = lot.PriceCoinShiny;
                this.PriceCoinPenny = lot.PriceCoinPenny;
            }

            public uint CountAvailable { get; }

            public ushort LotQuantity { get; }

            public ushort PriceCoinPenny { get; }

            public ushort PriceCoinShiny { get; }

            public IProtoItem ProtoItem { get; }

            public TradingStationLotState State { get; }
        }

        public readonly struct TradingStationMark : IRemoteCallParameter
        {
            public readonly bool IsOwner;

            public readonly Vector2Ushort TilePosition;

            public readonly uint TradingStationId;

            public TradingStationMark(uint tradingStationId, Vector2Ushort tilePosition, bool isOwner)
            {
                this.TradingStationId = tradingStationId;
                this.TilePosition = tilePosition;
                this.IsOwner = isOwner;
            }
        }

        public class BootstrapperTradingStationsMapMarksSystem : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                Client.Characters.CurrentPlayerCharacterChanged += ClientTryRequestMarks;

                ClientTryRequestMarks();

                void ClientTryRequestMarks()
                {
                    ClientTradingStationMarksList.Clear();
                    if (Api.Client.Characters.CurrentPlayerCharacter is not null)
                    {
                        Instance.CallServer(_ => _.ServerRemote_RequestMarks());
                    }
                }
            }
        }
    }
}
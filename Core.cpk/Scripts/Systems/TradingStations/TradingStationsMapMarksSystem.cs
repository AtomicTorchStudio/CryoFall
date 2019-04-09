namespace AtomicTorch.CBND.CoreMod.Systems.TradingStations
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Bootstrappers;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.TradingStations;
    using AtomicTorch.CBND.CoreMod.Systems.WorldObjectOwners;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core.Data;
    using AtomicTorch.CBND.GameApi.Data;
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

        private static readonly List<IStaticWorldObject> ServerTradingStationsList
            = IsServer
                  ? new List<IStaticWorldObject>()
                  : null;

        public override string Name => "Trading stations map marks system";

        public static void ServerAddMark(IStaticWorldObject tradingStation)
        {
            var owners = WorldObjectOwnersSystem.SharedGetOwners(tradingStation);
            if (owners.Count == 0)
            {
                // no owners - incomplete trading station
                // (first owner set when the trading station instance spawned and ServerOnBuilt method called in ProtoTradingStation)
                return;
            }

            ServerTradingStationsList.Add(tradingStation);

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

        public static void ServerRemoveMark(IStaticWorldObject tradingStation)
        {
            if (!ServerTradingStationsList.Remove(tradingStation))
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

        [RemoteCallSettings(DeliveryMode.ReliableOrdered, groupName: nameof(TradingStationsMapMarksSystem))]
        private void ServerRemote_RequestMarks()
        {
            var character = ServerRemoteContext.Character;

            var result = new List<TradingStationMark>(capacity: ServerTradingStationsList.Count);

            // add to the character private scope all owned areas
            foreach (var tradingStation in ServerTradingStationsList)
            {
                var isOwner = WorldObjectOwnersSystem.SharedIsOwner(character, tradingStation);
                result.Add(new TradingStationMark(tradingStation.Id,
                                                  tradingStation.TilePosition,
                                                  isOwner));
            }

            this.CallClient(character, _ => _.ClientRemote_MarksRequestResult(result));
        }

        private void WorldObjectOwnersSystemOwnersChangedHandler(IStaticWorldObject worldObject)
        {
            if (!(worldObject.ProtoStaticWorldObject is IProtoObjectTradingStation))
            {
                // not a trading station
                return;
            }

            // refresh the mark for all players - remove and add the mark
            ServerRemoveMark(worldObject);
            ServerAddMark(worldObject);
        }

        public struct TradingStationMark : IRemoteCallParameter
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
                    if (Api.Client.Characters.CurrentPlayerCharacter != null)
                    {
                        Instance.CallServer(_ => _.ServerRemote_RequestMarks());
                    }
                }
            }
        }
    }
}
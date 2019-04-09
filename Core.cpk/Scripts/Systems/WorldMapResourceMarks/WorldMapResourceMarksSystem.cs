namespace AtomicTorch.CBND.CoreMod.Systems.WorldMapResourceMarks
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Bootstrappers;
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;

    public class WorldMapResourceMarksSystem : ProtoSystem<WorldMapResourceMarksSystem>
    {
        private static ILogicObject serverManagerInstance;

        private static NetworkSyncList<WorldMapResourceMark> sharedResourceMarksList;

        public static event Action<WorldMapResourceMark> ClientMarkAdded;

        public static event Action<WorldMapResourceMark> ClientMarkRemoved;

        public override string Name => "World map resource marks system";

        public static IEnumerable<WorldMapResourceMark> ClientEnumerateMarks()
        {
            if (sharedResourceMarksList == null)
            {
                yield break;
            }

            foreach (var entry in sharedResourceMarksList)
            {
                yield return entry;
            }
        }

        public static void ServerAddMark(IStaticWorldObject staticWorldObject)
        {
            Api.ValidateIsServer();

            sharedResourceMarksList.Add(new WorldMapResourceMark(staticWorldObject.TilePosition,
                                                                 staticWorldObject.ProtoStaticWorldObject));
        }

        public static void ServerRemoveMark(IStaticWorldObject staticWorldObject)
        {
            Api.ValidateIsServer();
            var protoStaticWorldObject = staticWorldObject.ProtoStaticWorldObject;
            var position = staticWorldObject.TilePosition;

            // find and remove the mark
            for (var index = 0; index < sharedResourceMarksList.Count; index++)
            {
                var mark = sharedResourceMarksList[index];
                if (mark.ProtoWorldObject == protoStaticWorldObject
                    && mark.Position == position)
                {
                    sharedResourceMarksList.RemoveAt(index);
                    return;
                }
            }
        }

        private ILogicObject ServerRemote_AcquireManagerInstance()
        {
            var character = ServerRemoteContext.Character;
            Logger.Important("World map resources requested from server");
            Server.World.ForceEnterScope(character, serverManagerInstance);
            return serverManagerInstance;
        }

        [PrepareOrder(afterType: typeof(BootstrapperServerCore))]
        public class BootstrapperWorldMapResourcesSystem : BaseBootstrapper
        {
            public override void ClientInitialize()
            {
                Client.Characters.CurrentPlayerCharacterChanged += ClientTryRequestWorldResourcesAsync;

                ClientTryRequestWorldResourcesAsync();

                async void ClientTryRequestWorldResourcesAsync()
                {
                    if (Api.Client.Characters.CurrentPlayerCharacter == null)
                    {
                        return;
                    }

                    Logger.Important("World map resource marks requested from server");

                    var managerInstance = await Instance.CallServer(_ => _.ServerRemote_AcquireManagerInstance());
                    var marksList = WorldMapResourceMarksManager.GetPublicState(managerInstance)
                                                                .Marks;
                    Logger.Important($"World map resource marks received from server: {marksList.Count} marks total");

                    //ClientWorldMapResourceMarksManager.SetAreas(list);
                    if (sharedResourceMarksList != null)
                    {
                        var onRemoved = ClientMarkRemoved;
                        if (onRemoved != null)
                        {
                            foreach (var mark in sharedResourceMarksList)
                            {
                                onRemoved.Invoke(mark);
                            }
                        }

                        sharedResourceMarksList.ClientElementInserted -= this.ClientMarksListElementInsertedHandler;
                        sharedResourceMarksList.ClientElementRemoved -= this.ClientMarksListElementRemovedHandler;
                    }

                    sharedResourceMarksList = marksList;
                    sharedResourceMarksList.ClientElementInserted += this.ClientMarksListElementInsertedHandler;
                    sharedResourceMarksList.ClientElementRemoved += this.ClientMarksListElementRemovedHandler;

                    var onAdded = ClientMarkAdded;
                    if (onAdded != null)
                    {
                        foreach (var mark in sharedResourceMarksList)
                        {
                            onAdded.Invoke(mark);
                        }
                    }
                }
            }

            public override void ServerInitialize(IServerConfiguration serverConfiguration)
            {
                Server.World.WorldBoundsChanged += this.ServerWorldBoundsChangedHandler;

                ServerLoadSystem();
            }

            private static void ServerLoadSystem()
            {
                const string key = nameof(WorldMapResourceMarksManager);
                if (Server.Database.TryGet(key, key, out ILogicObject savedManager))
                {
                    Server.World.DestroyObject(savedManager);
                }

                serverManagerInstance = Server.World.CreateLogicObject<WorldMapResourceMarksManager>();
                Server.Database.Set(key, key, serverManagerInstance);

                var publicState = WorldMapResourceMarksManager.GetPublicState(serverManagerInstance);
                sharedResourceMarksList = new NetworkSyncList<WorldMapResourceMark>();
                publicState.Marks = sharedResourceMarksList;
            }

            private void ClientMarksListElementInsertedHandler(
                NetworkSyncList<WorldMapResourceMark> source,
                int index,
                WorldMapResourceMark value)
            {
                ClientMarkAdded?.Invoke(value);
            }

            private void ClientMarksListElementRemovedHandler(
                NetworkSyncList<WorldMapResourceMark> source,
                int index,
                WorldMapResourceMark removedValue)
            {
                ClientMarkRemoved?.Invoke(removedValue);
            }

            private void ServerWorldBoundsChangedHandler()
            {
                const string key = nameof(WorldMapResourceMarksManager);
                Server.Database.Remove(key, key);
                Server.World.DestroyObject(serverManagerInstance);
                ServerLoadSystem();
            }
        }
    }
}
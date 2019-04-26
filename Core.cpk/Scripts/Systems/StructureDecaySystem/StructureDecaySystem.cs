namespace AtomicTorch.CBND.CoreMod.Systems.StructureDecaySystem
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Triggers;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// This system iterates over all the world objects on the server and applies decay
    /// when the decay conditions are satisfied.
    /// </summary>
    public sealed class StructureDecaySystem : ProtoSystem<StructureDecaySystem>
    {
        private static readonly ICoreServerService ServerCore
            = IsServer ? Server.Core : null;

        private static readonly IGameServerService ServerGame
            = IsServer ? Server.Game : null;

        private static readonly IWorldServerService ServerWorld
            = IsServer ? Server.World : null;

        private static readonly List<IStaticWorldObject> TempList
            = new List<IStaticWorldObject>(capacity: 100000);

        private static bool isUpdatingNow;

        public override string Name => "Structure decay system";

        /// <summary>
        /// This method is usually used when a land claim building is destroyed
        /// - to force all the buildings to start decay immediately.
        /// </summary>
        public static void ServerBeginDecayForStructuresInArea(RectangleInt areaBounds)
        {
            var decayStartTime = ServerGame.FrameTime - StructureConstants.StructuresDecayDelaySeconds;

            for (var x = areaBounds.X; x < areaBounds.X + areaBounds.Width; x++)
            for (var y = areaBounds.Y; y < areaBounds.Y + areaBounds.Height; y++)
            {
                if (x < 0
                    || y < 0
                    || x >= ushort.MaxValue
                    || y >= ushort.MaxValue)
                {
                    continue;
                }

                var staticObjects = ServerWorld.GetStaticObjects(new Vector2Ushort((ushort)x, (ushort)y));
                foreach (var worldObject in staticObjects)
                {
                    if (worldObject.ProtoStaticWorldObject is IProtoObjectStructure)
                    {
                        var privateState = worldObject.GetPrivateState<StructurePrivateState>();
                        privateState.ServerDecayStartTime = decayStartTime;
                    }
                }
            }
        }

        public static void ServerResetDecayTimer(StructurePrivateState privateState)
        {
            privateState.ServerDecayStartTime = ServerGame.FrameTime
                                                + StructureConstants.StructuresDecayDelaySeconds;
        }

        protected override void PrepareSystem()
        {
            if (IsClient)
            {
                // only server will update structure decay
                return;
            }

            // configure time interval trigger
            TriggerTimeInterval.ServerConfigureAndRegister(
                interval: TimeSpan.FromSeconds(StructureConstants.StructureDecaySystemUpdateIntervalSeconds),
                callback: this.ServerTimerTickCallback,
                name: "System." + this.ShortId);
        }

        private static bool ServerProcessDecay(IStaticWorldObject worldObject, double serverTime)
        {
            if (worldObject.IsDestroyed)
            {
                return false;
            }

            var protoObjectStructure = worldObject.ProtoStaticWorldObject as IProtoObjectStructure;
            if (protoObjectStructure == null)
            {
                // not a structure
                return false;
            }

            var privateState = worldObject.GetPrivateState<StructurePrivateState>();
            if (serverTime < privateState.ServerDecayStartTime)
            {
                // this structure is not decaying
                return false;
            }

            // need to decay this object
            if (LandClaimSystem.SharedIsLandClaimedByAnyone(worldObject.TilePosition))
            {
                // the object is in a land claim area
                if (!(worldObject.ProtoStaticWorldObject is IProtoObjectLandClaim))
                {
                    // only the land claim object can decay in the land claim area
                    return false;
                }

                // this is a land claim object so it could decay
                // but first, ensure that it will not decay if there are any online owners nearby
                StructureLandClaimDecayResetSystem.ServerRefreshLandClaimObject(worldObject);
                if (serverTime < privateState.ServerDecayStartTime)
                {
                    // this land claim object is not decaying anymore (the timer has been just reset)
                    return false;
                }
            }

            protoObjectStructure.ServerApplyDecay(
                worldObject,
                deltaTime: StructureConstants.StructureDecaySystemUpdateIntervalSeconds);

            return true;
        }

        // refresh structures decay
        private async void ServerTimerTickCallback()
        {
            if (isUpdatingNow)
            {
                Logger.Warning("Cannot process structure decay system tick - not finished the previous update yet");
                return;
            }

            if (!StructureConstants.IsStructuresDecayEnabled)
            {
                return;
            }

            // It's prohibitively expensive to iterate over all the server world objects at a single time
            // so we will time-slice this update by gathering all the static world objects first
            // and then iterating over them with YieldIfOutTime().
            var serverTime = ServerGame.FrameTime;
            isUpdatingNow = true;

            try
            {
                // TODO: this call might be too expensive if there are many built structures in the world
                TempList.AddRange(Server.World.FindStaticWorldObjectsOfProto<IProtoObjectStructure>());
                var objectsDecayedCount = 0;

                foreach (var worldObject in TempList)
                {
                    if (ServerProcessDecay(worldObject, serverTime))
                    {
                        objectsDecayedCount++;
                        await ServerCore.YieldIfOutOfTime();
                    }
                }

                var timeSpent = ServerGame.FrameTime - serverTime;
                Logger.Info(
                    string.Format(
                        "World decay updated. Total static world objects count: {0}. Decaying objects count: {1}. Total time spent (including time-slicing): {2:F2}s",
                        TempList.Count,
                        objectsDecayedCount,
                        timeSpent));
            }
            finally
            {
                isUpdatingNow = false;
                TempList.Clear();
            }
        }
    }
}
namespace AtomicTorch.CBND.CoreMod.Systems.StructureDecaySystem
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Rates;
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
        private static readonly IGameServerService ServerGame
            = IsServer ? Server.Game : null;

        private static readonly IWorldServerService ServerWorld
            = IsServer ? Server.World : null;

        private static readonly List<IStaticWorldObject> TempList
            = new(capacity: 100000);

        private static IProtoObjectStructure[] serverListProtoStructuresToApplyDecay;

        public override string Name => "Structure decay system";

        /// <summary>
        /// This method is usually used when a land claim building is destroyed
        /// - to force all the buildings to start decay almost immediately.
        /// </summary>
        public static void ServerBeginDecayForStructuresInArea(RectangleInt areaBounds)
        {
            var decayStartTime = ServerGame.FrameTime + (5 * 60);

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
                    var prototype = worldObject.ProtoStaticWorldObject;
                    if (prototype is IProtoObjectStructure
                            and not IProtoObjectLandClaim)
                    {
                        var privateState = worldObject.GetPrivateState<StructurePrivateState>();
                        privateState.ServerDecayStartTime = decayStartTime;
                    }
                }
            }
        }

        public static void ServerResetDecayTimer(
            StructurePrivateState privateState,
            double decayDelaySeconds)
        {
            privateState.ServerDecayStartTime = ServerGame.FrameTime + decayDelaySeconds;
        }

        protected override void PrepareSystem()
        {
            if (IsClient
                || !RateStructuresDecayEnabled.SharedValue)
            {
                return;
            }

            serverListProtoStructuresToApplyDecay = Api.FindProtoEntities<IProtoObjectStructure>()
                                                       .ToArray();
            TriggerEveryFrame.ServerRegister(callback: ServerUpdate,
                                             name: "System." + this.ShortId);
        }

        private static bool ServerProcessDecay(IStaticWorldObject worldObject, double serverTime, double deltaTime)
        {
            if (worldObject.IsDestroyed)
            {
                return false;
            }

            if (worldObject.ProtoStaticWorldObject is not IProtoObjectStructure protoObjectStructure)
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
                if (worldObject.ProtoStaticWorldObject is not IProtoObjectLandClaim)
                {
                    // only the land claim object can decay in the land claim area
                    return false;
                }
            }

            protoObjectStructure.ServerApplyDecay(worldObject, deltaTime);
            return true;
        }

        // refresh structures decay
        private static void ServerUpdate()
        {
            const double deltaTime = StructureConstants.DecaySystemUpdateIntervalSeconds;
            var serverTime = ServerGame.FrameTime;
            var frameNumber = ServerGame.FrameNumber;
            var frameRate = ServerGame.FrameRate;

            try
            {
                foreach (var protoStructure in serverListProtoStructuresToApplyDecay)
                {
                    protoStructure.EnumerateGameObjectsWithSpread(TempList, deltaTime, frameNumber, frameRate);
                }

                var objectsDecayedCount = 0;
                foreach (var worldObject in TempList)
                {
                    if (ServerProcessDecay(worldObject, serverTime, deltaTime))
                    {
                        objectsDecayedCount++;
                    }
                }

                //Logger.Important(
                //    string.Format(
                //        "{3}: finished update. Total static world objects count: {0}. Decaying objects count: {1}. Total time /spent /(including time-slicing): {2:F2}s",
                //        tempListCount,
                //        objectsDecayedCount,
                //        timeSpent,
                //        this.Name));
            }
            finally
            {
                TempList.Clear();
            }
        }
    }
}
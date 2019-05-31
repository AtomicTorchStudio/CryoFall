namespace AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Walls
{
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Helpers.Client.Walls;
    using AtomicTorch.CBND.CoreMod.Helpers.Primitives;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.Systems.Deconstruction;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.DataStructures;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Primitives;

    /// <summary>
    /// A special class for destroyed walls placeholder. Not a wall by itself.
    /// Should be automatically removed when there are no neighbor walls or anything is built in the occupied cell.
    /// </summary>
    public class ObjectWallDestroyed
        : ProtoObjectStructure
            <StructurePrivateState,
                WallDestroyedPublicState,
                ObjectWallClientState>
    {
        public override string InteractionTooltipText => InteractionTooltipTexts.Deconstruct;

        // we don't want the destroyed walls to disallow constructions so let's call this is a floor decal
        public override StaticObjectKind Kind => StaticObjectKind.FloorDecal;

        public override string Name => "Destroyed wall";

        // not used
        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Stone;

        public override double ObstacleBlockDamageCoef => 0;

        public override double ServerUpdateIntervalSeconds => double.MaxValue;

        // not used
        public override float StructurePointsMax => 9001;

        public static void ServerSpawnDestroyedWall(
            Vector2Ushort tilePosition,
            IProtoObjectWall originalProtoObjectWall)
        {
            if (Server.World
                      .GetTile(tilePosition)
                      .StaticObjects
                      .Any(so => so.ProtoStaticWorldObject is ObjectWallDestroyed))
            {
                //Logger.Error("Already spawned a destroyed wall here: " + tilePosition);
                return;
            }

            var worldObject = Server.World.CreateStaticWorldObject<ObjectWallDestroyed>(tilePosition);
            GetPublicState(worldObject).OriginalProtoObjectWall = originalProtoObjectWall;
            Logger.Important($"Spawning a destroyed wall at: {tilePosition} ({originalProtoObjectWall})");
        }

        public override void ClientSetupBlueprint(
            Tile tile,
            IClientBlueprint blueprint)
        {
            blueprint.SpriteRenderer.TextureResource = this.Icon;
        }

        public override void ServerApplyDecay(IStaticWorldObject worldObject, double deltaTime)
        {
            // no decay
        }

        public override void ServerOnDestroy(IStaticWorldObject gameObject)
        {
            base.ServerOnDestroy(gameObject);
            SharedWallConstructionRefreshHelper.SharedRefreshNeighborObjects(gameObject.OccupiedTile,
                                                                             isDestroy: true);
        }

        public override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            double damagePreMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            obstacleBlockDamageCoef = 0;
            damageApplied = 0; // no damage
            return false; // no hit
        }

        protected override ITextureResource ClientCreateIcon()
        {
            var chunk = WallPatterns
                        .PatternsPrimary.First(p => p.IsPass(NeighborsPattern.Left | NeighborsPattern.Right))
                        .AtlasChunkPosition;
            var protoWallWood = GetProtoEntity<ObjectWallWood>();
            return protoWallWood.TextureAtlasDestroyed
                                .Chunk((byte)chunk.X,
                                       (byte)chunk.Y);
        }

        protected override void ClientDeinitializeStructure(IStaticWorldObject gameObject)
        {
            base.ClientDeinitializeStructure(gameObject);
            SharedWallConstructionRefreshHelper.SharedRefreshNeighborObjects(gameObject.OccupiedTile, isDestroy: true);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            // don't use base implementation
            //base.ClientInitialize(data);

            this.ClientAddAutoStructurePointsBar(data);

            var worldObject = data.GameObject;
            var clientState = GetClientState(worldObject);

            // create renderers
            clientState.Renderer = Client.Rendering.CreateSpriteRenderer(worldObject);
            clientState.Renderer.DrawOrderOffsetY = 0.4;

            ProtoObjectWallHelper.ClientRefreshRenderer(data.GameObject);

            SharedWallConstructionRefreshHelper.SharedRefreshNeighborObjects(
                data.GameObject.OccupiedTile,
                isDestroy: false);
        }

        protected override void ClientInteractFinish(ClientObjectData data)
        {
            DeconstructionSystem.ClientTryAbortAction();
        }

        protected override void ClientInteractStart(ClientObjectData data)
        {
            DeconstructionSystem.ClientTryStartAction();
        }

        protected sealed override void PrepareConstructionConfig(
            ConstructionTileRequirements tileRequirements,
            ConstructionStageConfig build,
            ConstructionStageConfig repair,
            ConstructionUpgradeConfig upgrade,
            out ProtoStructureCategory category)
        {
            build.IsAllowed = false;
            build.StagesCount = 1;
            repair.IsAllowed = false;
            repair.StagesCount = 1;
            category = GetCategory<StructureCategoryBuildings>();
        }

        protected sealed override void PrepareProtoStaticWorldObject()
        {
            base.PrepareProtoStaticWorldObject();

            if (IsServer)
            {
                ConstructionPlacementSystem.ServerStructureBuilt += this.ServerStructureBuiltHandler;
            }
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            var worldObject = data.GameObject;
            var tile = worldObject.OccupiedTile;
            var protoWall = data.PublicState.OriginalProtoObjectWall;

            if (!ProtoObjectWallHelper.SharedIsDestroyedWallRequired(tile, protoWall))
            {
                // schedule destruction of this wall object
                ServerTimersSystem.AddAction(delaySeconds: 0.1,
                                             () =>
                                             {
                                                 if (!worldObject.IsDestroyed
                                                     && !ProtoObjectWallHelper.SharedIsDestroyedWallRequired(
                                                         tile,
                                                         protoWall))
                                                 {
                                                     Server.World.DestroyObject(worldObject);
                                                 }
                                             });
                return;
            }

            SharedWallConstructionRefreshHelper.SharedRefreshNeighborObjects(
                worldObject.OccupiedTile,
                isDestroy: false);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody.AddShapeRectangle((1, 1),
                                               group: CollisionGroups.ClickArea);

            ProtoObjectWallHelper.SharedCalculateNeighborsPattern(
                data.GameObject.OccupiedTile,
                protoWall: data.SyncPublicState.OriginalProtoObjectWall,
                out var sameTypeNeighbors,
                out _,
                isConsiderDestroyed: false,
                isConsiderConstructionSites: false);

            foreach (var pattern in WallPatterns.PatternsPrimary)
            {
                if (pattern.IsPass(sameTypeNeighbors))
                {
                    pattern.SetupPhysicsDestroyed?.Invoke(data.PhysicsBody);
                    return;
                }
            }
        }

        protected override bool SharedIsAllowedObjectToInteractThrough(IWorldObject worldObject)
        {
            return true;
        }

        private static void SharedGatherOccupiedAndNeighborTiles(IStaticWorldObject structure, ITempList<Tile> tempList)
        {
            // gather the occupied tiles and theirs direct neighbors
            foreach (var tile in structure.OccupiedTiles)
            {
                if (!tile.IsValidTile)
                {
                    continue;
                }

                tempList.AddIfNotContains(tile);

                foreach (var neighborTile in tile.EightNeighborTiles)
                {
                    if (neighborTile.IsValidTile)
                    {
                        tempList.AddIfNotContains(neighborTile);
                    }
                }
            }
        }

        private void ServerStructureBuiltHandler(ICharacter character, IStaticWorldObject structure)
        {
            // cleanup area around the built structure from the destroyed walls
            var serverWorld = Server.World;
            using (var tempList = Api.Shared.GetTempList<Tile>())
            {
                SharedGatherOccupiedAndNeighborTiles(structure, tempList);

                // destroy all the static objects in the gathered tiles
                foreach (var tile in tempList)
                {
                    var tileStaticObjects = tile.StaticObjects;
                    for (var index = 0; index < tileStaticObjects.Count; index++)
                    {
                        var staticObject = tileStaticObjects[index];
                        if (staticObject.ProtoStaticWorldObject == this)
                        {
                            serverWorld.DestroyObject(staticObject);
                            index--;
                        }
                    }
                }
            }
        }
    }
}
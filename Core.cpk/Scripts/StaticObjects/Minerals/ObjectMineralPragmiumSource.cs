namespace AtomicTorch.CBND.CoreMod.StaticObjects.Minerals
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Characters;
    using AtomicTorch.CBND.CoreMod.Characters.Mobs;
    using AtomicTorch.CBND.CoreMod.ClientComponents.FX;
    using AtomicTorch.CBND.CoreMod.Helpers.Server;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Objects;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.Systems.Creative;
    using AtomicTorch.CBND.CoreMod.Systems.LandClaim;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.PvE;
    using AtomicTorch.CBND.CoreMod.Systems.ServerTimers;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Extensions;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectMineralPragmiumSource
        : ProtoObjectMineral
          <ObjectMineralPragmiumSource.PrivateState,
              StaticObjectPublicState,
              DefaultMineralClientState>,
          IProtoObjectPsiSource
    {
        // How many nodes the game server should spawn when pragmium Source is destroyed.
        private const int DestroySpawnNodeCount = 9;

        private const string ErrorCannotBuild_PragmiumSourceTooCloseOnPvE =
            "Too close to a pragmium source[br](PvE-only restriction).";

        private const int MobDespawnDistance = 10;

        // How many guardian mobs pragmium source can have simultaneously.
        private const int MobsCountLimit = 3;

        private const int MobSpawnDistance = 2;

        // How many nodes a pragmium source can have simultaneously.
        private const int NodesCountLimit = 3;

        // How often a pragmium source will attempt to spawn nodes and decay.
        private const int ServerSpawnAndDecayIntervalSeconds = 10 * 60; // 10 minutes

        // How many guardian mobs a pragmium source will respawn at every spawn interval (ServerSpawnAndDecayIntervalSeconds).
        private const int ServerSpawnMobsMaxCountPerIteration = 2; // spawn at max 2 mobs per iteration

        // How many nodes a pragmium source will respawn at every spawn interval (ServerSpawnAndDecayIntervalSeconds).
        private const int ServerSpawnNodesMaxCountPerIteration = 1; // spawn at max 1 nodes per iteration

        public static readonly ConstructionTileRequirements.Validator ValidatorCheckNoPragmiumSourceNearbyOnPvE
            = new ConstructionTileRequirements.Validator(
                ErrorCannotBuild_PragmiumSourceTooCloseOnPvE,
                context =>
                {
                    var forCharacter = context.CharacterBuilder;
                    if (forCharacter == null)
                    {
                        return true;
                    }

                    if (context.TileOffset != Vector2Int.Zero)
                    {
                        return true;
                    }

                    if (!PveSystem.SharedIsPve(clientLogErrorIfDataIsNotYetAvailable: false))
                    {
                        // this limitation doesn't apply to PvP mode
                        return true;
                    }

                    if (CreativeModeSystem.SharedIsInCreativeMode(forCharacter))
                    {
                        return true;
                    }

                    var position = context.Tile.Position;
                    var world = IsServer
                                    ? (IWorldService)Server.World
                                    : (IWorldService)Client.World;

                    var pragmiumSources = world.GetStaticWorldObjectsOfProto<ObjectMineralPragmiumSource>();
                    foreach (var objectPragmiumSource in pragmiumSources)
                    {
                        if (position.TileSqrDistanceTo(objectPragmiumSource.TilePosition)
                            <= 16 * 16) // this value is derived from half distance of the max land claim size
                        {
                            // too close to a pragmium source
                            return false;
                        }
                    }

                    return true;
                });

        private static readonly Lazy<IProtoCharacter> LazyProtoMob
            = new Lazy<IProtoCharacter>(
                GetProtoEntity<MobPragmiumBeetle>);

        private static readonly double LifetimeTotalDurationSeconds
            = TimeSpan.FromHours(12).TotalSeconds;

        private static readonly Lazy<ObjectMineralPragmiumNode> ProtoNodeLazy
            = new Lazy<ObjectMineralPragmiumNode>(
                GetProtoEntity<ObjectMineralPragmiumNode>);

        // we don't want to see any decals under it
        public override StaticObjectKind Kind => StaticObjectKind.Structure;

        public override string Name => "Pragmium source";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

        public double PsiIntensity => 0.1;

        public double PsiRadiusMax => 5;

        public double PsiRadiusMin => 3;

        public override double ServerUpdateIntervalSeconds => ServerSpawnAndDecayIntervalSeconds;

        public override float StructurePointsMax => 5000;

        // has light source
        public override BoundsInt ViewBoundsExpansion => new BoundsInt(minX: -6,
                                                                       minY: -2,
                                                                       maxX: 6,
                                                                       maxY: 4);

        public void ServerForceUpdate(IStaticWorldObject worldObject, double deltaTime)
        {
            var publicState = GetPublicState(worldObject);

            // process decay
            var damage = this.StructurePointsMax * deltaTime / LifetimeTotalDurationSeconds;
            var newStructurePoints = (float)(publicState.StructurePointsCurrent - damage);
            if (newStructurePoints <= 0)
            {
                // decayed completely - destroy
                publicState.StructurePointsCurrent = newStructurePoints = 0;
                this.ServerOnStaticObjectZeroStructurePoints(null, null, worldObject);
                return;
            }

            publicState.StructurePointsCurrent = newStructurePoints;

            if (!Server.World.IsObservedByAnyPlayer(worldObject))
            {
                ServerTrySpawnNodes(worldObject);
                ServerTrySpawnMobs(worldObject);
            }
        }

        public bool ServerIsPsiSourceActive(IWorldObject worldObject) => true;

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return new Vector2D(1, 2.2);
        }

        public override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            double damagePreMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            if (weaponCache.Character != null)
            {
                // damaged by character
                if (IsServer)
                {
                    // notify other characters
                    using var tempList = Api.Shared.GetTempList<ICharacter>();
                    Server.World.GetScopedByPlayers(targetObject, tempList);
                    tempList.Remove(weaponCache.Character);
                    this.CallClient(tempList.AsList(), _ => _.ClientRemote_OnHit());
                }
                else
                {
                    ClientOnHit();
                }
            }

            if (weaponCache.ProtoWeapon != null
                && !(weaponCache.ProtoWeapon is IProtoItemWeaponMelee))
            {
                // hit but not damaged - only melee weapons (including pickaxes can damage this)
                obstacleBlockDamageCoef = this.ObstacleBlockDamageCoef;
                damageApplied = 0;
                return true;
            }

            return base.SharedOnDamage(weaponCache,
                                       targetObject,
                                       damagePreMultiplier,
                                       out obstacleBlockDamageCoef,
                                       out damageApplied);
        }

        /// <summary>
        /// When the explosion is processed this method will be called (see ObjectMineralPragmiumSourceExplosion).
        /// </summary>
        internal static void ServerOnExplode(
            Vector2Ushort epicenterPosition,
            double explosionRadius)
        {
            // let's spawn scattered pragmium nodes around the explosion site 
            var countToSpawnRemains = DestroySpawnNodeCount;
            var attemptsRemains = 2000;

            while (countToSpawnRemains > 0)
            {
                attemptsRemains--;
                if (attemptsRemains <= 0)
                {
                    // attempts exceeded
                    return;
                }

                // calculate random distance from the explosion epicenter
                var distance = RandomHelper.Range(2, explosionRadius);

                // ensure we spawn more objects closer to the epicenter
                var spawnProbability = 1 - (distance / explosionRadius);
                spawnProbability = Math.Pow(spawnProbability, 1.5);
                if (!RandomHelper.RollWithProbability(spawnProbability))
                {
                    // random skip
                    continue;
                }

                var angle = RandomHelper.NextDouble() * MathConstants.DoublePI;
                var spawnPosition = new Vector2Ushort(
                    (ushort)(epicenterPosition.X + distance * Math.Cos(angle)),
                    (ushort)(epicenterPosition.Y + distance * Math.Sin(angle)));

                // try spawn a pragmium node
                if (ServerTrySpawnNode(spawnPosition))
                {
                    // spawned successfully!
                    countToSpawnRemains--;
                }
            }
        }

        protected override ITextureResource ClientGetTextureResource(
            IStaticWorldObject gameObject,
            StaticObjectPublicState publicState)
        {
            return this.DefaultTexture;
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            ObjectMineralPragmiumHelper.ClientInitializeLightForSource(data.GameObject);
        }

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            renderer.SpritePivotPoint = (0, 0);
            renderer.PositionOffset = (0, 0);
            renderer.DrawOrderOffsetY = 1.0;
        }

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("##");
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureResource(GenerateTexturePath(thisType));
        }

        protected override void PrepareProtoMineral(MineralDropItemsConfig config)
        {
            // drops nothing as there is a separate method for explosion
        }

        protected override void PrepareTileRequirements(ConstructionTileRequirements tileRequirements)
        {
            base.PrepareTileRequirements(tileRequirements);
            tileRequirements.Add(ConstructionTileRequirements.ValidatorNotRestrictedAreaEvenForServer);
        }

        protected override void ServerOnStaticObjectZeroStructurePoints(
            WeaponFinalCache weaponCache,
            ICharacter byCharacter,
            IWorldObject targetObject)
        {
            var tilePosition = targetObject.TilePosition;
            var privateState = GetPrivateState((IStaticWorldObject)targetObject);

            base.ServerOnStaticObjectZeroStructurePoints(weaponCache, byCharacter, targetObject);

            Server.World.CreateStaticWorldObject<ObjectMineralPragmiumSourceExplosion>(tilePosition);

            ServerTimersSystem.AddAction(
                Api.GetProtoEntity<ObjectMineralPragmiumSourceExplosion>().ExplosionDelay.TotalSeconds,
                () =>
                {
                    // kill all spawned mobs
                    foreach (var character in Api.Shared.WrapInTempList(privateState.MobsList).EnumerateAndDispose())
                    {
                        if (!character.IsDestroyed)
                        {
                            character.GetPublicState<ICharacterPublicState>()
                                     .CurrentStats
                                     .ServerSetHealthCurrent(0);
                        }
                    }
                });
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            // please note this update is called rarely (defined by ServerUpdateIntervalSeconds)
            this.ServerForceUpdate(data.GameObject, data.DeltaTime);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(
                    radius: 0.45,
                    center: (1, 0.7))
                .AddShapeRectangle(
                    size: (0.8, 0.6),
                    offset: (0.6, 0.5),
                    group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(
                    size: (0.8, 0.7),
                    offset: (0.6, 0.5),
                    group: CollisionGroups.HitboxRanged);
        }

        private static void ClientOnHit()
        {
            const float shakesDuration = 0.1f,
                        shakesDistanceMin = 0.1f,
                        shakesDistanceMax = 0.125f;
            ClientComponentCameraScreenShakes.AddRandomShakes(duration: shakesDuration,
                                                              worldDistanceMin: -shakesDistanceMin,
                                                              worldDistanceMax: shakesDistanceMax);
        }

        private static void ServerTrySpawnMobs(IStaticWorldObject worldObject)
        {
            if (LandClaimSystem.SharedIsLandClaimedByAnyone(worldObject.Bounds))
            {
                // don't spawn mobs as the land is claimed
                return;
            }

            // calculate how many creatures are still alive
            var mobsList = GetPrivateState(worldObject).MobsList;

            var mobsAlive = 0;
            for (var index = 0; index < mobsList.Count; index++)
            {
                var character = mobsList[index];
                if (character.IsDestroyed)
                {
                    mobsList.RemoveAt(index--);
                    continue;
                }

                if (character.TilePosition.TileSqrDistanceTo(worldObject.TilePosition)
                    > MobDespawnDistance * MobDespawnDistance)
                {
                    // the guardian mob is too far - probably lured away by a player
                    using var tempListObservers = Api.Shared.GetTempList<ICharacter>();
                    Server.World.GetScopedByPlayers(character, tempListObservers);
                    if (tempListObservers.Count == 0)
                    {
                        // despawn this mob as it's not observed by any player
                        Server.World.DestroyObject(character);
                        mobsList.RemoveAt(index--);
                    }

                    continue;
                }

                mobsAlive++;
            }

            var countToSpawn = MobsCountLimit - mobsAlive;
            if (countToSpawn <= 0)
            {
                return;
            }

            // spawn mobs(s) nearby
            countToSpawn = Math.Min(countToSpawn, ServerSpawnMobsMaxCountPerIteration);
            ServerMobSpawnHelper.ServerTrySpawnMobsCustom(protoMob: LazyProtoMob.Value,
                                                          spawnedCollection: mobsList,
                                                          countToSpawn,
                                                          excludeBounds: worldObject.Bounds.Inflate(1),
                                                          maxSpawnDistanceFromExcludeBounds: MobSpawnDistance,
                                                          noObstaclesCheckRadius: 0.5,
                                                          maxAttempts: 200);
        }

        private static bool ServerTrySpawnNode(Vector2Ushort spawnPosition)
        {
            var protoNode = ProtoNodeLazy.Value;
            if (!protoNode.CheckTileRequirements(spawnPosition,
                                                 character: null,
                                                 logErrors: false))
            {
                return false;
            }

            var node = Server.World.CreateStaticWorldObject(protoNode, spawnPosition);
            if (node is null)
            {
                return false;
            }

            // make this node to despawn automatically
            ObjectMineralPragmiumNode.ServerRestartDestroyTimer(
                ObjectMineralPragmiumNode.GetPrivateState(node));
            return true;
        }

        private static void ServerTrySpawnNodes(IStaticWorldObject worldObject)
        {
            // calculate how many nodes are nearby
            var nodesAroundCount = 0;
            var neighborTiles = worldObject.OccupiedTiles
                                           .SelectMany(t => t.EightNeighborTiles)
                                           .Distinct()
                                           .ToList();

            foreach (var neighborTile in neighborTiles)
            {
                foreach (var otherObject in neighborTile.StaticObjects)
                {
                    if (!(otherObject.ProtoStaticWorldObject is ObjectMineralPragmiumNode))
                    {
                        continue;
                    }

                    nodesAroundCount++;
                    if (nodesAroundCount >= NodesCountLimit)
                    {
                        // there are already enough nodes around
                        return;
                    }
                }
            }

            // spawn node(s) nearby
            var countToSpawn = NodesCountLimit - nodesAroundCount;
            var attempts = neighborTiles.Count * 4;
            countToSpawn = Math.Min(countToSpawn, ServerSpawnNodesMaxCountPerIteration);

            while (attempts-- > 0)
            {
                var neighborTile = neighborTiles.TakeByRandom();
                if (neighborTile.StaticObjects.Count > 0)
                {
                    // cannot spawn there
                    continue;
                }

                if (!ServerTrySpawnNode(neighborTile.Position))
                {
                    // cannot spawn there
                    continue;
                }

                countToSpawn--;
                if (countToSpawn == 0)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// When the pragmium source is hit by a player it should dangerously shake! :-)
        /// </summary>
        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_OnHit()
        {
            ClientOnHit();
        }

        public class PrivateState : BasePrivateState
        {
            [TempOnly]
            public List<ICharacter> MobsList { get; set; } = new List<ICharacter>();
        }
    }
}
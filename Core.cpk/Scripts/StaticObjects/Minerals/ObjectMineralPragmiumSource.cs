namespace AtomicTorch.CBND.CoreMod.StaticObjects.Minerals
{
    using System;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.ClientComponents.FX;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.Objects;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Explosives;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.CBND.GameApi.ServicesServer;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectMineralPragmiumSource : ProtoObjectMineral, IProtoObjectPsiSource
    {
        // How many nodes the game server should spawn when Pragmium Source is destroyed.
        private const int DestroySpawnNodeCount = 12;

        // How many nodes Pragmium Source can have simultaneously.
        private const int NodesCountLimit = 5;

        // How often Pragmium Source will attempt to spawn nodes and decay.
        private const int ServerSpawnAndDecayIntervalSeconds = 10 * 60; // 10 minutes

        // How many nodes Pragmium Source will respawn at every spawn interval (ServerSpawnAndDecayIntervalSeconds).
        private const int ServerSpawnNodesMaxCountPerIteration = 2; // spawn at max 2 nodes

        // Total lifetime of the Pragmium Source.
        private static readonly double LifetimeTotalDurationSeconds = TimeSpan.FromDays(6).TotalSeconds;

        private static readonly Lazy<ObjectMineralPragmiumNode> ProtoNodeLazy
            = new Lazy<ObjectMineralPragmiumNode>(GetProtoEntity<ObjectMineralPragmiumNode>);

        private static readonly IWorldServerService ServerWorld = IsServer ? Server.World : null;

        // we don't want to see any decals under it
        public override StaticObjectKind Kind => StaticObjectKind.Structure;

        public override string Name => "Pragmium source";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Stone;

        public double PsiIntensity => 0.1;

        public double PsiRadiusMax => 5;

        public double PsiRadiusMin => 3;

        public override double ServerUpdateIntervalSeconds => ServerSpawnAndDecayIntervalSeconds;

        public override float StructurePointsMax => 5000;

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
                    using (var tempList = Api.Shared.GetTempList<ICharacter>())
                    {
                        ServerWorld.GetScopedByPlayers(targetObject, tempList);
                        tempList.Remove(weaponCache.Character);
                        this.CallClient(tempList, _ => _.ClientRemote_OnHit());
                    }
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
            // let's spawn scatter Pragmium nodes around the explosion site 
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
                var distance = RandomHelper.Range(0, explosionRadius);

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

                // try spawn a Pragmium node
                if (ServerTrySpawnPragmiumNode(spawnPosition))
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
            base.ServerOnStaticObjectZeroStructurePoints(weaponCache, byCharacter, targetObject);
            ServerWorld.CreateStaticWorldObject<ObjectMineralPragmiumSourceExplosion>(tilePosition);
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            // please note this update is called rarely (defined by ServerUpdateIntervalSeconds)
            var worldObject = data.GameObject;
            var publicState = data.PublicState;

            // process decay
            var damage = this.StructurePointsMax * data.DeltaTime / LifetimeTotalDurationSeconds;
            var newStructurePoints = (float)(publicState.StructurePointsCurrent - damage);
            if (newStructurePoints > 0)
            {
                publicState.StructurePointsCurrent = newStructurePoints;
            }
            else
            {
                // decayed completely - destroy
                publicState.StructurePointsCurrent = newStructurePoints = 0;
                this.ServerOnStaticObjectZeroStructurePoints(null, null, worldObject);
                return;
            }

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

                if (!ServerTrySpawnPragmiumNode(neighborTile.Position))
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

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(
                    size: (1.1, 0.5),
                    offset: (0.45, 0.45))
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

        private static bool ServerTrySpawnPragmiumNode(Vector2Ushort spawnPosition)
        {
            var protoNode = ProtoNodeLazy.Value;
            return protoNode.CheckTileRequirements(spawnPosition,
                                                   character: null,
                                                   logErrors: false)
                   && ServerWorld.CreateStaticWorldObject(protoNode, spawnPosition) != null;
        }

        /// <summary>
        /// When the pragmium source is hit by a player it should dangerously shake! :-)
        /// </summary>
        [RemoteCallSettings(DeliveryMode.ReliableSequenced)]
        private void ClientRemote_OnHit()
        {
            ClientOnHit();
        }
    }
}
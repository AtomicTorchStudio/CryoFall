namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.Systems.NewbieProtection;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectPragmiumQueenRemains
        : ProtoObjectMineral
            <ObjectPragmiumQueenRemains.PrivateState, StaticObjectPublicState, DefaultMineralClientState>
    {
        // The remains destruction will be postponed on this duration
        // if it cannot be destroy because there are characters observing it.
        private static readonly double DestructionTimeoutPostponeSeconds
            = TimeSpan.FromMinutes(2).TotalSeconds;

        // The remains will destroy after this duration if there are no characters observing it.
        private static readonly double DestructionTimeoutSeconds
            = TimeSpan.FromMinutes(30).TotalSeconds;

        private TextureResource[] textures;

        public override string Name => "Pragmium Queen remains";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

        public override float StructurePointsMax => 2000;

        public override Vector2D SharedGetObjectCenterWorldOffset(IWorldObject worldObject)
        {
            return base.SharedGetObjectCenterWorldOffset(worldObject) + (0, 0.25);
        }

        protected override ITextureResource ClientGetTextureResource(
            IStaticWorldObject gameObject,
            StaticObjectPublicState publicState)
        {
            return this.textures[PositionalRandom.Get(gameObject.TilePosition,
                                                      minInclusive: 0,
                                                      maxExclusive: this.textures.Length,
                                                      seed: 791838756)];
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            using var tempFiles = Api.Shared.FindFiles(ContentPaths.Textures + GenerateTexturePath(thisType));
            this.textures = new TextureResource[tempFiles.Count];

            var list = tempFiles.AsList();
            for (var index = 0; index < list.Count; index++)
            {
                var tempFile = list[index];
                this.textures[index] = new TextureResource(tempFile);
            }

            return this.textures[0];
        }

        protected override void PrepareProtoMineral(MineralDropItemsConfig config)
        {
            config.Stage1
                  .Add<ItemOrePragmium>(count: 3, countRandom: 1)
                  .Add<ItemGoldNugget>(count: 3,  countRandom: 1)
                  .Add<ItemOreLithium>(count: 10, countRandom: 2);

            config.Stage2
                  .Add<ItemOrePragmium>(count: 3, countRandom: 1)
                  .Add<ItemGoldNugget>(count: 3,  countRandom: 1)
                  .Add<ItemOreLithium>(count: 10, countRandom: 2);

            config.Stage3
                  .Add<ItemOrePragmium>(count: 3, countRandom: 1)
                  .Add<ItemGoldNugget>(count: 3,  countRandom: 1)
                  .Add<ItemOreLithium>(count: 10, countRandom: 2);

            config.Stage4
                  .Add<ItemPragmiumHeart>(count: 1)
                  .Add<ItemOrePragmium>(count: 5, countRandom: 2)
                  .Add<ItemGoldNugget>(count: 5,  countRandom: 2)
                  .Add<ItemOreLithium>(count: 10, countRandom: 3);
        }

        protected override void ServerInitialize(ServerInitializeData data)
        {
            base.ServerInitialize(data);

            // reset the destroy timer (even if object is already initialized (e.g. loading a savegame))
            data.PrivateState.DestroyAtTime = Server.Game.FrameTime + DestructionTimeoutSeconds;
        }

        protected override void ServerUpdate(ServerUpdateData data)
        {
            base.ServerUpdate(data);

            if (Api.IsEditor)
            {
                // do not destroy by timeout in Editor
                return;
            }

            var privateState = data.PrivateState;
            var timeNow = Server.Game.FrameTime;

            // Destroy Pragmium node if the timeout is exceeded
            // and there is no Pragmium Source node nearby
            // and there are no player characters observing it.
            if (timeNow < privateState.DestroyAtTime)
            {
                return;
            }

            // should destroy because timed out
            var worldObject = data.GameObject;
            if (Server.World.IsObservedByAnyPlayer(worldObject))
            {
                // cannot destroy - there are players observing it
                privateState.DestroyAtTime = timeNow + DestructionTimeoutPostponeSeconds;
                return;
            }

            Server.World.DestroyObject(worldObject);
        }

        protected override double SharedCalculateDamageByWeapon(
            WeaponFinalCache weaponCache,
            double damagePreMultiplier,
            IStaticWorldObject targetObject,
            out double obstacleBlockDamageCoef)
        {
            if (NewbieProtectionSystem.SharedIsNewbie(weaponCache.Character))
            {
                // don't allow mining a boss loot while under newbie protection
                if (IsClient)
                {
                    NewbieProtectionSystem.ClientNotifyNewbieCannotPerformAction(this);
                }

                obstacleBlockDamageCoef = 0;
                return 0;
            }

            return base.SharedCalculateDamageByWeapon(weaponCache,
                                                      damagePreMultiplier,
                                                      targetObject,
                                                      out obstacleBlockDamageCoef);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.3,  center: (0.5, 0.4))
                .AddShapeCircle(radius: 0.45, center: (0.5, 0.5), group: CollisionGroups.HitboxMelee);
            // no ranged hitbox
        }

        public class PrivateState : BasePrivateState
        {
            public double DestroyAtTime { get; set; }
        }
    }
}
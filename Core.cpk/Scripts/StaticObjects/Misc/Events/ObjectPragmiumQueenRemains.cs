namespace AtomicTorch.CBND.CoreMod.StaticObjects.Misc.Events
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Minerals;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class ObjectPragmiumQueenRemains : ProtoObjectBossLoot
    {
        private TextureResource[] textures;

        public override string Name => "Pragmium Queen remains";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

        public override float StructurePointsMax => 1500;

        // has light source
        public override BoundsInt ViewBoundsExpansion => new(minX: -1,
                                                             minY: -1,
                                                             maxX: 1,
                                                             maxY: 2);

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

        protected override void ClientInitialize(ClientInitializeData data)
        {
            base.ClientInitialize(data);
            ObjectMineralPragmiumHelper.ClientInitializeLightForNode(data.GameObject);
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
                  .Add<ItemOrePragmium>(count: 4, countRandom: 2)
                  .Add<ItemGoldNugget>(count: 3,  countRandom: 1)
                  .Add<ItemOreLithium>(count: 5,  countRandom: 1);

            config.Stage2
                  .Add<ItemOrePragmium>(count: 4, countRandom: 2)
                  .Add<ItemGoldNugget>(count: 3,  countRandom: 1)
                  .Add<ItemOreLithium>(count: 5,  countRandom: 1);

            config.Stage3
                  .Add<ItemOrePragmium>(count: 4, countRandom: 2)
                  .Add<ItemGoldNugget>(count: 3,  countRandom: 1)
                  .Add<ItemOreLithium>(count: 5,  countRandom: 1);

            config.Stage4
                  .Add<ItemPragmiumHeart>(count: 1)
                  .Add<ItemOrePragmium>(count: 6, countRandom: 2)
                  .Add<ItemGoldNugget>(count: 4,  countRandom: 1)
                  .Add<ItemOreLithium>(count: 5,  countRandom: 2);
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeCircle(radius: 0.3,  center: (0.5, 0.4))
                .AddShapeCircle(radius: 0.45, center: (0.5, 0.5), group: CollisionGroups.HitboxMelee);
            // no ranged hitbox
        }
    }
}
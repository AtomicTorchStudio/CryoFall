namespace AtomicTorch.CBND.CoreMod.StaticObjects.Props
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public abstract class ProtoObjectProp : ProtoStaticWorldObject
    {
        [SuppressMessage("ReSharper", "CanExtractXamlLocalizableStringCSharp")]
        protected ProtoObjectProp()
        {
            var name = this.GetType().Name;
            if (!name.StartsWith("ObjectProp", StringComparison.Ordinal))
            {
                throw new Exception("Prop class name must start with \"ObjectProp\": " + this.GetType().Name);
            }

            this.ShortId = name.Substring("ObjectProp".Length);
        }

        public virtual bool CanFlipSprite => false;

        public override double ClientUpdateIntervalSeconds => double.MaxValue;

        public override StaticObjectKind Kind => StaticObjectKind.Structure;

        public sealed override string Name => this.ShortId;

        public override ObjectSoundMaterial ObjectSoundMaterial
            => ObjectSoundMaterial.Stone;

        public override double ObstacleBlockDamageCoef => 1;

        public sealed override double ServerUpdateIntervalSeconds => double.MaxValue;

        public override string ShortId { get; }

        public override float StructurePointsMax => 9001; // it's non-damageable anyway

        public sealed override bool SharedOnDamage(
            WeaponFinalCache weaponCache,
            IStaticWorldObject targetObject,
            double damagePreMultiplier,
            out double obstacleBlockDamageCoef,
            out double damageApplied)
        {
            obstacleBlockDamageCoef = this.ObstacleBlockDamageCoef;
            damageApplied = 0; // no damage
            return true; // hit
        }

        protected static void AddFullHeightWallHitboxes(
            CreatePhysicsData data,
            double width = 1.0,
            double offsetX = 0,
            double offsetY = 0)
        {
            data.PhysicsBody
                .AddShapeRectangle((width, 1),
                                   offset: (offsetX, offsetY + 0),
                                   group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((width, 1.27),
                                   offset: (offsetX, offsetY + 0.15),
                                   group: CollisionGroups.HitboxRanged);
        }

        protected static void AddHalfHeightWallHitboxes(
            CreatePhysicsData data,
            double width = 1.0,
            double offsetX = 0,
            double offsetY = 0)
        {
            data.PhysicsBody
                .AddShapeRectangle((width, 0.25),
                                   offset: (offsetX, offsetY + 0.75),
                                   group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle((width, 0.57),
                                   offset: (offsetX, offsetY + 0.85),
                                   group: CollisionGroups.HitboxRanged);
        }

        protected static void AddRectangleWithHitboxes(CreatePhysicsData data, Vector2D size, Vector2D? offset = null)
        {
            data.PhysicsBody
                .AddShapeRectangle(size, offset)
                .AddShapeRectangle(size, offset, group: CollisionGroups.HitboxMelee)
                .AddShapeRectangle(size, offset, group: CollisionGroups.HitboxRanged);
        }

        protected override void ClientInitialize(ClientInitializeData data)
        {
            // don't use base implementation
            //base.ClientInitialize(data);

            var renderer = Client.Rendering.CreateSpriteRenderer(
                data.GameObject,
                textureResource: this.DefaultTexture);
            data.ClientState.Renderer = renderer;

            // flip renderer with some deterministic randomization
            if (this.CanFlipSprite
                && PositionalRandom.Get(data.GameObject.TilePosition, 0, 3, seed: 721886451) == 0)
            {
                renderer.DrawMode = DrawMode.FlipHorizontally;
            }

            this.ClientSetupRenderer(renderer);
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            var folderPath = SharedGetRelativeFolderPath(thisType, typeof(ProtoObjectProp));
            return new TextureResource($"StaticObjects/Props/{folderPath}/{thisType.Name}.png");
        }
    }
}
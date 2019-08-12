namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using System;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Seeds;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectLootGrass : ProtoObjectLoot
    {
        private static TextureAtlasResource textureAtlas;

        public ObjectLootGrass()
        {
            textureAtlas = new TextureAtlasResource(this.GenerateTexturePath(), 4, 1, true);
        }

        public override string Name => "Grass";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Vegetation;

        protected override bool CanFlipSprite => false; // it's grass - flip is done by shader

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            renderer.PositionOffset = (0.5, 0.0);
            renderer.SpritePivotPoint = (0.5, 0);
            renderer.DrawOrderOffsetY = 0.38;
            renderer.Scale = 1.3;

            // making the grass choose random element of the atlas depending on world position
            var position = renderer.SceneObject.Position.ToVector2Ushort();
            var atlasChunk = (byte)PositionalRandom.Get(position, 0, 4, seed: 12345);
            renderer.TextureResource = textureAtlas.Chunk(atlasChunk, 0);

            ClientGrassRenderingHelper.Setup(renderer,
                                             power: 0.1f,
                                             pivotY: 0.2f);
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return textureAtlas;
        }

        protected override void PrepareLootDroplist(DropItemsList droplist)
        {
            // primary
            droplist.Add<ItemFibers>(count: 3, countRandom: 2);

            // chance to also drop seeds
            droplist.Add(probability: 1 / 50.0,
                         nestedList: new DropItemsList(outputs: 1)
                                     .Add<ItemSeedsCarrot>(count: 1)
                                     .Add<ItemSeedsCucumber>(count: 1)
                                     .Add<ItemSeedsTomato>(count: 1)
                                     .Add<ItemSeedsBellPepper>(count: 1));
        }

        protected override ReadOnlySoundPreset<ObjectSound> PrepareSoundPresetObject()
        {
            return base.PrepareSoundPresetObject()
                       .Clone()
                       // this is grass - play vegetation interact success sound
                       .Replace(ObjectSound.InteractSuccess, "Objects/Vegetation/InteractSuccess.ogg");
        }

        protected override void SharedCreatePhysics(CreatePhysicsData data)
        {
            data.PhysicsBody
                .AddShapeRectangle(offset: (0.35, 0.2), size: (0.3, 0.6), group: CollisionGroups.HitboxMelee)
                .AddShapeCircle(radius: 0.45, center: (0.5, 0.5), group: CollisionGroups.ClickArea);
            // no ranged hitbox
        }
    }
}
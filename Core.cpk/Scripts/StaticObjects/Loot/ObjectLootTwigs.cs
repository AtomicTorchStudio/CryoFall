namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectLootTwigs : ProtoObjectLoot
    {
        public override string Name => "Twigs";

        public override ObjectSoundMaterial ObjectSoundMaterial => ObjectSoundMaterial.Wood;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.SpritePivotPoint = (0.5, 0.5);
            renderer.PositionOffset = (0.5, 0.5);
            //renderer.DrawOrderOffsetY += 0.5;
        }

        protected override void PrepareLootDroplist(DropItemsList droplist)
        {
            droplist.Add<ItemTwigs>(count: 2, countRandom: 2);
        }
    }
}
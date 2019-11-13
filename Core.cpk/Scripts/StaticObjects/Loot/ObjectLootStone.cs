namespace AtomicTorch.CBND.CoreMod.StaticObjects.Loot
{
    using AtomicTorch.CBND.CoreMod.Items.Generic;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.CoreMod.Systems.Droplists;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class ObjectLootStone : ProtoObjectLoot
    {
        public override string Name => "Stone";

        public override ObjectMaterial ObjectMaterial => ObjectMaterial.Stone;

        protected override void ClientSetupRenderer(IComponentSpriteRenderer renderer)
        {
            base.ClientSetupRenderer(renderer);
            renderer.SpritePivotPoint = (0.5, 0.5);
            renderer.PositionOffset = (0.5, 0.5);
            //renderer.DrawOrderOffsetY += 0.5;
        }

        protected override void PrepareLootDroplist(DropItemsList droplist)
        {
            droplist.Add<ItemStone>(count: 1, countRandom: 2);
        }
    }
}
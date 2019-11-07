namespace AtomicTorch.CBND.CoreMod.StaticObjects.Special
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Special.Base;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectCharredGround3Deposit : ProtoObjectCharredGround
    {
        [NotLocalizable]
        public override string Name => "Charred ground (Deposit)";

        protected override void CreateLayout(StaticObjectLayout layout)
        {
            layout.Setup("###",
                         "###",
                         "###");
        }

        protected override ITextureResource PrepareDefaultTexture(Type thisType)
        {
            return new TextureResource("StaticObjects/Special/ObjectDepletedResource",
                                       isTransparent: true);
        }
    }
}
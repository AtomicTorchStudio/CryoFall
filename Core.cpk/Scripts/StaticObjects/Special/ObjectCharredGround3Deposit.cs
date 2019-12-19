namespace AtomicTorch.CBND.CoreMod.StaticObjects.Special
{
    using System;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Special.Base;
    using AtomicTorch.CBND.GameApi;
    using AtomicTorch.CBND.GameApi.Data.World;
    using AtomicTorch.CBND.GameApi.Resources;

    public class ObjectCharredGround3Deposit : ProtoObjectCharredGround
    {
        // we want to ensure the spawn conditions will use this as a restriction
        public override bool IsIgnoredBySpawnScripts => false;

        [NotLocalizable]
        public override string Name => "Charred ground (Deposit)";

        // 6 hours (setting it higher might lead to an issue where there is no space to spawn on a very populated server)
        public override double ObjectDespawnDurationSeconds => 6 * 60 * 60;

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
namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.CoreMod.Systems.Physics;
    using AtomicTorch.CBND.GameApi.Data.Physics;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Primitives;

    public class SkeletonMutantWolf : SkeletonWolf
    {
        public override SkeletonResource SkeletonResourceBack { get; }
            = new("MutantWolf/Back");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new("MutantWolf/Front");

    }
}
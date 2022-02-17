namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.GameApi.Resources;

    public class SkeletonWolfPolar : SkeletonWolf
    {
        public override SkeletonResource SkeletonResourceBack { get; }
            = new("WolfPolar/Back");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new("WolfPolar/Front");
    }
}
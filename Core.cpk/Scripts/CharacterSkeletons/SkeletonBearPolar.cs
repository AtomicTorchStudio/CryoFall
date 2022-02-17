namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.GameApi.Resources;

    public class SkeletonBearPolar : SkeletonBear
    {
        public override SkeletonResource SkeletonResourceBack { get; }
            = new("BearPolar/Back");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new("BearPolar/Front");
    }
}
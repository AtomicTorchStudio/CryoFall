namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.GameApi.Resources;

    public class SkeletonFireLizard : SkeletonCloakedLizard
    {
        public override SkeletonResource SkeletonResourceBack { get; }
            = new SkeletonResource("FireLizard/Back");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new SkeletonResource("FireLizard/Front");
    }
}
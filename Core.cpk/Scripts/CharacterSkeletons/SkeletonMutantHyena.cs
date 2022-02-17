namespace AtomicTorch.CBND.CoreMod.CharacterSkeletons
{
    using AtomicTorch.CBND.GameApi.Resources;

    public class SkeletonMutantHyena : SkeletonHyena
    {
        public override SkeletonResource SkeletonResourceBack { get; }
            = new("MutantHyena/Back");

        public override SkeletonResource SkeletonResourceFront { get; }
            = new("MutantHyena/Front");
    }
}
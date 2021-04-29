namespace AtomicTorch.CBND.CoreMod.Items.Reactor
{
    public class ItemReactorDeflector : ProtoItemReactorModule
    {
        public override string Description =>
            "Special multilayer shielding that partially blocks psionic radiation emitted outside of the reactor.";

        public override string Name => "Psionic deflector";

        public override double PsiEmissionModifierValue => -1.8;
    }
}
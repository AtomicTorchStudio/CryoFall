namespace AtomicTorch.CBND.CoreMod.Items.Reactor
{
    public class ItemReactorModerator : ProtoItemReactorModule
    {
        public override string Description =>
            "Alters configuration of the psionic fields produced inside the reactor core, increasing its output.";

        public override double EfficiencyModifierPercents => 16;

        public override double FuelLifetimeModifierPercents => -7;

        public override string Name => "Psionic moderator";

        public override double PsiEmissionModifierValue => 0.25;
    }
}
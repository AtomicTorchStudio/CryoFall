namespace AtomicTorch.CBND.CoreMod.Items.Reactor
{
    public class ItemReactorDiffuser : ProtoItemReactorModule
    {
        public override string Description =>
            "Special device that diffuses psionic radiation inside the reactor core, improving fuel lifetime at the cost of reduced power output.";

        public override double EfficiencyModifierPercents => -5;

        public override double FuelLifetimeModifierPercents => 20;

        public override string Name => "Psionic diffuser";

        public override double PsiEmissionModifierValue => 0.4;
    }
}
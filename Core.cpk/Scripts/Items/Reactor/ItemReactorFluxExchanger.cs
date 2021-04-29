namespace AtomicTorch.CBND.CoreMod.Items.Reactor
{
    public class ItemReactorFluxExchanger : ProtoItemReactorModule
    {
        public override string Description =>
            "Improves reactor response by introducing additional psionic flux controllers, greatly enhancing the reactor's startup and shutdown times.";

        public override string Name => "Psionic flux exchanger";

        public override double StartupShutdownTimeModifierPercents => -24;
    }
}
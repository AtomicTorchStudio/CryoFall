namespace AtomicTorch.CBND.CoreMod.StaticObjects.Explosives
{
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ObjectExplosivePrivateState : BasePrivateState
    {
        public ICharacter DeployedByCharacter { get; set; }

        [TempOnly]
        public double ExplosionDelaySecondsRemains { get; set; }
    }
}
// ReSharper disable once CheckNamespace
namespace AtomicTorch.CBND.GameApi.Data.Weapons
{
    public readonly struct DamageProportion
    {
        public readonly DamageType DamageType;

        public readonly double Proportion;

        public DamageProportion(DamageType damageType, double proportion)
        {
            this.DamageType = damageType;
            this.Proportion = proportion;
        }
    }
}
// ReSharper disable once CheckNamespace
namespace AtomicTorch.CBND.GameApi.Data.Weapons
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class DamageDistribution : IEnumerable<DamageProportion>
    {
        private readonly IDictionary<DamageType, DamageProportion> damageDistributions =
            new Dictionary<DamageType, DamageProportion>();

        public DamageDistribution(DamageType damageType, double proportion)
        {
            this.damageDistributions.Add(damageType, new DamageProportion(damageType, proportion));
        }

        public DamageDistribution()
        {
        }

        public IEnumerator<DamageProportion> GetEnumerator()
        {
            return this.damageDistributions.Values.GetEnumerator();
        }

        public DamageDistribution Set(DamageType damageType, double proportion)
        {
            if (this.damageDistributions.ContainsKey(damageType))
            {
                throw new Exception("Damage proportion is already set for damage type: " + damageType);
            }

            this.damageDistributions.Add(damageType, new DamageProportion(damageType, proportion));
            return this;
        }

        public IReadOnlyList<DamageProportion> ToReadOnlyList()
        {
            return this.damageDistributions.Values.ToList();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
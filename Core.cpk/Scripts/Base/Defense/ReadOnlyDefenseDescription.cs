namespace AtomicTorch.CBND.CoreMod
{
    using System;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.CBND.GameApi.Data;

    public class ReadOnlyDefenseDescription
    {
        private ReadOnlyDefenseDescription(DefenseDescription d)
        {
            var multiplier = d.Multiplier;
            // calculate final values (apply multiplier)
            this.Chemical = multiplier * d.Chemical;
            this.Cold = multiplier * d.Cold;
            this.Electrical = multiplier * d.Electrical;
            this.Heat = multiplier * d.Heat;
            this.Impact = multiplier * d.Impact;
            this.Kinetic = multiplier * d.Kinetic;
            this.Psi = multiplier * d.Psi;
            this.Radiation = multiplier * d.Radiation;
        }

        public double Chemical { get; }

        public double Cold { get; }

        public double Electrical { get; }

        public double Heat { get; }

        public double Impact { get; }

        public double Kinetic { get; }

        public double Psi { get; }

        public double Radiation { get; }

        public static ReadOnlyDefenseDescription Create(DefenseDescription defenseDescription)
        {
            return new ReadOnlyDefenseDescription(defenseDescription);
        }

        public void FillEffects(IProtoEntity prototype, BaseStatsDictionary effects, double maximumDefensePercent = 1)
        {
            Add(StatName.DefenseImpact,     this.Impact);
            Add(StatName.DefenseKinetic,    this.Kinetic);
            Add(StatName.DefenseHeat,       this.Heat);
            Add(StatName.DefenseCold,       this.Cold);
            Add(StatName.DefenseChemical,   this.Chemical);
            Add(StatName.DefenseElectrical, this.Electrical);
            Add(StatName.DefenseRadiation,  this.Radiation);
            Add(StatName.DefensePsi,        this.Psi);

            void Add(StatName statName, double defensePercent)
            {
                if (defensePercent < 0
                    || defensePercent > maximumDefensePercent)
                {
                    throw new Exception(
                        $"Incorrect defense property value. It must be in range from 0 to {maximumDefensePercent} (inclusive). The defense {statName} for {prototype} is set to {defensePercent:F2}");
                }

                effects.AddValue(prototype, statName, defensePercent);
            }
        }
    }
}
namespace AtomicTorch.CBND.CoreMod
{
    public class DefenseDescription
    {
        public double Chemical;

        public double Cold;

        public double Electrical;

        public double Heat;

        public double Impact;

        public double Kinetic;

        public double Multiplier = 1;

        public double Psi;

        public double Radiation;

        public DefenseDescription Set(
            double chemical,
            double cold,
            double electrical,
            double heat,
            double impact,
            double kinetic,
            double psi,
            double radiation)
        {
            this.Chemical = chemical;
            this.Cold = cold;
            this.Electrical = electrical;
            this.Heat = heat;
            this.Impact = impact;
            this.Kinetic = kinetic;
            this.Psi = psi;
            this.Radiation = radiation;
            return this;
        }

        public DefenseDescription Set(ReadOnlyDefenseDescription readOnlyDefenseDescription)
        {
            var d = readOnlyDefenseDescription;
            this.Multiplier = 1;
            this.Chemical = d.Chemical;
            this.Cold = d.Cold;
            this.Electrical = d.Electrical;
            this.Heat = d.Heat;
            this.Impact = d.Impact;
            this.Kinetic = d.Kinetic;
            this.Psi = d.Psi;
            this.Radiation = d.Radiation;
            return this;
        }

        public DefenseDescription SetMultiplier(double multiplier)
        {
            this.Multiplier = multiplier;
            return this;
        }

        public ReadOnlyDefenseDescription ToReadOnly()
        {
            return ReadOnlyDefenseDescription.Create(this);
        }
    }
}
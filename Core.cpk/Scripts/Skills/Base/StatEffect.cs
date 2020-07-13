namespace AtomicTorch.CBND.CoreMod.Skills
{
    using System;
    using System.ComponentModel;
    using AtomicTorch.CBND.CoreMod.Stats;
    using AtomicTorch.GameEngine.Common.Extensions;

    public readonly struct StatEffect : ISkillEffect
    {
        public StatEffect(
            StatName statName,
            byte level,
            FuncBonusAtLevel formulaValueBonus,
            FuncBonusAtLevel formulaPercentBonus,
            double valueBonus,
            double percentBonus,
            bool displayTotalValue)
        {
            this.StatName = statName;
            this.Level = level;

            if (formulaValueBonus == null
                && formulaPercentBonus == null
                && valueBonus == 0
                && percentBonus == 0)
            {
                throw new ArgumentNullException(
                    $"Both provided formulas for stat effect \'{statName}\" are null and {nameof(valueBonus)} and {nameof(percentBonus)} are zero");
            }

            this.FormulaValueBonus = formulaValueBonus;
            this.FormulaPercentBonus = formulaPercentBonus;
            this.ValueBonus = valueBonus;
            this.PercentBonus = percentBonus;
            this.DisplayTotalValue = displayTotalValue;
        }

        public string Description
            => this.StatName.GetAttribute<DescriptionAttribute>()?.Description
               ?? this.StatName.ToString();

        public bool DisplayTotalValue { get; }

        /// <summary>
        /// Required level for the effect to take action.
        /// </summary>
        public byte Level { get; }

        public StatName StatName { get; }

        private FuncBonusAtLevel FormulaPercentBonus { get; }

        private FuncBonusAtLevel FormulaValueBonus { get; }

        private double PercentBonus { get; }

        private double ValueBonus { get; }

        public double CalcTotalPercentBonus(byte level)
        {
            var value = this.PercentBonus;

            var formula = this.FormulaPercentBonus;
            if (formula != null)
            {
                value += formula(level);
            }

            return value;
        }

        public double CalcTotalValueBonus(byte level)
        {
            var value = this.ValueBonus;

            var formula = this.FormulaValueBonus;
            if (formula != null)
            {
                value += formula(level);
            }

            return value;
        }
    }
}
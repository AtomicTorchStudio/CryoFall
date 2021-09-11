namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.GameApi;

    public class RateBossDifficultyPragmiumQueen
        : BaseRateDouble<RateBossDifficultyPragmiumQueen>
    {
        [NotLocalizable]
        public override string Description =>
            @"Difficulty of the Pragmium Queen (and the amount of loot/reward).
              The number corresponds to the number of players necessary to kill the boss
              with a reasonable challenge
              (with mechs or without mechs but in T4 armor, machineguns, with Stimpacks).                  
              You can change this rate to make it possible to kill the boss
              by a single player (set it to 1, or 1.5 for extra challenge and reward)
              or any other number of players up to 10.
              It also affects the number of loot piles you get when the boss is defeated.
              The value range is from 1 to 10 (inclusive).";

        public override string Id => "BossDifficulty.PragmiumQueen";

        public override string Name => "Boss difficulty: Pragmium Queen";

        public override double ValueDefault => 3;

        public override double ValueMax => 10;

        public override double ValueMin => 1;

        public override double ValueStepChange => 0.5;

        public override RateValueType ValueType => RateValueType.Number;

        public override RateVisibility Visibility => RateVisibility.Primary;
    }
}
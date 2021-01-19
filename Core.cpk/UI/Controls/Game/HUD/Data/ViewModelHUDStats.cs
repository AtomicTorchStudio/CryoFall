namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.CBND.GameApi.Extensions;

    public class ViewModelHUDStats : BaseViewModel
    {
        public ViewModelHUDStats()
        {
            if (IsDesignTime)
            {
                return;
            }

            // get current player character stats and subscribe on their changes
            var currentPlayerCharacter = Client.Characters.CurrentPlayerCharacter;
            var serverState = PlayerCharacter.GetPublicState(currentPlayerCharacter);
            var stats = serverState.CurrentStatsExtended;

            this.Health.ValueMax = stats.HealthMax;
            this.Health.ValueCurrent = stats.HealthCurrent;
            stats.ClientSubscribe(_ => _.HealthMax,     value => this.Health.ValueMax = value,     this);
            stats.ClientSubscribe(_ => _.HealthCurrent, value => this.Health.ValueCurrent = value, this);

            this.Stamina.ValueMax = stats.StaminaMax;
            this.Stamina.ValueCurrent = stats.StaminaCurrent;
            stats.ClientSubscribe(_ => _.StaminaMax,     value => this.Stamina.ValueMax = value,     this);
            stats.ClientSubscribe(_ => _.StaminaCurrent, value => this.Stamina.ValueCurrent = value, this);

            this.Food.ValueMax = stats.FoodMax;
            this.Food.ValueCurrent = stats.FoodCurrent;
            stats.ClientSubscribe(_ => _.FoodMax,     value => this.Food.ValueMax = value,     this);
            stats.ClientSubscribe(_ => _.FoodCurrent, value => this.Food.ValueCurrent = value, this);

            this.Water.ValueMax = stats.WaterMax;
            this.Water.ValueCurrent = stats.WaterCurrent;
            stats.ClientSubscribe(_ => _.WaterMax,     value => this.Water.ValueMax = value,     this);
            stats.ClientSubscribe(_ => _.WaterCurrent, value => this.Water.ValueCurrent = value, this);
        }

        public ViewModelHUDStatBar Food { get; }
            = new(CoreStrings.CharacterStatName_Food,
                foregroundColor: ApplyBrightness(Color.FromRgb(0x11, 0xCC, 0x55), 0.7),
                backgroundColor: ApplyBrightness(Color.FromRgb(0x11, 0xCC, 0x55), 0.075));

        public ViewModelHUDStatBar Health { get; }
            = new(CoreStrings.CharacterStatName_Health,
                foregroundColor: ApplyBrightness(Color.FromRgb(0xDD, 0x11, 0x33), 0.7),
                backgroundColor: ApplyBrightness(Color.FromRgb(0xDD, 0x11, 0x33), 0.075));

        public ViewModelHUDStatBar Stamina { get; }
            = new(CoreStrings.CharacterStatName_Stamina,
                foregroundColor: ApplyBrightness(Color.FromRgb(0xEE, 0xCC, 0x55), 0.7),
                backgroundColor: ApplyBrightness(Color.FromRgb(0xEE, 0xCC, 0x55), 0.075));

        public ViewModelHUDStatBar Water { get; }
            = new(CoreStrings.CharacterStatName_Water,
                foregroundColor: ApplyBrightness(Color.FromRgb(0x00, 0x9A, 0xFF), 0.7),
                backgroundColor: ApplyBrightness(Color.FromRgb(0x00, 0x9A, 0xFF), 0.075));

        private static Color ApplyBrightness(Color color, double brighness)
        {
            return Color.Multiply(color, (float)brighness)
                        .WithAlpha(0xFF);
        }
    }
}
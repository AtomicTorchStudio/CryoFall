namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;

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

        public ViewModelHUDStatBar Food { get; } = new ViewModelHUDStatBar(
            CoreStrings.CharacterStatName_Food,
            foregroundColor: Color.FromRgb(0x44, 0xBB, 0x00),
            backgroundColor: Color.FromRgb(0x00, 0x36, 0x00),
            fireColor: Color.FromRgb(0x44,       0xBB, 0x00));

        public ViewModelHUDStatBar Health { get; } = new ViewModelHUDStatBar(
            CoreStrings.CharacterStatName_Health,
            foregroundColor: Color.FromRgb(0xEE, 0x00, 0x00),
            backgroundColor: Color.FromRgb(0x71, 0x11, 0x00),
            fireColor: Color.FromRgb(0xEE,       0x00, 0x00));

        public ViewModelHUDStatBar Stamina { get; } = new ViewModelHUDStatBar(
            CoreStrings.CharacterStatName_Stamina,
            foregroundColor: Color.FromRgb(0xFF, 0xAA, 0x00),
            backgroundColor: Color.FromRgb(0x47, 0x25, 0x00),
            fireColor: Color.FromRgb(0xCC,       0xAA, 0x00));

        public ViewModelHUDStatBar Water { get; } = new ViewModelHUDStatBar(
            CoreStrings.CharacterStatName_Water,
            foregroundColor: Color.FromRgb(0x00, 0x99, 0xEE),
            backgroundColor: Color.FromRgb(0x00, 0x2A, 0x47),
            fireColor: Color.FromRgb(0x00,       0x99, 0xEE));
    }
}
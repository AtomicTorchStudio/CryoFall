namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD
{
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.ClientComponents.Core;
    using AtomicTorch.CBND.CoreMod.Systems.TimeOfDaySystem;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HUDGameTimeIndicator : BaseUserControl
    {
        public static readonly DependencyProperty CurrentTimeTextProperty =
            DependencyProperty.Register(nameof(CurrentTimeText),
                                        typeof(string),
                                        typeof(HUDGameTimeIndicator),
                                        new PropertyMetadata(default(string)));

        private readonly StringBuilder stringBuilder = new StringBuilder();

        private int lastHours = int.MaxValue;

        private int lastMinutes = int.MaxValue;

        public string CurrentTimeText
        {
            get => (string)this.GetValue(CurrentTimeTextProperty);
            set => this.SetValue(CurrentTimeTextProperty, value);
        }

        protected override void InitControl()
        {
        }

        protected override void OnLoaded()
        {
            ClientComponentUpdateHelper.UpdateCallback += this.Update;
        }

        protected override void OnUnloaded()
        {
            ClientComponentUpdateHelper.UpdateCallback -= this.Update;
        }

        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        private void Update()
        {
            var time = TimeOfDaySystem.CurrentTimeOfDayHours;
            var hours = (int)time;
            var minutes = (int)(60 * ((time - hours) % 1.0));

            if (this.lastHours == hours
                && this.lastMinutes == minutes)
            {
                return;
            }

            this.lastHours = hours;
            this.lastMinutes = minutes;

            this.stringBuilder
                .Append(hours.ToString("00"))
                .Append(":")
                .Append(minutes.ToString("00"));

            this.CurrentTimeText = this.stringBuilder.ToString();
            this.stringBuilder.Clear();
        }
    }
}
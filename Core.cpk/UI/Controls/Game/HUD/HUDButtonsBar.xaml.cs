namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD
{
    using System.Windows.Controls;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class HUDButtonsBar : BaseUserControl
    {
        public static HUDButtonsBar Instance { get; private set; }

        public UIElementCollection Buttons { get; private set; }

        protected override void InitControl()
        {
            Instance = this;
            this.Buttons = this.GetByName<StackPanel>("ButtonsStackPanel").Children;
        }
    }
}
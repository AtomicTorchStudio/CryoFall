namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Explosives
{
    using System.Windows.Controls;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class BombCountdownDisplay : BaseUserControl
    {
        public TextBlock TextBlock { get; private set; }

        protected override void InitControl()
        {
            base.InitControl();
            this.TextBlock = this.GetByName<TextBlock>("TextBlockControl");
        }
    }
}
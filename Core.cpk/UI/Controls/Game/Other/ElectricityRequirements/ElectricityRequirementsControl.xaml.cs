namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Other.ElectricityRequirements
{
    using System.Windows;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class ElectricityRequirementsControl : BaseUserControl
    {
        public static readonly DependencyProperty RequiredElectricityAmountProperty =
            DependencyProperty.Register("RequiredElectricityAmount",
                                        typeof(uint),
                                        typeof(ElectricityRequirementsControl),
                                        new PropertyMetadata(default(uint)));

        public uint RequiredElectricityAmount
        {
            get => (uint)this.GetValue(RequiredElectricityAmountProperty);
            set => this.SetValue(RequiredElectricityAmountProperty, value);
        }

        protected override void InitControl()
        {
        }
    }
}
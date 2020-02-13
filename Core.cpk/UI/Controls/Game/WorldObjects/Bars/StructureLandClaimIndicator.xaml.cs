namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Bars
{
    using System.Windows;
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public partial class StructureLandClaimIndicator : BaseUserControl, ICacheableControl
    {
        public static readonly DependencyProperty IsClaimedProperty =
            DependencyProperty.Register(nameof(IsClaimed),
                                        typeof(bool),
                                        typeof(StructureLandClaimIndicator),
                                        new PropertyMetadata(default(bool)));

        public IComponentAttachedControl AttachedToComponent;

        public bool IsClaimed
        {
            get => (bool)this.GetValue(IsClaimedProperty);
            set => this.SetValue(IsClaimedProperty, value);
        }

        public void ResetControlForCache()
        {
        }

        public void Setup(bool isClaimed)
        {
            this.IsClaimed = isClaimed;
        }

        protected override void InitControl()
        {
        }
    }
}
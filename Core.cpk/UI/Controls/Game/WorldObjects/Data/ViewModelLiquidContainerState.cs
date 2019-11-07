namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures.Barrels;
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelLiquidContainerState : BaseViewModel
    {
        protected readonly LiquidContainerState LiquidContainerState;

        private readonly LiquidType liquidType;

        public ViewModelLiquidContainerState(
            LiquidContainerState liquidContainerState,
            LiquidContainerConfig liquidContainerConfig,
            LiquidType liquidType = LiquidType.Water)
        {
            this.LiquidContainerState = liquidContainerState;
            this.liquidType = liquidType;
            this.Capacity = liquidContainerConfig.Capacity;

            liquidContainerState.ClientSubscribe(
                _ => _.Amount,
                _ => this.RefreshAmount(),
                this);

            this.RefreshAmount();
        }

        public double Amount { get; set; }

        public double Capacity { get; }

        public Color LiquidColor
        {
            get
            {
                var (liquidColor, liquidIcon) = LiquidColorIconHelper.GetColorAndIcon(this.liquidType);
                return liquidColor;
            }
        }

        public Brush LiquidIcon
        {
            get
            {
                var (liquidColor, liquidIcon) = LiquidColorIconHelper.GetColorAndIcon(this.liquidType);
                return liquidIcon;
            }
        }

        public string LiquidTitle
            => this.liquidType.GetDescription();

        public void RefreshAmount()
        {
            this.Amount = this.LiquidContainerState.Amount;
        }
    }
}
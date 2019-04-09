namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using AtomicTorch.CBND.CoreMod.Systems.LiquidContainer;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.State;

    public class ViewModelLiquidContainerState : BaseViewModel
    {
        protected readonly LiquidContainerState LiquidContainerState;

        public ViewModelLiquidContainerState(
            LiquidContainerState liquidContainerState,
            LiquidContainerConfig liquidContainerConfig)
        {
            this.LiquidContainerState = liquidContainerState;
            this.Capacity = liquidContainerConfig.Capacity;

            liquidContainerState.ClientSubscribe(
                _ => _.Amount,
                _ => this.RefreshAmount(),
                this);

            this.RefreshAmount();
        }

        public float Amount { get; set; }

        public float Capacity { get; }

        public void RefreshAmount()
        {
            this.Amount = this.LiquidContainerState.Amount;
        }
    }
}
namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using AtomicTorch.CBND.CoreMod.Rates;

    public class ViewModelRateInt : BaseViewModelRateWithRange<int>
    {
        public ViewModelRateInt(IRateWithRange<int> rate) : base(rate)
        {
        }
    }
}
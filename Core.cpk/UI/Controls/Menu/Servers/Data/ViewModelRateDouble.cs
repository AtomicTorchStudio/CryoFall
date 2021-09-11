namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using AtomicTorch.CBND.CoreMod.Rates;

    public class ViewModelRateDouble : BaseViewModelRateWithRange<double>
    {
        public ViewModelRateDouble(IRateWithRange<double> rate) : base(rate)
        {
        }
    }
}
namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using AtomicTorch.CBND.CoreMod.Rates;

    public class ViewModelRateString : BaseViewModelRate<string>
    {
        public ViewModelRateString(IRate<string> rate) : base(rate)
        {
        }
    }
}
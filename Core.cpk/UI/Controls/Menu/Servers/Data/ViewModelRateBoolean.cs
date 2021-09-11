namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using AtomicTorch.CBND.CoreMod.Rates;

    public class ViewModelRateBoolean : BaseViewModelRate<bool>
    {
        public ViewModelRateBoolean(IRate<bool> rate) : base(rate)
        {
        }
    }
}
namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using AtomicTorch.CBND.CoreMod.Rates;

    public class ViewModelRateUint : BaseViewModelRateWithRange<uint>
    {
        public ViewModelRateUint(IRateWithRange<uint> rate) : base(rate)
        {
        }
    }
}
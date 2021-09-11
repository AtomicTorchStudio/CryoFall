namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using AtomicTorch.CBND.CoreMod.Rates;

    public class ViewModelRateUshort : BaseViewModelRateWithRange<ushort>
    {
        public ViewModelRateUshort(IRateWithRange<ushort> rate) : base(rate)
        {
        }
    }
}
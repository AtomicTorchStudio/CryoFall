namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using AtomicTorch.CBND.CoreMod.Rates;

    public class ViewModelRateByte : BaseViewModelRateWithRange<byte>
    {
        public ViewModelRateByte(IRateWithRange<byte> rate) : base(rate)
        {
        }
    }
}
namespace AtomicTorch.CBND.CoreMod.Rates
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    public interface IRate
    {
        object AbstractValueDefault { get; }

        string Description { get; }

        string Id { get; }

        string Name { get; }

        IRate OrderAfterRate { get; }

        object SharedAbstractValue { get; }

        RateVisibility Visibility { get; }

        IViewModelRate ClientCreateViewModel();

        void ClientSetAbstractValue(object value);

        void ServerInit();

        void SharedApplyToConfig(IServerRatesConfig ratesConfig, object value);
    }

    public interface IRate<out TValue> : IRate
    {
        TValue SharedValue { get; }

        TValue ValueDefault { get; }
    }
}
namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.CurrentGame.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelCurrentServerRatesBrowserControl : BaseViewModel
    {
        public ViewModelCurrentServerRatesBrowserControl()
        {
            RatesManager.ClientRatesReceived += this.ClientRatesReceivedHandler;
            this.ApplyActualRates();
        }

        public IReadOnlyList<IViewModelRate> RatesAll { get; private set; }

        public IReadOnlyList<IViewModelRate> RatesModified { get; private set; }

        public IReadOnlyList<IViewModelRate> RatesUnmodified { get; private set; }

        protected override void DisposeViewModel()
        {
            RatesManager.ClientRatesReceived -= this.ClientRatesReceivedHandler;
            base.DisposeViewModel();
        }

        private void ApplyActualRates()
        {
            this.RatesAll = RatesManager
                            .Rates
                            .Where(r => r.Visibility != RateVisibility.Hidden)
                            .OrderBy(r => r.Visibility)
                            .ThenBy(r => r.OrderAfterRate is null)
                            .ThenBy(r => r.Name)
                            .TopologicalSort(GetRateOrder)
                            .Select(e => e.ClientCreateViewModel())
                            .ToList();

            foreach (var viewModelRate in this.RatesAll)
            {
                viewModelRate.SetAbstractValue(viewModelRate.Rate.SharedAbstractValue);
            }

            this.RatesModified = this.RatesAll.Where(r => !r.IsDefaultValue)
                                     .ToList();

            this.RatesUnmodified = this.RatesAll.Where(r => r.IsDefaultValue)
                                       .ToList();

            // local helper method for getting rate order
            static IEnumerable<IRate> GetRateOrder(IRate rate)
            {
                if (rate.OrderAfterRate is not null)
                {
                    yield return rate.OrderAfterRate;
                }
            }
        }

        private void ClientRatesReceivedHandler()
        {
            this.ApplyActualRates();
        }
    }
}
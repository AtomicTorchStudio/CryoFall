namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.CBND.CoreMod.RatesPresets.Base;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelRatesEditorControl : BaseViewModel
    {
        public ViewModelRatesEditorControl(Action callbackAnyRateChanged)
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
                viewModelRate.ValueChangedCallback = callbackAnyRateChanged;
            }

            this.RatesPrimary = this.RatesAll
                                    .Where(r => r.Rate.Visibility == RateVisibility.Primary)
                                    .ToList();

            this.RatesAdvanced = this.RatesAll
                                     .Where(r => r.Rate.Visibility == RateVisibility.Advanced)
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

        public bool AreAdvancedRatesVisible { get; set; }

        public IReadOnlyList<IViewModelRate> RatesAdvanced { get; }

        public IReadOnlyList<IViewModelRate> RatesAll { get; }

        public IReadOnlyList<IViewModelRate> RatesPrimary { get; }

        public void ApplyPreset(BaseRatesPreset preset)
        {
            foreach (var viewModelRate in this.RatesAll)
            {
                viewModelRate.ResetToDefault();
            }

            var ratesDictionary = this.RatesAll.ToDictionary(p => p.Rate);
            foreach (var pair in preset.Rates)
            {
                ratesDictionary[pair.Key].SetAbstractValue(pair.Value);
            }
        }

        public void SetRates(Dictionary<IRate, object> ratesDictionary)
        {
            foreach (var viewModelRate in this.RatesAll)
            {
                if (ratesDictionary.TryGetValue(viewModelRate.Rate, out var value))
                {
                    viewModelRate.SetAbstractValue(value);
                }
            }
        }
    }
}
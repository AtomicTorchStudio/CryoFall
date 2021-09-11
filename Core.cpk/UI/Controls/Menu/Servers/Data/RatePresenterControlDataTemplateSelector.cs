namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using System.Windows;
    using System.Windows.Controls;

    public class RatePresenterControlDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate TemplateDefault { get; set; }

        public DataTemplate TemplateDouble { get; set; }

        public DataTemplate TemplateWithRange { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                ViewModelRateDouble     => this.TemplateDouble,
                IViewModelRateWithRange => this.TemplateWithRange,
                _                       => this.TemplateDefault
            };
        }
    }
}
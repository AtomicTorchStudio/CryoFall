namespace AtomicTorch.CBND.CoreMod.UI.Controls.Menu.Servers.Data
{
    using System;
    using AtomicTorch.CBND.CoreMod.Rates;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public interface IViewModelRate
    {
        public BaseCommand CommandSetDefault { get; }

        string Id { get; }

        public bool IsDefaultValue { get; }

        string Name { get; }

        IRate Rate { get; }

        Action ValueChangedCallback { get; set; }

        public object ValueDefault { get; }

        string ValueDefaultText { get; }

        object GetAbstractValue();

        void ResetToDefault();

        void SetAbstractValue(object value);
    }
}
namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core.Menu
{
    using System;

    public interface IMenu : IDisposable
    {
        event Action IsOpenedChanged;

        bool IsOpened { get; }

        void InitMenu();

        void Toggle();
    }
}
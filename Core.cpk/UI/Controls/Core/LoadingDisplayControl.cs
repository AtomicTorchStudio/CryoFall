namespace AtomicTorch.CBND.CoreMod.UI.Controls.Core
{
    using System.Windows;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class LoadingDisplayControl : BaseControl
    {
        static LoadingDisplayControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(LoadingDisplayControl),
                new FrameworkPropertyMetadata(typeof(LoadingDisplayControl)));
        }
    }
}
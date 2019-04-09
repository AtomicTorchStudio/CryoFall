namespace AtomicTorch.CBND.CoreMod.Editor.Tools.EditorToolGenerator
{
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewModelHeightSetting : BaseViewModel
    {
        public ViewModelHeightSetting(double maxValue, BaseCommand commandDelete)
        {
            this.MaxValue = maxValue;
            this.CommandDelete = commandDelete;
        }

        public ViewModelHeightSetting()
        {
        }

        public BaseCommand CommandDelete { get; }

        public double MaxValue { get; set; }
    }
}
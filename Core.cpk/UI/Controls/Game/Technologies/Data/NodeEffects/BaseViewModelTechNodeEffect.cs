namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class BaseViewModelTechNodeEffect : BaseViewModel
    {
        private readonly BaseTechNodeEffect effect;

        protected BaseViewModelTechNodeEffect(BaseTechNodeEffect effect)
        {
            this.effect = effect;
        }

        public Brush Icon => Api.Client.UI.GetTextureBrush(this.effect.Icon);
    }
}
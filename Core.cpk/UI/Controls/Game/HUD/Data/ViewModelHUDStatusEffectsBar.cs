namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.HUD.Data
{
    using System.Collections.ObjectModel;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;
    using AtomicTorch.CBND.GameApi.Data.Logic;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class ViewModelHUDStatusEffectsBar : BaseViewModel
    {
        private NetworkSyncListObservableWrapperWithConverter<ILogicObject, ViewModelStatusEffect>
            currentStatusEffectsWrapper;

        public ViewModelHUDStatusEffectsBar(NetworkSyncList<ILogicObject> statusEffects)
        {
            this.currentStatusEffectsWrapper = statusEffects.ToObservableCollectionWithWrapper(
                s => new ViewModelStatusEffect(s));
        }

        public ObservableCollection<ViewModelStatusEffect> CurrentStatusEffects
            => this.currentStatusEffectsWrapper.ObservableCollection;

        public void Flicker<TProtoStatusEffect>()
            where TProtoStatusEffect : IProtoStatusEffect
        {
            foreach (var viewModel in this.CurrentStatusEffects)
            {
                if (viewModel.ProtoStatusEffect is TProtoStatusEffect)
                {
                    // found the status effect view model
                    viewModel.Flicker();
                    return;
                }
            }
        }

        protected override void DisposeViewModel()
        {
            this.currentStatusEffectsWrapper.Dispose();
            this.currentStatusEffectsWrapper = null;
            base.DisposeViewModel();
        }
    }
}
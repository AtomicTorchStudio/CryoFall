namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.Data
{
    using System.Collections.ObjectModel;
    using AtomicTorch.CBND.CoreMod.CharacterStatusEffects;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Crafting.Data;
    using AtomicTorch.CBND.GameApi.Data.State.NetSync;

    public class ViewModelCharacterPublicStatusEffects : BaseViewModel
    {
        private NetworkSyncListObservableWrapperWithConverter
            <IProtoStatusEffect, ViewModelPublicStatusEffect> wrapper;

        public ViewModelCharacterPublicStatusEffects(NetworkSyncList<IProtoStatusEffect> publicStatusEffects)
        {
            this.wrapper = new NetworkSyncListObservableWrapperWithConverter
                <IProtoStatusEffect, ViewModelPublicStatusEffect>(publicStatusEffects,
                                                                  effect => new ViewModelPublicStatusEffect(effect));
        }

        public ObservableCollection<ViewModelPublicStatusEffect> StatusEffects
            => this.wrapper.ObservableCollection;

        protected override void DisposeViewModel()
        {
            this.wrapper.Dispose();
            this.wrapper = null;
            base.DisposeViewModel();
        }
    }
}
namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Completionist.Data
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Completionist;
    using AtomicTorch.CBND.GameApi.Data;

    public class ViewModelCompletionistPageFish
        : BaseViewModelCompletionistPage<DataEntryCompletionistFish, ViewDataEntryFishCompletionist>
    {
        public ViewModelCompletionistPageFish(
            Dictionary<IProtoEntity, ViewDataEntryFishCompletionist> binding,
            int columnsCount,
            int iconSize,
            Action entriesPendingCountChanged)
            : base(binding, columnsCount, iconSize, entriesPendingCountChanged)
        {
        }

        protected override void RefreshViewModelState(
            DataEntryCompletionistFish value,
            ViewDataEntryFishCompletionist viewModel)
        {
            base.RefreshViewModelState(value, viewModel);
            viewModel.MaxWeight = value.MaxWeight;
            viewModel.MaxLength = value.MaxLength;
        }
    }
}
namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Completionist.Data
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Systems.Completionist;
    using AtomicTorch.CBND.GameApi.Data;

    public class ViewModelCompletionistPageDefault
        : BaseViewModelCompletionistPage<DataEntryCompletionist, ViewDataEntryCompletionist>
    {
        public ViewModelCompletionistPageDefault(
            Dictionary<IProtoEntity, ViewDataEntryCompletionist> binding,
            int columnsCount,
            int iconSize,
            Action entriesPendingCountChanged)
            : base(binding, columnsCount, iconSize, entriesPendingCountChanged)
        {
        }
    }
}
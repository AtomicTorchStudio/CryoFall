namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.ConstructionTooltip.Data
{
    using System;
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.Items.Tools;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Systems.Construction;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public abstract class BaseViewModelConstructionRequirementsTooltip : BaseViewModel
    {
        private ushort stageCountRemains;

        protected BaseViewModelConstructionRequirementsTooltip()
        {
            ClientHotbarSelectedItemManager.SelectedItemChanged += this.HotbarSelectedItemChangedHandler;
            this.RefreshCanBuildOrRepairNow();
        }

        public abstract string ActionTitle { get; }

        public bool CanBuildOrRepairNow { get; private set; }

        public ushort StageCountRemains
        {
            get => this.stageCountRemains;
            protected set
            {
                if (this.stageCountRemains == value)
                {
                    return;
                }

                this.stageCountRemains = value;
                this.NotifyThisPropertyChanged();
            }
        }

        public abstract IReadOnlyList<ProtoItemWithCount> StageRequiredItems { get; }

        protected ushort CalculateStagesCount(
            IConstructionStageConfigReadOnly config,
            double structurePointsCurrent,
            double structurePointsMax,
            bool isAllowTolerance)
        {
            var totalStages = config.StagesCount;
            int currentStage;

            if (isAllowTolerance)
            {
                currentStage = (int)Math.Round(totalStages * structurePointsCurrent / structurePointsMax,
                                               MidpointRounding.AwayFromZero);
            }
            else
            {
                currentStage = (int)(totalStages * structurePointsCurrent / structurePointsMax);
            }

            var stageCountRemains = totalStages - currentStage;
            if (stageCountRemains < 0)
            {
                // should not be possible, but still...
                stageCountRemains = 0;
            }

            return (ushort)stageCountRemains;
        }

        protected override void DisposeViewModel()
        {
            ClientHotbarSelectedItemManager.SelectedItemChanged -= this.HotbarSelectedItemChangedHandler;
            base.DisposeViewModel();
        }

        private void HotbarSelectedItemChangedHandler(IItem obj)
        {
            this.RefreshCanBuildOrRepairNow();
        }

        private void RefreshCanBuildOrRepairNow()
        {
            var selectedItem = ClientHotbarSelectedItemManager.SelectedItem;
            this.CanBuildOrRepairNow = selectedItem?.ProtoItem is IProtoItemToolToolbox;
        }
    }
}
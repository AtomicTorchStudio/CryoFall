namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Technologies.Data
{
    using System.Collections.Generic;
    using AtomicTorch.CBND.CoreMod.StaticObjects.Structures;
    using AtomicTorch.CBND.CoreMod.Systems;
    using AtomicTorch.CBND.CoreMod.Technologies;
    using AtomicTorch.CBND.GameApi.Data.Items;

    public class ViewModelTechNodeEffectStructureUnlock : BaseViewModelTechNodeEffect
    {
        private readonly TechNodeEffectStructureUnlock effect;

        public ViewModelTechNodeEffectStructureUnlock(TechNodeEffectStructureUnlock effect) : base(effect)
        {
            this.effect = effect;
        }

        public IProtoItem[] RequiredProtoItems
        {
            get
            {
                var items = this.effect.Structure
                                .ConfigBuild
                                .StageRequiredItems;

                if (items.Count == 0)
                {
                    // maybe there are upgrades that results in this structure?
                    items = this.FindUpgradeItemsList();
                }

                var array = new IProtoItem[items.Count];
                for (var index = 0; index < items.Count; index++)
                {
                    array[index] = items[index].ProtoItem;
                }

                return array;
            }
        }

        public string Title => this.effect.Structure.Name;

        private IReadOnlyList<ProtoItemWithCount> FindUpgradeItemsList()
        {
            var upgradeTargetStructure = this.effect.Structure;

            foreach (var protoObjectStructure in StructuresHelper.AllConstructableStructuresIncludingUpgrades)
            {
                var configUpgrade = protoObjectStructure.ConfigUpgrade;
                if (configUpgrade is null
                    || configUpgrade.Entries.Count == 0)
                {
                    continue;
                }

                foreach (var upgradeEntry in configUpgrade.Entries)
                {
                    if (ReferenceEquals(upgradeEntry.ProtoStructure, upgradeTargetStructure))
                    {
                        // found a source structure that could be upgraded to the target structure
                        return upgradeEntry.RequiredItems;
                    }
                }
            }

            return null;
        }
    }
}
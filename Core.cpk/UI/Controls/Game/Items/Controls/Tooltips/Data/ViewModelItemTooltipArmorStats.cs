namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data
{
    using System.Collections.Generic;
    using System.Windows.Media;
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Equipment;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.CBND.GameApi.Data.Weapons;
    using AtomicTorch.GameEngine.Common.Extensions;
    using AtomicTorch.GameEngine.Common.Helpers;

    public class ViewModelItemTooltipArmorStats : BaseViewModel
    {
        public ViewModelItemTooltipArmorStats(IProtoItemEquipment protoItem)
        {
            var effects = protoItem.ProtoEffects;

            var damageTypes = EnumExtensions.GetValues<DamageType>();
            var primaryEntriesCount = 3;

            var arrayPrimary = new DataDefense[primaryEntriesCount];
            var arraySecondary = new DataDefense[damageTypes.Length - primaryEntriesCount];

            var hasAnyNonZeroProtection = false;

            for (var index = 0; index < primaryEntriesCount; index++)
            {
                AddEntry(index);
            }

            for (var index = primaryEntriesCount; index < damageTypes.Length; index++)
            {
                AddEntry(index);
            }

            this.DefenseEntriesPrimary = arrayPrimary;
            this.DefenseEntriesSecondary = arraySecondary;
            this.HasAnyNonZeroProtection = hasAnyNonZeroProtection;

            void AddEntry(int index)
            {
                var damageType = damageTypes[index];
                var defenseStatName = WeaponDamageSystem.SharedGetDefenseStatName(damageType);

                byte defenseValue = 0;
                if (effects.Values.TryGetValue(defenseStatName, out var defense))
                {
                    defenseValue = (byte)MathHelper.Clamp(defense * 100, 0, 100);
                }

                var entry = new DataDefense(damageType, defenseValue);

                if (index < primaryEntriesCount)
                {
                    arrayPrimary[index] = entry;
                }
                else
                {
                    arraySecondary[index - primaryEntriesCount] = entry;
                }

                if (defenseValue > 0)
                {
                    hasAnyNonZeroProtection = true;
                }
            }
        }

        public IReadOnlyList<DataDefense> DefenseEntriesPrimary { get; }

        public IReadOnlyList<DataDefense> DefenseEntriesSecondary { get; }

        public bool HasAnyNonZeroProtection { get; }

        public readonly struct DataDefense
        {
            public DataDefense(DamageType damageType, byte defense)
            {
                this.DamageType = damageType;
                this.Defense = defense;
            }

            public DamageType DamageType { get; }

            public string DamageTypeName => this.DamageType.GetDescription();

            public byte Defense { get; }

            public ImageSource IconImageSource => ClientDamageTypeIconHelper.GetImageSource(this.DamageType);
        }
    }
}
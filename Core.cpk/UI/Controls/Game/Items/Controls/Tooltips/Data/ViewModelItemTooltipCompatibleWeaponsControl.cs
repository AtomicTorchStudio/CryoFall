namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelItemTooltipCompatibleWeaponsControl : BaseViewModel
    {
        public ViewModelItemTooltipCompatibleWeaponsControl(IProtoItemAmmo protoItemAmmo)
        {
            this.CompatibleWeaponProtos = protoItemAmmo.CompatibleWeaponProtos
                                                       .OrderBy(e => e.GetType().Namespace)
                                                       .ThenBy(e => e.Name)
                                                       .Select(e => e.Name)
                                                       .ToArray();
        }

        public IReadOnlyCollection<string> CompatibleWeaponProtos { get; }
    }
}
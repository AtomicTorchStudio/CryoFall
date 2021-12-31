namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelItemTooltipCompatibleWeaponsControl : BaseViewModel
    {
        public ViewModelItemTooltipCompatibleWeaponsControl(IProtoItemAmmo protoItemAmmo)
        {
            this.CompatibleWeaponProtos = protoItemAmmo.CompatibleWeaponProtos
                                                       .Where(p => p.Icon != null
                                                                   && !((IProtoItemWithSkinData)p).IsSkin)
                                                       .OrderBy(p => p.GetType().Namespace)
                                                       .ThenBy(p => p.Name)
                                                       .Select(p => p.Name)
                                                       .ToArray();
        }

        public IReadOnlyCollection<string> CompatibleWeaponProtos { get; }
    }
}
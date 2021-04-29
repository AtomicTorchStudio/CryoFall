namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;

    public class ViewModelItemTooltipCompatibleAmmoControl : BaseViewModel
    {
        public ViewModelItemTooltipCompatibleAmmoControl(IProtoItemWeapon protoItemWeapon)
        {
            this.CompatibleAmmoProtos = protoItemWeapon.CompatibleAmmoProtos
                                                       .OrderBy(p => p.Name)
                                                       .Select(p => p.Name)
                                                       .ToList();
        }

        public IReadOnlyCollection<string> CompatibleAmmoProtos { get; }
    }
}
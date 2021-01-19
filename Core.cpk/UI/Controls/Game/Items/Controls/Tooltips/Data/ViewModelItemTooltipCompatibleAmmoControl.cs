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
            var list = protoItemWeapon.CompatibleAmmoProtos
                                      .OrderBy(e => e.Name)
                                      .Select(e => e.Name)
                                      .ToList();
            this.CompatibleAmmoProtos = list;
        }

        public IReadOnlyCollection<string> CompatibleAmmoProtos { get; }
    }
}
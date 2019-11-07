namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Items.Controls.Tooltips.Data
{
    using System.Collections.Generic;
    using System.Linq;
    using AtomicTorch.CBND.CoreMod.Items.Ammo;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.CoreMod.UI.Controls.Core;
    using AtomicTorch.GameEngine.Common.Extensions;

    public class ViewModelItemCompatibleAmmo : BaseViewModel
    {
        public ViewModelItemCompatibleAmmo(IProtoItemWeapon protoItemWeapon)
        {
            var list = protoItemWeapon.CompatibleAmmoProtos.ToList();
            list.SortBy(e => e.Name);
            this.CompatibleAmmoProtos = list;
        }

        public IReadOnlyCollection<IProtoItemAmmo> CompatibleAmmoProtos { get; }
    }
}
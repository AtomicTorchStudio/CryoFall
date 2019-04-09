namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class WeaponState
    {
        public IItem ActiveItemWeapon;

        public IProtoItemWeapon ActiveProtoWeapon;

        public double CooldownSecondsRemains;

        public double DamageApplyDelaySecondsRemains;

        public bool IsFiring;

        public double TimeSinceFirstShot;

        public WeaponFinalCache WeaponCache;

        private bool inputIsFiring;

        private WeaponReloadingState weaponReloadingState;

        public event Action WeaponReloadingStateChanged;

        public bool IsEventWeaponStartSent { get; set; }

        public uint? ServerLastClientReportedShotsDoneCount { get; set; }

        public uint ShotsDone { get; set; }

        public WeaponReloadingState WeaponReloadingState
        {
            get => this.weaponReloadingState;
            set
            {
                if (this.weaponReloadingState == value)
                {
                    return;
                }

                this.weaponReloadingState = value;
                this.WeaponReloadingStateChanged?.Invoke();
            }
        }

        public void SetInputIsFiring(
            bool inputIsFiring,
            uint? shotsDone = null)
        {
            this.inputIsFiring = inputIsFiring;
            if (inputIsFiring
                || Api.IsClient)
            {
                this.ServerLastClientReportedShotsDoneCount = null;
                return;
            }

            this.ServerLastClientReportedShotsDoneCount = shotsDone;
        }

        public void SetWeaponItem(IItem item, IProtoItemWeapon protoItem)
        {
            if (item != null)
            {
                protoItem = item.ProtoGameObject as IProtoItemWeapon;
                item = protoItem != null ? item : null;
            }

            if (this.ActiveItemWeapon == item
                && this.ActiveProtoWeapon == protoItem)
            {
                // no need to change
                return;
            }

            this.ActiveItemWeapon = item;
            this.ActiveProtoWeapon = protoItem;
            this.WeaponCache = null;
            this.IsEventWeaponStartSent = false;

            this.ShotsDone = 0;
            this.ServerLastClientReportedShotsDoneCount = 0;

            // cancel firing input
            this.SetInputIsFiring(false);

            if (Api.IsClient)
            {
                WeaponAmmoSystem.ClientTryAbortReloading();
            }
        }

        public void SetWeaponProtoOnly(IProtoItemWeapon activeProtoWeapon)
        {
            this.ActiveItemWeapon = null;
            this.ActiveProtoWeapon = activeProtoWeapon;
            this.WeaponCache = null;
            this.IsEventWeaponStartSent = false;
        }

        public bool SharedGetInputIsFiring()
        {
            if (Api.IsClient)
            {
                return this.inputIsFiring;
            }

            if (this.inputIsFiring)
            {
                return true;
            }

            return this.ServerLastClientReportedShotsDoneCount.HasValue
                   && this.ShotsDone < this.ServerLastClientReportedShotsDoneCount;
        }
    }
}
namespace AtomicTorch.CBND.CoreMod.Systems.Weapons
{
    using System;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Data.Items;
    using AtomicTorch.CBND.GameApi.Scripting;

    public class WeaponState
    {
        public double CooldownSecondsRemains;

        public double ReadySecondsRemains;

        public double DamageApplyDelaySecondsRemains;

        public double FirePatternCooldownSecondsRemains;

        public ushort FirePatternCurrentShotNumber;

        public bool IsEventWeaponStartSent;

        public bool IsFiring;

        public IItem ItemWeapon;

        public IProtoItemWeapon ProtoWeapon;

        public uint? ServerLastClientReportedShotsDoneCount;

        public uint ShotsDone;

        public WeaponFinalCache WeaponCache;

        private bool inputIsFiring;

        private WeaponReloadingState weaponReloadingState;

        public event Action ClientActiveWeaponChanged;

        public event Action ClientWeaponReloadingStateChanged;

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
                if (Api.IsClient)
                {
                    this.ClientWeaponReloadingStateChanged?.Invoke();
                }
            }
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

        public void SharedSetInputIsFiring(
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

        public void SharedSetWeaponItem(IItem item, IProtoItemWeapon protoItem)
        {
            if (item != null)
            {
                protoItem = item.ProtoGameObject as IProtoItemWeapon;
                item = protoItem != null ? item : null;
            }

            if (this.ItemWeapon == item
                && this.ProtoWeapon == protoItem)
            {
                // no need to change
                return;
            }

            this.ItemWeapon = item;
            this.ProtoWeapon = protoItem;
            this.WeaponCache = null;
            this.IsEventWeaponStartSent = false;

            this.ShotsDone = 0;
            this.ServerLastClientReportedShotsDoneCount = 0;

            // cancel firing input
            this.SharedSetInputIsFiring(false);

            if (Api.IsClient)
            {
                WeaponAmmoSystem.ClientTryAbortReloading();
            }

            this.SharedOnWeaponChanged();
        }

        public void SharedSetWeaponProtoOnly(IProtoItemWeapon activeProtoWeapon)
        {
            this.ItemWeapon = null;
            this.ProtoWeapon = activeProtoWeapon;
            this.WeaponCache = null;
            this.IsEventWeaponStartSent = false;

            this.SharedOnWeaponChanged();
        }

        private void SharedOnWeaponChanged()
        {
            this.CooldownSecondsRemains 
                = this.ReadySecondsRemains 
                      = Math.Max(this.CooldownSecondsRemains,
                                                   this.ProtoWeapon?.ReadyDelayDuration ?? 0);

            if (Api.IsClient)
            {
                this.ClientActiveWeaponChanged?.Invoke();
            }
        }
    }
}
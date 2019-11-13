namespace AtomicTorch.CBND.CoreMod.Systems.Cursor
{
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ClientCursorWeaponSystem
    {
        private static readonly CursorResource CrosshairNormal
            = new CursorResource("cursor_crosshair_normal",
                                 pivotPosition: (24, 24));

        private static readonly CursorResource CrosshairOffset1
            = new CursorResource("cursor_crosshair_offset1",
                                 pivotPosition: (32, 32));

        private static readonly CursorResource CrosshairOffset2
            = new CursorResource("cursor_crosshair_offset2",
                                 pivotPosition: (48, 48));

        private static readonly CursorResource CrosshairReloading
            = new CursorResource("cursor_crosshair_reloading",
                                 pivotPosition: (48, 48));

        public static CursorResource CurrentCursorResource { get; set; }

        public static void Update()
        {
            CurrentCursorResource = GetCurrentCursorResource();
        }

        private static CursorResource GetCurrentCursorResource()
        {
            if (Api.Client.UI.GetVisualInPointedPosition() != null)
            {
                return null;
            }

            var state = ClientCurrentCharacterHelper.PrivateState?.WeaponState;
            var protoWeapon = state?.ProtoWeapon;
            if (!(protoWeapon is IProtoItemWeaponRanged))
            {
                return null;
            }

            if ((state.WeaponReloadingState?.SecondsToReloadRemains ?? 0) > 0
                || state.ReadySecondsRemains > 0
                || !state.ProtoWeapon.SharedCanFire(ClientCurrentCharacterHelper.Character, state))
                 
            {
                return CrosshairReloading;
            }

            var pattern = protoWeapon.FirePatternPreset;
            if (!pattern.IsEnabled)
            {
                // no weapon fire spread
                return CrosshairNormal;
            }

            var shotNumber = state.FirePatternCurrentShotNumber;
            if (shotNumber == 0)
            {
                return CrosshairNormal;
            }

            var initialSequenceLength = pattern.InitialSequence.Length;
            if (shotNumber < initialSequenceLength)
            {
                // initial fire sequence
                return CrosshairOffset1;
            }

            // cycled fire sequence
            return CrosshairOffset2;
        }
    }
}
namespace AtomicTorch.CBND.CoreMod.Systems.Cursor
{
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Items.Weapons;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ClientCursorWeaponSystem
    {
        // for 1080p/1440p screens or lower
        private static readonly CursorPreset Resolution1080p
            = new(normal: new CursorResource("1080p/cursor_crosshair_normal",
                                             pivotPosition: (16, 16)),
                  offset1: new CursorResource("1080p/cursor_crosshair_offset1",
                                              pivotPosition: (22, 22)),
                  offset2: new CursorResource("1080p/cursor_crosshair_offset2",
                                              pivotPosition: (32, 32)),
                  reloading: new CursorResource("1080p/cursor_crosshair_reloading",
                                                pivotPosition: (32, 32)));

        // for anything higher than 1440p
        private static readonly CursorPreset Resolution4K
            = new(normal: new CursorResource("4K/cursor_crosshair_normal",
                                             pivotPosition: (24, 24)),
                  offset1: new CursorResource("4K/cursor_crosshair_offset1",
                                              pivotPosition: (32, 32)),
                  offset2: new CursorResource("4K/cursor_crosshair_offset2",
                                              pivotPosition: (48, 48)),
                  reloading: new CursorResource("4K/cursor_crosshair_reloading",
                                                pivotPosition: (48, 48)));

        public static CursorResource CurrentCursorResource { get; set; }

        public static void Update()
        {
            CurrentCursorResource = GetCurrentCursorResource();
        }

        private static CursorResource GetCurrentCursorResource()
        {
            if (Api.Client.UI.GetVisualInPointedPosition() is not null)
            {
                return null;
            }

            var state = ClientCurrentCharacterHelper.PrivateState?.WeaponState;
            var protoWeapon = state?.ProtoWeapon;
            if (protoWeapon is not IProtoItemWeaponRanged)
            {
                return null;
            }

            var viewportSize = Api.Client.Rendering.ViewportSize;
            var preset = viewportSize.X > 2560
                         && viewportSize.Y > 1440
                             ? Resolution4K
                             : Resolution1080p;

            if ((state.WeaponReloadingState?.SecondsToReloadRemains ?? 0) > 0
                || state.ReadySecondsRemains > 0
                || !state.ProtoWeapon.SharedCanFire(ClientCurrentCharacterHelper.Character, state))

            {
                return preset.Reloading;
            }

            var pattern = protoWeapon.FirePatternPreset;
            if (!pattern.IsEnabled)
            {
                // no weapon fire spread
                return preset.Normal;
            }

            var shotNumber = state.FirePatternCurrentShotNumber;
            if (shotNumber == 0)
            {
                return preset.Normal;
            }

            var initialSequenceLength = pattern.InitialSequence.Length;
            if (shotNumber < initialSequenceLength)
            {
                // initial fire sequence
                return preset.Offset1;
            }

            // cycled fire sequence
            return preset.Offset2;
        }

        private readonly struct CursorPreset
        {
            public readonly CursorResource Normal;

            public readonly CursorResource Offset1;

            public readonly CursorResource Offset2;

            public readonly CursorResource Reloading;

            public CursorPreset(
                CursorResource normal,
                CursorResource offset1,
                CursorResource offset2,
                CursorResource reloading)
            {
                this.Normal = normal;
                this.Offset1 = offset1;
                this.Offset2 = offset2;
                this.Reloading = reloading;
            }
        }
    }
}
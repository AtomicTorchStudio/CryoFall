namespace AtomicTorch.CBND.CoreMod.Items.Weapons
{
    using AtomicTorch.CBND.CoreMod.Helpers.Client;
    using AtomicTorch.CBND.CoreMod.Systems.Weapons;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class WeaponGrenadeLauncherHelper
    {
        private static readonly IInputClientService ClientInput
            = Api.IsClient ? Api.Client.Input : null;

        public static Vector2D ClientGetCustomTargetPosition()
        {
            var character = ClientCurrentCharacterHelper.Character;
            var weaponState = ClientCurrentCharacterHelper.PrivateState.WeaponState;
            var targetPosition = ClientInput.MouseWorldPosition;

            if (ClientInput.IsKeyHeld(InputKey.Alt,        evenIfHandled: true)
                || ClientInput.IsKeyHeld(InputKey.Control, evenIfHandled: true))
            {
                // shoot in the direction as far as the range allows
                if (weaponState.WeaponCache is null)
                {
                    WeaponSystem.SharedRebuildWeaponCache(character, weaponState);
                }

                // ReSharper disable once PossibleNullReferenceException
                var rangeMax = weaponState.WeaponCache.RangeMax;
                var fromPosition = character.Position;
                var direction = targetPosition - fromPosition;
                targetPosition = fromPosition + direction.Normalized * rangeMax;
            }

            return targetPosition;
        }
    }
}
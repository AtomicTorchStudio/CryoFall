namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.WorldObjects.SoundCue
{
    using AtomicTorch.CBND.CoreMod.UI.Helpers;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.GameEngine.Common.Primitives;

    public static class ClientSoundCueManager
    {
        public static void OnSoundEvent(Vector2D soundWorldPosition, bool isFriendly)
        {
            var viewBounds = Api.Client.Rendering.WorldCameraCurrentViewWorldBounds;
            if (viewBounds.Contains(soundWorldPosition))
            {
                // no sound cue necessary
                return;
            }

            var control = ControlsCache<ControlSoundCue>.Instance.Pop();
            control.ShowAt(soundWorldPosition, viewBounds, isFriendly);
        }
    }
}
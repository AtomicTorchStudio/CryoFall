namespace AtomicTorch.CBND.CoreMod.Systems
{
    using System;
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Scripting;
    using AtomicTorch.CBND.GameApi.ServicesClient.Components;

    public class PublicActionStateWithTargetObjectSounds : BasePublicActionState
    {
        [NonSerialized]
        protected IComponentSoundEmitter soundEmitter;

        protected override void ClientOnCompleted()
        {
            this.DestroyProcessSoundEmitter();

            if (this.TargetWorldObject == null)
            {
                return;
            }

            var objectSoundPreset = this.SharedGetObjectSoundPreset();
            objectSoundPreset?.PlaySound(
                this.IsCancelled
                    ? ObjectSound.InteractFail
                    : ObjectSound.InteractSuccess,
                this.Character);
        }

        protected override void ClientOnStart()
        {
            if (this.TargetWorldObject == null)
            {
                return;
            }

            var objectSoundPreset = this.SharedGetObjectSoundPreset();
            if (objectSoundPreset is null)
            {
                return;
            }

            objectSoundPreset.PlaySound(ObjectSound.InteractStart, this.Character);

            var soundProcess = objectSoundPreset.GetSound(ObjectSound.InteractProcess);
            if (soundProcess != null)
            {
                this.soundEmitter = Api.Client.Audio.CreateSoundEmitter(
                    this.Character,
                    soundProcess,
                    isLooped: true);
            }
        }

        protected void DestroyProcessSoundEmitter()
        {
            this.soundEmitter?.Destroy();
            this.soundEmitter = null;
        }

        protected virtual ReadOnlySoundPreset<ObjectSound> SharedGetObjectSoundPreset()
        {
            return this.TargetWorldObject
                       .ProtoWorldObject
                       .SharedGetObjectSoundPreset();
        }
    }
}
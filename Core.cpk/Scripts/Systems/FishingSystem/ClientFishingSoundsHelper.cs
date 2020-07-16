namespace AtomicTorch.CBND.CoreMod.Systems.FishingSystem
{
    using AtomicTorch.CBND.CoreMod.SoundPresets;
    using AtomicTorch.CBND.GameApi.Data.Characters;
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ClientFishingSoundsHelper
    {
        private static readonly SoundResource SoundResourceBaiting
            = new SoundResource("Items/Fishing/Baiting");

        private static readonly ReadOnlySoundResourceSet SoundSetCancel
            = new SoundResourceSet()
              .Add("Items/Fishing/Cancel_")
              .ToReadOnly();

        private static readonly ReadOnlySoundResourceSet SoundSetFail
            = new SoundResourceSet()
              .Add("Items/Fishing/Fail_")
              .ToReadOnly();

        private static readonly ReadOnlySoundResourceSet SoundSetStart
            = new SoundResourceSet()
              .Add("Items/Fishing/Start_")
              .ToReadOnly();

        private static readonly ReadOnlySoundResourceSet SoundSetSuccess
            = new SoundResourceSet()
              .Add("Items/Fishing/Success_")
              .ToReadOnly();

        public static void PlaySoundBaiting(ICharacter character)
        {
            Api.Client.Audio.PlayOneShot(SoundResourceBaiting,
                                         character);
        }

        public static void PlaySoundCancel(ICharacter character)
        {
            Api.Client.Audio.PlayOneShot(SoundSetCancel.GetSound(),
                                         character);
        }

        public static void PlaySoundFail(ICharacter character)
        {
            Api.Client.Audio.PlayOneShot(SoundSetFail.GetSound(),
                                         character);
        }

        public static void PlaySoundStart(ICharacter character)
        {
            Api.Client.Audio.PlayOneShot(SoundSetStart.GetSound(),
                                         character);
        }

        public static void PlaySoundSuccess(ICharacter character)
        {
            Api.Client.Audio.PlayOneShot(SoundSetSuccess.GetSound(),
                                         character);
        }
    }
}
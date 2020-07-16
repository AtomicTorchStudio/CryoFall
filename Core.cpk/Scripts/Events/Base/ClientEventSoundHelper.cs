namespace AtomicTorch.CBND.CoreMod.Events.Base
{
    using AtomicTorch.CBND.GameApi.Resources;
    using AtomicTorch.CBND.GameApi.Scripting;

    public static class ClientEventSoundHelper
    {
        private static readonly SoundResource SoundResourceEventStarted
            = new SoundResource("Events/WorldEventStart");

        private static uint lastPlayedFrameNumber;

        public static void PlayEventStartedSound()
        {
            var frameNumber = Api.Client.Core.ClientFrameNumber;
            if (lastPlayedFrameNumber == frameNumber)
            {
                return;
            }

            lastPlayedFrameNumber = frameNumber;
            Api.Client.Audio.PlayOneShot(SoundResourceEventStarted, volume: 0.5f);
        }
    }
}
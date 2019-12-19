namespace AtomicTorch.CBND.CoreMod.Characters.Input
{
    using AtomicTorch.CBND.CoreMod.Characters.Player;
    using AtomicTorch.CBND.GameApi.Scripting.ClientComponents;
    using AtomicTorch.CBND.GameApi.Scripting.Network;
    using AtomicTorch.CBND.GameApi.ServicesClient;

    /// <summary>
    /// This class does automatic input sending
    /// (with redundancy to ensure that input will reach server with highest chance).
    /// </summary>
    public class ComponentPlayerInputSender : ClientComponent
    {
        private const double SendingIntervalSecondsFirstFiveTries = 0.025;

        private const double SendingIntervalSecondsNextTries = 0.050;

        private static readonly ICoreClientService Core = Client.Core;

        private CharacterInputUpdate lastInputData;

        private byte lastInputId;

        private bool lastInputIsServerAck;

        private int lastInputSentCount;

        private PlayerCharacter playerCharacterProto;

        private double timeToNextSending;

        public void OnServerAck(byte inputId)
        {
            if (inputId == this.lastInputId)
            {
                this.lastInputIsServerAck = true;
            }
        }

        public void Send(CharacterInputUpdate data)
        {
            this.lastInputId = ++this.lastInputId;
            this.lastInputData = data;
            this.lastInputSentCount = 0;
            this.lastInputIsServerAck = false;

            this.SendNow();
        }

        public void Setup(PlayerCharacter playerCharacterProto)
        {
            this.playerCharacterProto = playerCharacterProto;
        }

        public override void Update(double deltaTime)
        {
            // check if need to send now
            if (this.lastInputIsServerAck)
            {
                // already acknowledged
                return;
            }

            if (Core.ClientRealTime < this.timeToNextSending)
            {
                // it's too early - just sent another package
                return;
            }

            this.SendNow();
        }

        private void SendNow()
        {
            this.lastInputSentCount++;

            this.playerCharacterProto.CallServer(
                _ => _.ServerRemote_SetInput(this.lastInputData, this.lastInputId));

            var timeInterval = this.lastInputSentCount <= 5
                                   ? SendingIntervalSecondsFirstFiveTries
                                   : SendingIntervalSecondsNextTries;
            this.timeToNextSending = Core.ClientRealTime + timeInterval;
        }
    }
}
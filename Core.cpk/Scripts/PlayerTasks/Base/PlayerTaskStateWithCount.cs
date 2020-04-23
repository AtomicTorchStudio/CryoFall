namespace AtomicTorch.CBND.CoreMod.PlayerTasks
{
    using AtomicTorch.CBND.GameApi.Data.State;

    public class PlayerTaskStateWithCount : PlayerTaskState
    {
        [SyncToClient(DeliveryMode.ReliableSequenced, maxUpdatesPerSecond: 1)]
        public ushort CountCurrent { get; private set; }

        public void SetCountCurrent(int count, ushort countMax)
        {
            if (count <= 0)
            {
                this.CountCurrent = 0;
                return;
            }

            if (count >= countMax)
            {
                this.CountCurrent = countMax;
                return;
            }

            this.CountCurrent = (ushort)count;
        }
    }
}
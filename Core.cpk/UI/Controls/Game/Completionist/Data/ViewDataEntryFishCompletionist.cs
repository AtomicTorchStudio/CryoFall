namespace AtomicTorch.CBND.CoreMod.UI.Controls.Game.Completionist.Data
{
    using AtomicTorch.CBND.GameApi.Data;
    using AtomicTorch.GameEngine.Common.Client.MonoGame.UI;

    public class ViewDataEntryFishCompletionist : ViewDataEntryCompletionist
    {
        public ViewDataEntryFishCompletionist(
            IProtoEntity prototype,
            ActionCommandWithParameter commandClaimReward)
            : base(prototype,
                   commandClaimReward)
        {
        }

        public float MaxLength { get; set; }

        public string MaxLengthText => this.MaxLength.ToString("F2");

        public float MaxWeight { get; set; }

        public string MaxWeightText => this.MaxWeight.ToString("F2");
    }
}